using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class ApplyCameraTransformation : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref Translation translation, ref Rotation rotation, in CameraOriginData origin,
            in CameraPanData pan, in CameraTiltData tilt, in CameraYawData yawData, in CameraZoomData zoomData) =>
        {
            float pivotDistance = origin.PivotDistance + zoomData.Zoom;

            float pitch = math.radians(tilt.TiltAngle);
            float yaw = math.radians(yawData.YawAngle);

            float xOffset = pan.PanOffset + (pivotDistance * math.sin(yaw));
            float yOffset = pivotDistance * math.tan(pitch);
            float zOffset = -(pivotDistance * math.cos(yaw));

            translation.Value = origin.OriginalPosition;
            translation.Value.x += xOffset;
            translation.Value.y += yOffset;
            translation.Value.z += zOffset;

            rotation.Value = quaternion.EulerXYZ(pitch, yaw, 0.0f);
        }).Schedule();
    }
}
