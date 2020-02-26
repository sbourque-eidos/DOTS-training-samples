using System;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct UniformSpawner : IComponentData
{
    public Entity Prefab;
    public int Count;
    public float3 Min;
    public float3 Max;
}
