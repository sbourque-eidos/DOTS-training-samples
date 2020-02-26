using UnityEngine;
using Unity.Entities;

public class CameraZoom : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float m_Amplitude = 2.5f;

    [SerializeField]
    float m_Period = 35.0f;

    [SerializeField]
    float m_Phase;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CameraZoomData
        {
            Amplitude = m_Amplitude,
            Period = m_Period,
            Phase = m_Phase
        });
    }
}

public struct CameraZoomData : IComponentData
{
    public float Zoom;
    public float Amplitude;
    public float Period;
    public float Phase;
}
