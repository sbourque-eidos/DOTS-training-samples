using Unity.Entities;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraPanSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraPanData pan) =>
        {
            pan.PanOffset = CameraUtils.ComputePeriodicValue(time, pan.Amplitude, pan.Period, pan.Phase);
        }).ScheduleParallel();
    }
}
