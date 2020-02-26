using Unity.Entities;
using Unity.Mathematics;

public class CameraYawSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraYawData yaw) =>
        {
            float angularFrequency = (2.0f * math.PI) / yaw.Period;
            yaw.YawAngle = yaw.Amplitude * (float)math.sin((angularFrequency * time) + yaw.Phase);
        }).Schedule();
    }
}
