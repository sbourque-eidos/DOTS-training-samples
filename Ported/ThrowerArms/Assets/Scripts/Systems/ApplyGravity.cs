
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;
using Unity.Collections;

public struct FreeFalling : IComponentData
{
}

[BurstCompile]
public struct ApplyGravitOnVelocityJob : IJobForEach<Velocity, FreeFalling>
{
    [ReadOnly] public float VelocityOffset;

    public void Execute(ref Velocity velocity, [ReadOnly] ref FreeFalling _)
    {
        velocity.Value.y += VelocityOffset;
    }
}

[BurstCompile]
public struct ApplyGravitOnPositionJob : IJobForEach<Translation, FreeFalling>
{
    [ReadOnly] public float PositionOffset;

    public void Execute(ref Translation translation, [ReadOnly] ref FreeFalling _)
    {
        translation.Value.y += PositionOffset;
    }
}

[UpdateBefore(typeof(ReserveBallSystem))]
[UpdateBefore(typeof(ReserveCanSystem))]
public class ApplyGravitySystem : JobComponentSystem
{
    const float Gravity = -9.81f;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        float deltaTime = Time.DeltaTime;
        float deltaTimeSquared = deltaTime * deltaTime;

        var applyGravityOnPositionJob = new ApplyGravitOnPositionJob
        {
            PositionOffset = 0.5f * Gravity * deltaTimeSquared
        };

        var applyGravityOnVelocityJob = new ApplyGravitOnVelocityJob
        {
            VelocityOffset = Gravity * deltaTime
        };

        JobHandle velocityHandle = applyGravityOnVelocityJob.Schedule(this, inputDeps);
        JobHandle positionHandle = applyGravityOnPositionJob.Schedule(this, inputDeps);

        return JobHandle.CombineDependencies(velocityHandle, positionHandle);
    }
}
