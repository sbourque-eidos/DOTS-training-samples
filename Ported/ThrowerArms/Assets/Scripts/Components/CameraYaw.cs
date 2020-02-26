using UnityEngine;
using Unity.Entities;

public class CameraYaw : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float m_Amplitude = 30.0f;

    [SerializeField]
    float m_Period = 35.0f;

    [SerializeField]
    float m_Phase = 0.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CameraYawData
        {
            Amplitude = m_Amplitude,
            Period = m_Period,
            Phase = m_Phase
        });
    }
}

public struct CameraYawData : IComponentData
{
    public float YawAngle;
    public float Amplitude;
    public float Period;
    public float Phase;
}
