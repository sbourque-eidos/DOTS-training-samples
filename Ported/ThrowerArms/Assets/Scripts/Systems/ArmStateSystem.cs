using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ArmStateSystem : SystemBase
{
    struct Ground : IComponentData
    {
    }

    EntityCommandBufferSystem m_EcbSystem;

    protected override void OnCreate()
    {
        m_EcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        var ground = EntityManager.CreateEntity(typeof(Ground));
        EntityManager.AddComponentData(ground, new Plane()
        {
            Direction = new float3(0.0f, 1.0f, 0.0f),
            Distance = -5.0f
        });
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        var groundEntity = GetSingletonEntity<Ground>();
        EntityManager.DestroyEntity(groundEntity);
    }

    protected override void OnUpdate()
    {
        var ecb = m_EcbSystem.CreateCommandBuffer().ToConcurrent();

        var targetBallData = GetComponentDataFromEntity<TargetBall>(isReadOnly: true);
        var targetCanData = GetComponentDataFromEntity<TargetCan>(isReadOnly: true);
        var translationData = GetComponentDataFromEntity<Translation>();

        var velocityData = GetComponentDataFromEntity<Velocity>(isReadOnly: true);
        var localToWorldData = GetComponentDataFromEntity<LocalToWorld>(isReadOnly: true);

        var groundEntity = GetSingletonEntity<Ground>();
        var dt = Time.DeltaTime;

        var handle = Entities.ForEach((Entity entity, int entityInQueryIndex, ref ArmStateData state, in LocalToWorld transform) =>
        {
            switch (state.CurrentState)
            {
                case ArmStateData.State.RequestingBallTargeting:
                    ecb.AddComponent<TargetingBallTag>(entityInQueryIndex, entity);
                    state.CurrentState = ArmStateData.State.TargetingBall;
                    break;
                case ArmStateData.State.TargetingBall:
                    if (targetBallData.Exists(entity))
                    {
                        TargetBall target = targetBallData[entity];

                        state.CurrentState = ArmStateData.State.PickingUp;
                        state.Cooldown = 2.0f;
                        translationData[target.Value] = new Translation { Value = transform.Position };
                        ecb.RemoveComponent<Velocity>(entityInQueryIndex, target.Value);

                        var targetIK = new IKTarget
                        {
                            Target = transform.Position,
                            Timer = state.Cooldown,
                            CurrentTime = 0.0f
                        };

                        ecb.AddComponent<IKTarget>(0, entity, targetIK);
                    }
                    break;
                case ArmStateData.State.PickingUp:
                    state.Cooldown -= dt;
                    if (state.Cooldown <= 0.0f)
                    {
                        state.CurrentState = ArmStateData.State.TargetingCan;
                        ecb.AddComponent<TargetingCanTag>(entityInQueryIndex, entity);
                    }
                    break;
                case ArmStateData.State.TargetingCan:
                    if (targetCanData.Exists(entity))
                    {
                        TargetBall target = targetBallData[entity];
                        state.Cooldown = 2.0f;

                        float3 newPosition = transform.Position + new float3(0.0f, 2.0f, 0.0f);
                        translationData[target.Value] = new Translation { Value = newPosition };
                        state.CurrentState = ArmStateData.State.WindingUp;

                        var targetIK = new IKTarget
                        {
                            Target = transform.Position,
                            Timer = state.Cooldown,
                            CurrentTime = 0.0f
                        };

                        ecb.SetComponent<IKTarget>(0, entity, targetIK);
                    }
                    break;
                case ArmStateData.State.WindingUp:
                    state.Cooldown -= dt;
                    if (state.Cooldown <= 0.0f)
                    {
                        state.CurrentState = ArmStateData.State.Throwing;
                    }
                    ecb.RemoveComponent<IKTarget>(0, entity);
                    break;
                case ArmStateData.State.Throwing:
                    TargetBall ball = targetBallData[entity];
                    TargetCan can = targetCanData[entity];
                    ecb.AddComponent(entityInQueryIndex, ball.Value, new Target { Value = can.Value });

                    float3 canPosition = localToWorldData[can.Value].Position;
                    float3 canVelocity = velocityData[can.Value].Value;

                    float3 ballPosition = localToWorldData[ball.Value].Position;
                    float3 ballVelocity = CurveSolver.Solve(canPosition, canVelocity, ballPosition, 1.0f);

                    ecb.AddComponent(entityInQueryIndex, ball.Value, new Velocity { Value = ballVelocity });
                    ecb.AddComponent<FreeFalling>(entityInQueryIndex, ball.Value);
                    ecb.RemoveComponent<TargetBall>(entityInQueryIndex, entity);
                    ecb.RemoveComponent<TargetCan>(entityInQueryIndex, entity);
                    ecb.AddComponent(entityInQueryIndex, ball.Value, new KillableData { TargetKillPlane = groundEntity });
                    ecb.AddComponent(entityInQueryIndex, can.Value, new KillableData { TargetKillPlane = groundEntity });

                    state.CurrentState = ArmStateData.State.RequestingBallTargeting;
                    break;
            }
        })
        .WithReadOnly(targetBallData)
        .WithReadOnly(targetCanData)
        .WithReadOnly(velocityData)
        .WithReadOnly(localToWorldData)
        .WithNativeDisableParallelForRestriction(translationData)
        .ScheduleParallel(Dependency);

        Dependency = handle;
        m_EcbSystem.AddJobHandleForProducer(Dependency);
    }
}
