using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(DetectCollisionSystem))]
public class ScaleColliderSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sphereHandle = Entities.ForEach((ref Sphere sphere, ref Mass mass, in NonUniformScale scale, in MassDensity density) =>
        {
            sphere.Radius = scale.Value.x * 0.5f;
            mass.Value = (4.0f / 3.0f) * math.PI * math.pow(sphere.Radius, 3) * density.Value;
        }).ScheduleParallel(Dependency);

        var cylinderHandle = Entities.ForEach((ref Cylinder cylinder, ref Mass mass, in NonUniformScale scale, in MassDensity density) =>
        {
            cylinder.Length = scale.Value.y;
            cylinder.Radius = scale.Value.x * 0.5f;
            mass.Value = math.PI * cylinder.Length * math.pow(cylinder.Radius, 2.0f) * density.Value;
        }).ScheduleParallel(sphereHandle);

        Dependency = cylinderHandle;
    }
}
