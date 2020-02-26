﻿using System;
using Unity.Entities;
using Unity.Mathematics;

public struct ConstantSpawner : IComponentData
{
    public float3 MinArea;
    public float3 MaxArea;
    public Entity Prefab;
}
