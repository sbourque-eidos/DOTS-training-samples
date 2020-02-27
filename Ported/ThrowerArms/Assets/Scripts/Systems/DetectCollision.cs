﻿
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;

[BurstCompile]
struct DetectCollisionJob : IJobForEachWithEntity<Sphere, Target>
{
    [WriteOnly] public NativeQueue<ContactPoint>.ParallelWriter Contacts;
    [ReadOnly] public ComponentDataFromEntity<Cylinder> Cylinders;
    [ReadOnly] public ComponentDataFromEntity<Translation> Positions;
    [ReadOnly] public ComponentDataFromEntity<Rotation> Rotations;

    public void Execute(Entity entity, int _, ref Sphere sphere, [ReadOnly] ref Target target)
    {
        Entity targetEntity = target.Value;

        Cylinder   cylinder         = Cylinders[targetEntity];
        float3     cylinderPosition = Positions[targetEntity].Value;
        float3     spherePosition   = Positions[entity].Value;
        quaternion cylinderRotation = Rotations[targetEntity].Value;

        if (CollisionManager.CollideSphereCylinder(
            cylinderRotation,
            cylinderPosition,
            cylinder.Length,
            cylinder.Radius,
            spherePosition,
            sphere.Radius))
        {
            float3 hitPosition = 0.5f * (cylinderPosition + spherePosition);

            var contact = new ContactPoint
            {
                EntityA = entity,
                EntityB = targetEntity,
                Position = hitPosition
            };

            Contacts.Enqueue(contact);
        }
    }
}

public class DetectCollisionSystem : JobComponentSystem
{
    public static NativeQueue<ContactPoint> Contacts;
    public static JobHandle Handle;

    protected override void OnCreate()
    {
        Contacts = new NativeQueue<ContactPoint>(Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        Contacts.Dispose();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var cylinders    = GetComponentDataFromEntity<Cylinder>(true);
        var translations = GetComponentDataFromEntity<Translation>(true);
        var rotations    = GetComponentDataFromEntity<Rotation>(true);

        var job = new DetectCollisionJob()
        {
            Cylinders = cylinders,
            Positions = translations,
            Rotations = rotations,
            Contacts = Contacts.AsParallelWriter()
        };

        Handle = job.Schedule(this, inputDependencies);
        return Handle;
    }
}