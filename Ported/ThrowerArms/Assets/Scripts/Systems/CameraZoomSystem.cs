using Unity.Entities;
using Unity.Mathematics;

public class CameraZoomSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraZoomData zoom) =>
        {
            float angularFrequency = (2.0f * math.PI) / zoom.Period;
            zoom.Zoom = zoom.Amplitude * (float)math.sin((angularFrequency * time) + zoom.Phase);
        }).Schedule();
    }
}
