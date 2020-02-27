using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class ReserveCanSystem : SystemBase
{
    const float k_AnticipateTime = 1.0f;    // In seconds
    const float k_MaximumDistance = 10.0f;

    // Key: anticipated x position rounded down, Value: can (one per int pos max)
    NativeHashMap<int, Entity> m_SortedCans;

    // Key: Can, Value: Arm
    NativeHashMap<Entity, Entity> m_IntentedCans;

    EndSimulationEntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate()
    {
        base.OnCreate();

        m_SortedCans = new NativeHashMap<int, Entity>(1024, Allocator.Persistent);
        m_IntentedCans = new NativeHashMap<Entity, Entity>(1024, Allocator.Persistent);

        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        m_SortedCans.Dispose();
        m_IntentedCans.Dispose();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer();
        var sortedCans = m_SortedCans;
        var intendedCans = m_IntentedCans;

        var clearSortedCansHandle = Job.WithCode(() =>
        {
            sortedCans.Clear();
        }).Schedule(Dependency);

        var clearIntendedCansHandle = Job.WithCode(() =>
        {
            intendedCans.Clear();
        }).Schedule(Dependency);

        var sortHandle = Entities
            .WithAll<CanTag>()
            .WithNone<FreeFalling>()
            .WithNone<Parent>()
            .WithNone<TargetedTag>()
            .ForEach((Entity entity, in Translation translation, in Velocity velocity) =>
        {
            var key = (int)translation.Value.x;
            sortedCans.TryAdd(key, entity);
        }).Schedule(clearSortedCansHandle);

        var intendedCansParallel = intendedCans.AsParallelWriter();

        var baseSeed = (uint)UnityEngine.Time.frameCount;
        var random = new Random(baseSeed);
        random.NextBool();

        // Write intentions
        var intentionHandle = Entities
            .WithReadOnly(sortedCans)
            .WithAll<TargetingCanTag>()
            .WithNone<Target>()
            .ForEach((Entity entity, in Translation translation) =>
        {
            var minPosition = translation.Value.x - k_MaximumDistance;
            var maxPosition = translation.Value.x - k_MaximumDistance;
            var min = (int)minPosition;
            var max = (int)(maxPosition + 0.5f);

            var initialAttempt = random.NextInt(min, max);

            if (sortedCans.TryGetValue(initialAttempt, out var canEntity))
            {
                if (intendedCansParallel.TryAdd(canEntity, entity))
                    return;
            }

            for (int i = min; i <= max; ++i)
            {
                if (sortedCans.TryGetValue(i, out canEntity))
                {
                    if (intendedCansParallel.TryAdd(canEntity, entity))
                        return;
                }
            }
        }).ScheduleParallel(JobHandle.CombineDependencies(sortHandle
            , clearIntendedCansHandle));

        // Resolve intentions
        var resolvedHandle = Job
            .WithCode(() =>
        {
            var keyValues = intendedCans.GetKeyValueArrays(Allocator.Temp);
            var cans = keyValues.Keys;
            var arms = keyValues.Values;

            for (int i = 0; i < cans.Length; ++i)
            {
                var canEntity = cans[i];
                var armEntity = arms[i];
               
                ecb.AddComponent<TargetedTag>(canEntity);
                ecb.RemoveComponent<KillableData>(canEntity);
                ecb.AddComponent(armEntity, new Target { Value = canEntity });
                ecb.RemoveComponent<TargetingCanTag>(armEntity);

            }
        }).Schedule(intentionHandle);

        Dependency = resolvedHandle;

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}