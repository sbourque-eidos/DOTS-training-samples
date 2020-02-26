
using Unity.Entities;
using Unity.Mathematics;

public struct ContactPoint : IComponentData
{
    public Entity EntityA;
    public Entity EntityB;
}

public static class CollisionManager
{
    public static void SegmentToPoints(quaternion rotation, float3 center, float length, out float3 A, out float3 B)
    {
        float3 halfLength = math.rotate(rotation, new float3(0.0f, 0.5f * length, 0.0f));
        A = center + halfLength;
        B = center - halfLength;
    }

    public static float ClosestPointNormalizedDistance(float3 A, float3 B, float3 P)
    {
        float3 AP = P - A;
        float3 AB = B - A;

        float projectionMagnitude = math.dot(AP, AB);

        // lets assume length is never null
        float t = projectionMagnitude / math.lengthsq(AB);
        return math.clamp(t, 0.0f, 1.0f);
    }

    public static bool CollideSpherePlane(
        float3 spherePosition,
        float sphereRadius,
        float3 planeDirection,
        float planeDistance)
    {
        float sphereDistanceToCenter = math.dot(spherePosition, planeDirection);
        float sphereDistanceToPlane = sphereDistanceToCenter - planeDistance;
        return sphereDistanceToPlane > sphereRadius;
    }

    public static bool CollideCylinderPlane(
        quaternion cylinderRotation,
        float3 cylinderPosition,
        float cylinderLength,
        float cylinderRadius,
        float3 planeDirection,
        float planeDistance)
    {
        SegmentToPoints(cylinderRotation, cylinderPosition, cylinderLength, out float3 A, out float3 B);

        return CollideSpherePlane(A, cylinderRadius, planeDirection, planeDistance) ||
            CollideSpherePlane(B, cylinderRadius, planeDirection, planeDistance);
    }

    public static bool CollideSphereCylinder(
        quaternion cylinderRotation,
        float3 cylinderPosition,
        float cylinderLength,
        float cylinderRadius,
        float3 spherePosition,
        float sphereRadius)
    {
        SegmentToPoints(cylinderRotation, cylinderPosition, cylinderLength, out float3 A, out float3 B);

        float t = ClosestPointNormalizedDistance(A, B, spherePosition);

        float3 closestPoint = A + t * (B - A);

        // well, this is collision with capsule, but good enough
        float maxDistanceSq = sphereRadius * sphereRadius + cylinderRadius * cylinderRadius;
        return math.distancesq(closestPoint, spherePosition) < maxDistanceSq;
    }
}