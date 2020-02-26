using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public class IncreaseScaleSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem m_ecb;
    protected override void OnCreate()
    {
        base.OnCreate();
        m_ecb = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = m_ecb.CreateCommandBuffer().ToConcurrent();

        var dt = UnityEngine.Time.deltaTime;

        Entities.ForEach((
                Entity e,
                int entityInQueryIndex,
                ref IncreaseScaleData increaseScaleData,
                ref NonUniformScale nonUniformScale) =>
            {
                //compute new scale
                float newPercentageReached = increaseScaleData.PercentReached + (increaseScaleData.Speed * dt);
                newPercentageReached = math.clamp(newPercentageReached, 0, 1);

                float newScale = newPercentageReached * increaseScaleData.TargetScale;

                //update scale values
                nonUniformScale.Value = newScale;
                increaseScaleData.PercentReached = newPercentageReached;

                if(newScale == increaseScaleData.TargetScale)
                {
                    //remove component
                    ecb.RemoveComponent<IncreaseScaleData>(entityInQueryIndex, e);
                }
            })
            .ScheduleParallel();

        m_ecb.AddJobHandleForProducer(Dependency);
    }
}