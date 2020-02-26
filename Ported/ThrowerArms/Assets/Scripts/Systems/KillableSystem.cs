using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

public class KillableSystem : SystemBase
{
    EntityCommandBufferSystem m_EcbSystem;

    protected override void OnCreate()
    {
        m_EcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var sphereEcb = m_EcbSystem.CreateCommandBuffer().ToConcurrent();
        var cylinderEcb = m_EcbSystem.CreateCommandBuffer().ToConcurrent();

        var planeData = GetComponentDataFromEntity<Plane>(isReadOnly: true);

        var sphereHandle = Entities.ForEach((Entity entity, int entityInQueryIndex, in KillableData killableData,
            in LocalToWorld transform) =>
        {
            Entity killPlaneEntity = killableData.TargetKillPlane;
            if (planeData.Exists(killPlaneEntity))
            {
                Plane killPlane = planeData[killPlaneEntity];
                if (CollisionManager.CollideSpherePlane(transform.Position, 1.0f, killPlane.Direction, killPlane.Distance))
                {
                    sphereEcb.DestroyEntity(entityInQueryIndex, entity);
                }
            }
        }).WithAll<BallTag>().WithReadOnly(planeData).ScheduleParallel(Dependency);

        var cylinderHandle = Entities.ForEach((Entity entity, int entityInQueryIndex, in KillableData killableData,
            in LocalToWorld transform) =>
        {
            Entity killPlaneEntity = killableData.TargetKillPlane;
            if (planeData.Exists(killPlaneEntity))
            {
                Plane killPlane = planeData[killPlaneEntity];
                if (CollisionManager.CollideCylinderPlane(transform.Rotation, transform.Position, 2.0f, 1.0f, killPlane.Direction, killPlane.Distance))
                {
                    cylinderEcb.DestroyEntity(entityInQueryIndex, entity);
                }
            }
        }).WithAll<CanTag>().WithReadOnly(planeData).ScheduleParallel(sphereHandle);

        Dependency = cylinderHandle;
        m_EcbSystem.AddJobHandleForProducer(Dependency);
    }
}
