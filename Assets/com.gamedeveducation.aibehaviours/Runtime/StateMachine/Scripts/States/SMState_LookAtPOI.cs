using CharacterCore;
using UnityEngine;

namespace StateMachine
{
    public class SMState_LookAtPOI : SMStateBase
    {
        ILookHandler LookHandler;

        public SMState_LookAtPOI(ILookHandler InLookHandler, string InDisplayName = null) :
            base(InDisplayName)
        {
            LookHandler = InLookHandler;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            if (LookHandler == null)
                return ESMStateStatus.Failed;

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, (GameObject)null);

            bool bResult = false;
            if (POI != null)
                bResult = LookHandler.SetLookTarget(POI.transform);
            else
            {
                Vector3 POIPosition = CommonCore.Constants.InvalidVector3Position;
                LinkedBlackboard.TryGet(CommonCore.Names.LookAt_Position, out POIPosition, CommonCore.Constants.InvalidVector3Position);

                if (POIPosition != CommonCore.Constants.InvalidVector3Position)
                    bResult = LookHandler.SetLookTarget(POIPosition);
                else
                    LookHandler.ClearLookTarget();
            }

            return bResult ? ESMStateStatus.Running : ESMStateStatus.Failed;
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
