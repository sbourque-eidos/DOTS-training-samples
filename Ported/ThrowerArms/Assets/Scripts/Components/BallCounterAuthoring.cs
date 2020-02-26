using Unity.Entities;
using UnityEngine;

public struct BallCounter : IComponentData
{
}

[DisallowMultipleComponent]
[RequiresEntityConversion]
public class BallCounterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponent<BallCounter>(entity);
        dstManager.AddComponent<CurrentCount>(entity);
    }
}
