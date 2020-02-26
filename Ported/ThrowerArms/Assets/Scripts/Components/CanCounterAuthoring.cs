using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct CanCounter : IComponentData
{
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class CanCounterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<CanCounter>(entity);
        dstManager.AddComponent<CurrentCount>(entity);
    }
}
