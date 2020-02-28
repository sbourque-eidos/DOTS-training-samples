using Unity.Entities;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SpawnUniformlySystem : SystemBase
{
    EndInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();

        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();

        Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, in UniformSpawner spawner) =>
        {
            var interval = (spawner.Max - spawner.Min) / (spawner.Count - 1);
            for (int i = 0; i < spawner.Count; ++i)
            {
                var position = spawner.Min + interval * i;
                var newEntity = ecb.Instantiate(entityInQueryIndex, spawner.Prefab);
                ecb.SetComponent(entityInQueryIndex, newEntity, new Translation { Value = position });
            }
            ecb.RemoveComponent<UniformSpawner>(entityInQueryIndex, entity);
        }).ScheduleParallel();

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}   