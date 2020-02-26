using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[GenerateAuthoringComponent]
public struct IncreaseScaleData : IComponentData
{
    public float Speed;
    public float PercentReached; //[0,1]
    public float TargetScale; //[0,1]
}
