using Unity.Entities;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraYawSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraYawData yaw) =>
        {
            yaw.YawAngle = CameraUtils.ComputePeriodicValue(time, yaw.Amplitude, yaw.Period, yaw.Phase);
        }).ScheduleParallel();
    }
}
