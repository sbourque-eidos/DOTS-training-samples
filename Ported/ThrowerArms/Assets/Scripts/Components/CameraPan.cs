using UnityEngine;
using Unity.Entities;

public class CameraPan : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float m_Amplitude = 10.0f;

    [SerializeField]
    float m_Period = 35.0f;

    [SerializeField]
    float m_Phase = 0.0f;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CameraPanData
        {
             Amplitude = m_Amplitude,
             Period = m_Period,
             Phase = m_Phase
        });
    }
}

public struct CameraPanData : IComponentData
{
    public float PanOffset;
    public float Amplitude;
    public float Period;
    public float Phase;
}
