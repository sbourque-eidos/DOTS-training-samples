using UnityEngine;
using Unity.Entities;

public class CameraTilt : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    float m_Amplitude = 45.0f;

    [SerializeField]
    float m_Period = 35.0f;

    [SerializeField]
    float m_Phase;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new CameraTiltData
        {
            Amplitude = m_Amplitude,
            Period = m_Period,
            Phase = m_Phase
        });
    }
}

public struct CameraTiltData : IComponentData
{
    public float TiltAngle;
    public float Amplitude;
    public float Period;
    public float Phase;
}
