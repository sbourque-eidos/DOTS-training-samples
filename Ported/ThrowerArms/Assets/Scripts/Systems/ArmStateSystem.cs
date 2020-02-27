using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ArmStateSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref ArmStateData state) =>
        {
            switch (state.CurrentState)
            {
                case ArmStateData.State.TargetingBall:
                    break;
                case ArmStateData.State.PickingUp:
                    break;
                case ArmStateData.State.WindingUp:
                    break;
                case ArmStateData.State.TargetingCan:
                    break;
                case ArmStateData.State.Throwing:
                    break;
            }
        }).ScheduleParallel();
    }
}
