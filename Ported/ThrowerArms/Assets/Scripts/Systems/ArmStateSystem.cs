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

        //var targetingBallTagData = GetComponentDataFromEntity<TargetingBallTag>(isReadOnly: true);
        //var targetingCanTagData = GetComponentDataFromEntity<TargetingCanTag>(isReadOnly: true);
        var targetData = GetComponentDataFromEntity<Target>(isReadOnly: true);
        var translationData = GetComponentDataFromEntity<Translation>();

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
                    if (targetData.Exists(entity))
                    {
                        Target target = targetData[entity];

                        state.CurrentState = ArmStateData.State.PickingUp;
                        state.Cooldown = 2.0f;
                        translationData[target.Value] = new Translation { Value = transform.Position };
                        ecb.RemoveComponent<Velocity>(entityInQueryIndex, target.Value);
                    }
                    break;
                case ArmStateData.State.PickingUp:
                    state.Cooldown -= dt;
                    if (state.Cooldown <= 0.0f)
                    {
                        Target target = targetData[entity];
                        state.CurrentState = ArmStateData.State.WindingUp;
                        state.Cooldown = 2.0f;
                        translationData[target.Value] = new Translation { Value = transform.Position + new float3(0.0f, 2.0f, 0.0f) };
                    }
                    break;
                case ArmStateData.State.WindingUp:
                    state.Cooldown -= dt;
                    if (state.Cooldown <= 0.0f)
                    {
                        Target target = targetData[entity];
                        state.CurrentState = ArmStateData.State.RequestingBallTargeting;
                        ecb.AddComponent(entityInQueryIndex, target.Value, new Velocity { Value = new float3(0.0f, 4.0f, 4.0f) });
                        ecb.AddComponent<FreeFalling>(entityInQueryIndex, target.Value);
                        ecb.AddComponent(entityInQueryIndex, target.Value, new KillableData() { TargetKillPlane = groundEntity });
                        //ecb.AddComponent<TargetingCanTag>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<Target>(entityInQueryIndex, entity);
                    }
                    break;
                case ArmStateData.State.TargetingCan:
                    break;
                case ArmStateData.State.Throwing:
                    break;
            }
        })
        //.WithReadOnly(targetingBallTagData)
        //.WithReadOnly(targetingCanTagData)
        .WithReadOnly(targetData)
        .WithNativeDisableParallelForRestriction(translationData)
        .ScheduleParallel(Dependency);

        Dependency = handle;
        m_EcbSystem.AddJobHandleForProducer(Dependency);
    }
}
