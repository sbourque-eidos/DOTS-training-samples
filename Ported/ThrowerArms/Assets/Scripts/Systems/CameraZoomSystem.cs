using Unity.Entities;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraZoomSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraZoomData zoom) =>
        {
            zoom.Zoom = CameraUtils.ComputePeriodicValue(time, zoom.Amplitude, zoom.Period, zoom.Phase);
        }).ScheduleParallel();
    }
}
