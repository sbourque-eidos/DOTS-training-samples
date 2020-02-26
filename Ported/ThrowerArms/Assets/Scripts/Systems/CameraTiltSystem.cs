using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraTiltSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraTiltData tilt) =>
        {
            float angularFrequency = (2.0f * math.PI) / tilt.Period;
            tilt.TiltAngle = tilt.Amplitude * (float)math.sin(angularFrequency * time);
        }).Schedule();
    }
}
