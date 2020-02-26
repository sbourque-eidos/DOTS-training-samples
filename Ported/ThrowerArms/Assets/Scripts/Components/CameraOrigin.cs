using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class CameraOrigin : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float m_PivotDistance = 10.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CameraOriginData
        {
            OriginalPosition = transform.position,
            PivotDistance = m_PivotDistance
        });
        dstManager.AddComponent<CopyTransformToGameObject>(entity);
    }
}

public struct CameraOriginData : IComponentData
{
    public float3 OriginalPosition;
    public float PivotDistance;
}
