using Unity.Entities;
using Unity.Transforms;

[UpdateBefore(typeof(DetectCollisionSystem))]
[UpdateBefore(typeof(KillableSystem))]
public class ScaleColliderSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sphereHandle = Entities.ForEach((ref Sphere sphere, in NonUniformScale scale) =>
        {
            sphere.Radius = scale.Value.x * 0.5f;
        }).ScheduleParallel(Dependency);

        var cylinderHandle = Entities.ForEach((ref Cylinder cylinder, in NonUniformScale scale) =>
        {
            cylinder.Length = scale.Value.y;
            cylinder.Radius = scale.Value.x * 0.5f;
        }).ScheduleParallel(sphereHandle);

        Dependency = cylinderHandle;
    }
}
