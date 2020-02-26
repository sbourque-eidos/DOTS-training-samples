using Unity.Entities;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraTiltSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraTiltData tilt) =>
        {
            tilt.TiltAngle = CameraUtils.ComputePeriodicValue(time, tilt.Amplitude, tilt.Period, tilt.Phase);
        }).Schedule();
    }
}
