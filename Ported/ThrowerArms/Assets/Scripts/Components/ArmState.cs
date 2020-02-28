using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ArmState : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField]
    ArmStateData.State m_InitialState = ArmStateData.State.TargetingBall;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ArmStateData { CurrentState = m_InitialState });
    }
}

public struct ArmStateData : IComponentData
{
    public enum State
    {
        RequestingBallTargeting,
        TargetingBall,
        PickingUp,
        WindingUp,
        TargetingCan,
        Throwing
    }

    public State CurrentState;
    public float Cooldown;
}
