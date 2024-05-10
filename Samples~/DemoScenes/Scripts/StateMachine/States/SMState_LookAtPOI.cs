using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_LookAtPOI : SMStateBase
    {
        System.Action<GameObject> SetLookTargetFn;

        public SMState_LookAtPOI(System.Action<GameObject> InSetLookTargetFn, string InDisplayName = null) :
            base(InDisplayName)
        {
            SetLookTargetFn = InSetLookTargetFn;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            if (SetLookTargetFn == null)
                return ESMStateStatus.Failed;

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, null);

            if (POI == null)
                return ESMStateStatus.Failed;

            SetLookTargetFn(POI);

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
