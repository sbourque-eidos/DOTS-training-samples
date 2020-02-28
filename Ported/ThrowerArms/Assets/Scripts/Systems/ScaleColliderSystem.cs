using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Jobs;

[UpdateBefore(typeof(DetectCollisionSystem))]
public class ScaleColliderSystem : SystemBase
{
    const float kSphereVolumeCoefficient = (4.0f / 3.0f) * math.PI;

    protected override void OnUpdate()
    {
        var massData = GetComponentDataFromEntity<Mass>(isReadOnly: false);

        var sphereHandle = Entities.ForEach((Entity entity, ref Sphere sphere, in NonUniformScale scale, in MassDensity density) =>
        {
            sphere.Radius = scale.Value.x * 0.5f;

            float r = sphere.Radius;

            Mass mass = massData[entity];
            mass.Value = kSphereVolumeCoefficient * (r * r * r) * density.Value;
            massData[entity] = mass;
        }).WithAll<IncreaseScaleData, Mass>().WithNone<Cylinder>().WithNativeDisableContainerSafetyRestriction(massData).ScheduleParallel(Dependency);

        var cylinderHandle = Entities.ForEach((Entity entity, ref Cylinder cylinder, in NonUniformScale scale, in MassDensity density) =>
        {
            cylinder.Length = scale.Value.y;
            cylinder.Radius = scale.Value.x * 0.5f;

            float r = cylinder.Radius;

            Mass mass = massData[entity];
            mass.Value = math.PI * cylinder.Length * (r * r) * density.Value;
            massData[entity] = mass;
        }).WithAll<IncreaseScaleData, Mass>().WithNone<Sphere>().WithNativeDisableContainerSafetyRestriction(massData).ScheduleParallel(Dependency);

        Dependency = JobHandle.CombineDependencies(sphereHandle, cylinderHandle);
    }
}
