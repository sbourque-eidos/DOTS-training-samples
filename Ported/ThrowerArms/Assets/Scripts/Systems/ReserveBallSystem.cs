using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ReserveBallSystem : SystemBase
{
    const float k_AnticipateTime = 1.0f;    // In seconds
    const float k_MaximumDistance = 3.0f;

    // Key: anticipated x position rounded down, Value: ball (one per int pos max)
    NativeHashMap<int, Entity> m_SortedBalls;

    // Key: Ball, Value: Arm
    NativeHashMap<Entity, Entity> m_IntentedBalls;

    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_SortedBalls = new NativeHashMap<int, Entity>(1024, Allocator.Persistent);
        m_IntentedBalls = new NativeHashMap<Entity, Entity>(1024, Allocator.Persistent);

        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_SortedBalls.Dispose();
        m_IntentedBalls.Dispose();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
        var sortedBalls = m_SortedBalls;
        var intendedBalls = m_IntentedBalls;

        var clearSortedBallsHandle = Job.WithCode(() =>
        {
            sortedBalls.Clear();
        }).Schedule(Dependency);

        var clearIntendedBallsHandle = Job.WithCode(() =>
        {
            intendedBalls.Clear();
        }).Schedule(Dependency);

        var sortHandle = Entities
            .WithAll<BallTag>()
            .WithNone<FreeFalling>()
            .WithNone<Parent>()
            .WithNone<TargetedTag>()
            .ForEach((Entity entity, in Translation translation, in Velocity velocity) =>
        {
            var anticipatedPosition = translation.Value.x + velocity.Value.x * k_AnticipateTime;
            var key = (int)anticipatedPosition;
            sortedBalls.TryAdd(key, entity);
        }).Schedule(clearSortedBallsHandle);


        var translations = GetComponentDataFromEntity<Translation>(true);

        var intendedBallsParallel = intendedBalls.AsParallelWriter();

        // Write intentions
        var intentionHandle = Entities
            .WithReadOnly(sortedBalls)
            .WithReadOnly(translations)
            .WithAll<TargetingBallTag>()
            .WithNone<Target>()
            .ForEach((Entity entity, in Translation translation) =>
        {
            var minPosition = translation.Value.x - k_MaximumDistance;
            var maxPosition = translation.Value.x - k_MaximumDistance;
            var min = (int)minPosition;
            var max = (int)(maxPosition + 0.5f);

            for (int i = min; i <= max; ++i)
            {
                if (sortedBalls.TryGetValue(i, out var ballEntity))
                {
                    var ballPosition = translations[ballEntity];
                    if (math.abs(translation.Value.x - ballPosition.Value.x) <= k_MaximumDistance)
                    {
                        if (intendedBallsParallel.TryAdd(ballEntity, entity))
                            return;
                    }
                }
            }
        }).ScheduleParallel(JobHandle.CombineDependencies(sortHandle
            , clearIntendedBallsHandle));

        // Resolve intentions
        var resolvedHandle = Job
            .WithCode(() =>
        {
            var keyValues = intendedBalls.GetKeyValueArrays(Allocator.Temp);
            var balls = keyValues.Keys;
            var arms = keyValues.Values;

            for (int i = 0; i < balls.Length; ++i)
            {
                var ballEntity = balls[i];
                var armEntity = arms[i];
               
                ecb.AddComponent<TargetedTag>(ballEntity);
                ecb.RemoveComponent<KillableData>(ballEntity);
                ecb.AddComponent(armEntity, new Target { Value = ballEntity });
                ecb.RemoveComponent<TargetingBallTag>(armEntity);

            }
        }).Schedule(intentionHandle);

        Dependency = resolvedHandle;

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}