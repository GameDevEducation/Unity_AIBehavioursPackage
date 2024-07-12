using CharacterCore;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("Character/Character: Look at Handler")]
    public class AILookAtHandler : LookHandlerBase
    {
        public override void DetermineBestLookTarget(Blackboard<FastName> InBlackboard, out GameObject OutLookTargetGO, out Vector3 OutLookTargetPosition)
        {
            OutLookTargetGO = null;
            OutLookTargetPosition = CommonCore.Constants.InvalidVector3Position;

            // first priority is our awareness target
            GameObject CurrentAwarenessTarget = null;
            InBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);
            if (IsLookTargetValid(CurrentAwarenessTarget))
            {
                OutLookTargetGO = CurrentAwarenessTarget;
                return;
            }

            // next priority is our interaction target
            SmartObject CurrentInteractionSO = null;
            InBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out CurrentInteractionSO, null);
            if ((CurrentInteractionSO != null) && IsLookTargetValid(CurrentInteractionSO.gameObject))
            {
                OutLookTargetGO = CurrentInteractionSO.gameObject;
                return;
            }

            // next priority is our current focus target
            GameObject CurrentTarget = null;
            InBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);
            if (IsLookTargetValid(CurrentTarget))
            {
                OutLookTargetGO = CurrentTarget;
                return;
            }

            // next priority is our current look target
            GameObject CurrentLookAtTarget = null;
            InBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);
            if (IsLookTargetValid(CurrentLookAtTarget))
            {
                OutLookTargetGO = CurrentLookAtTarget;
                return;
            }

            // next priority is our current look position
            Vector3 CurrentLookAtPosition = CommonCore.Constants.InvalidVector3Position;
            InBlackboard.TryGet(CommonCore.Names.LookAt_Position, out CurrentLookAtPosition, CommonCore.Constants.InvalidVector3Position);
            if (IsLookTargetValid(CurrentLookAtPosition))
            {
                OutLookTargetPosition = CurrentLookAtPosition;
                return;
            }
        }

        bool IsLookTargetValid(GameObject InTargetGO)
        {
            return InTargetGO != null;
        }

        bool IsLookTargetValid(Vector3 InTargetPosition)
        {
            return InTargetPosition != CommonCore.Constants.InvalidVector3Position;
        }
    }
}
