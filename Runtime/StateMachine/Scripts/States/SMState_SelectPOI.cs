using CharacterCore;
using UnityEngine;

namespace StateMachine
{
    public class SMState_SelectPOI : SMStateBase
    {
        ILookHandler LookHandler;

        public SMState_SelectPOI(ILookHandler InLookHandler, string InDisplayName = null) :
            base(InDisplayName)
        {
            LookHandler = InLookHandler;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            if (LookHandler == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
                return ESMStateStatus.Failed;
            }

            GameObject LookTargetGO = null;
            Vector3 LookTargetPosition = CommonCore.Constants.InvalidVector3Position;
            LookHandler.DetermineBestLookTarget(LinkedBlackboard, out LookTargetGO, out LookTargetPosition);

            if (LookTargetGO != null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, LookTargetGO);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
            }
            else if (LookTargetPosition != CommonCore.Constants.InvalidVector3Position)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, LookTargetPosition);
            }

            return ESMStateStatus.Running;
        }

        protected override void OnExitInternal()
        {
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            return OnEnterInternal();
        }
    }
}
