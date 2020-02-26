using Unity.Entities;
using Unity.Mathematics;

[UpdateBefore(typeof(ApplyCameraTransformation))]
public class CameraPanSystem : SystemBase
{
    protected override void OnUpdate()
    {
        double time = Time.ElapsedTime;
        Entities.ForEach((ref CameraPanData pan) =>
        {
            float angularFrequency = (2.0f * math.PI) / pan.Period;
            pan.PanOffset = pan.Amplitude * (float)math.sin((angularFrequency * time) + pan.Phase);
        }).Schedule();
    }
}
