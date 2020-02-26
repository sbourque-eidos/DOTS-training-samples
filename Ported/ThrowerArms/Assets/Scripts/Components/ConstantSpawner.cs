using System;
using Unity.Entities;
using Unity.Mathematics;

public struct ConstantSpawner : IComponentData
{
    public Entity Prefab;
    public float3 MinArea;
    public float3 MaxArea;
    public float3 MinScale;
    public float3 MaxScale;
    public float3 InitialVelocity;
    public int TargetCount;
}
