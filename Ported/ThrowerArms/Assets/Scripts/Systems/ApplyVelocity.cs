
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public struct ApplyVelocityJob : IJobForEach<Translation, Velocity>
{
    public float DeltaTime;

    public void Execute(ref Translation translation, [ReadOnly] ref Velocity velocity)
    {
        translation.Value += velocity.Value * DeltaTime;
    }
}

[BurstCompile]
public struct ApplyAngularVelocityJob : IJobForEach<Rotation, AngularVelocity>
{
    public float DeltaTime;

    public void Execute(ref Rotation rotation, [ReadOnly] ref AngularVelocity velocity)
    {
        quaternion offsetRot = quaternion.Euler(velocity.Value * DeltaTime);
        rotation.Value = math.mul(rotation.Value, offsetRot);
    }
}

[UpdateBefore(typeof(ReserveBallSystem))]
[UpdateBefore(typeof(ReserveCanSystem))]
[UpdateAfter(typeof(ApplyGravitySystem))]
public class ApplyVelocitySystem : JobComponentSystem
{
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var applyVelocityJob = new ApplyVelocityJob
        {
            DeltaTime = Time.DeltaTime
        };

        var applyAngularVelocity = new ApplyAngularVelocityJob
        {
            DeltaTime = Time.DeltaTime
        };

        JobHandle handle = applyVelocityJob.Schedule(this, inputDeps);
        JobHandle handleAngular = applyAngularVelocity.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(handle, handleAngular);
    }
}
