
using Unity.Entities;

public struct Sphere : IComponentData
{
    public float Radius;
}

public struct Cylinder : IComponentData
{
    public float Length;
    public float Radius;
}

public struct Target : IComponentData
{
    public Entity Value;
}