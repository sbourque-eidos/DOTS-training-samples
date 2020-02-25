
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;

[BurstCompile]
public class ApplyVelocityJob : IJobForEach<Translation, Velocity>
{
    public float DeltaTime;

    public void Execute(ref Translation translation, ref Velocity velocity)
    {
        translation.Value += velocity.Value * DeltaTime;
    }
}

public class ApplyVelocitySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var applyVelocityJob = new ApplyVelocityJob
        {
            DeltaTime = Time.DeltaTime
        };

        JobHandle handle = applyVelocityJob.Schedule(inputDeps);
        return handle;
    }
}