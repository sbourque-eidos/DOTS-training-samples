using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct UIData : IComponentData
{
    public NativeString64 canCount;
    public NativeString64 ballCount;

}
