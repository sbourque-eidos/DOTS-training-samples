using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public class PlaneAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        float3 normal = transform.up.normalized;
        float3 pos = transform.position;

        float distance = math.dot(normal, pos);

        dstManager.AddComponentData(entity, new Plane
        {
            Direction = normal,
            Distance = distance
        });
    }
}

public struct Plane : IComponentData
{
    public float3 Direction;
    public float Distance;
}

