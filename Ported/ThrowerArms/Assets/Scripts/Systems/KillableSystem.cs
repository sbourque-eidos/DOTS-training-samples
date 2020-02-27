using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class KillableSystem : SystemBase
{
    EntityCommandBufferSystem m_EcbSystem;

    protected override void OnCreate()
    {
        m_EcbSystem = World.GetOrCreateSystem<EndInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var sphereEcb = m_EcbSystem.CreateCommandBuffer().ToConcurrent();
        var cylinderEcb = m_EcbSystem.CreateCommandBuffer().ToConcurrent();

        var planeData = GetComponentDataFromEntity<Plane>(isReadOnly: true);

        var sphereHandle = Entities.ForEach((Entity entity, int entityInQueryIndex, in KillableData killableData,
            in LocalToWorld transform, in Sphere sphere) =>
        {
            Entity killPlaneEntity = killableData.TargetKillPlane;
            if (planeData.Exists(killPlaneEntity))
            {
                Plane killPlane = planeData[killPlaneEntity];
                if (CollisionManager.CollideSpherePlane(transform.Position, sphere.Radius, killPlane.Direction, killPlane.Distance))
                {
                    sphereEcb.DestroyEntity(entityInQueryIndex, entity);
                }
            }
        }).WithReadOnly(planeData).ScheduleParallel(Dependency);

        var cylinderHandle = Entities.ForEach((Entity entity, int entityInQueryIndex, in KillableData killableData,
            in LocalToWorld transform, in Cylinder cylinder) =>
        {
            Entity killPlaneEntity = killableData.TargetKillPlane;
            if (planeData.Exists(killPlaneEntity))
            {
                Plane killPlane = planeData[killPlaneEntity];
                if (CollisionManager.CollideCylinderPlane(transform.Rotation, transform.Position, cylinder.Length, cylinder.Radius, killPlane.Direction, killPlane.Distance))
                {
                    cylinderEcb.DestroyEntity(entityInQueryIndex, entity);
                }
            }
        }).WithReadOnly(planeData).ScheduleParallel(sphereHandle);

        Dependency = cylinderHandle;
        m_EcbSystem.AddJobHandleForProducer(Dependency);
    }
}
