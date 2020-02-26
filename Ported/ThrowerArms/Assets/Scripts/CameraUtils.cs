using Unity.Mathematics;

public static class CameraUtils
{
    public static float ComputePeriodicValue(double time, float amplitude, float period, float phase)
    {
        float angularFrequency = (2.0f * math.PI) / period;
        return amplitude * (float)math.sin((angularFrequency * time) + phase);
    }
}
