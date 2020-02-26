using Unity.Entities;

[GenerateAuthoringComponent]
public struct Cylinder : IComponentData
{
    public float Length;
    public float Radius;
}
