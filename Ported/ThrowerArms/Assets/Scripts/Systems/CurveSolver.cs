
using Unity.Mathematics;

public static class CurveSolver
{
    public static float3 Gravity = new float3(0.0f, -9.81f, 0.0f);

    public static float3 Solve(float3 posInitTarget, float3 velTarget, float3 startPosition, float time)
    {
        // target has constant velocity and projectile is only affected by gravity
        // target = posInitTarget + velTarget * time
        // ball = startPosition + velocityToCompute * time + gravity / 2 * time^2
        // Say that target and ball must be equal, set a desired time and solve

        float3 startOffset = posInitTarget - startPosition;
        float3 velocity = startOffset / time + velTarget - 0.5f * Gravity * time;
        return velocity;
    }
}