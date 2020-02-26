using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class SpawnConstantlySystem : SystemBase
{
    const float k_IncreaseSpeed = 1.0f;

    EndInitializationEntityCommandBufferSystem m_EntityCommandBufferSystem;
    protected override void OnCreate()
    {
        base.OnCreate();

        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_EntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent();
        var baseSeed = (uint)UnityEngine.Time.frameCount;

        Entities.ForEach((Entity entity, int entityInQueryIndex, in ConstantSpawner spawner, in CurrentCount currentCount) =>
        {
            var random = new Random(baseSeed + (uint)entityInQueryIndex);
            random.NextFloat(); // For "real" random because the seed is used directly otherwise
            var amountToSpawn = spawner.TargetCount - currentCount.Value;

            for (int i = 0; i < amountToSpawn; ++i)
            {
                var randomPosition = random.NextFloat3(spawner.MinArea, spawner.MaxArea);
                var randomScalePercent = random.NextFloat(0,1);
                float3 randomScale = spawner.MinScale + randomScalePercent * (spawner.MaxScale - spawner.MinScale);
                var newEntity = ecb.Instantiate(entityInQueryIndex, spawner.Prefab);
                ecb.SetComponent(entityInQueryIndex, newEntity, new Translation { Value = randomPosition });
                ecb.AddComponent(entityInQueryIndex, newEntity, new Velocity { Value = spawner.InitialVelocity });
                ecb.AddComponent<NonUniformScale>(entityInQueryIndex, newEntity);
                ecb.AddComponent(entityInQueryIndex, newEntity
                    , new IncreaseScaleData
                    {
                        Speed = k_IncreaseSpeed,
                        TargetScale = randomScale,
                    });
                ecb.AddComponent(entityInQueryIndex, newEntity, new KillableData
                {
                    TargetKillPlane = spawner.DeathPlane
                });
            }

        }).ScheduleParallel();

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}