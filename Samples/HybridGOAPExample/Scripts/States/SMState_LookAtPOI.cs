using CommonCore;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class SMState_LookAtPOI : SMStateBase
    {
        System.Action<GameObject> SetLookTargetFn;

        public SMState_LookAtPOI(System.Action<GameObject> InSetLookTargetFn, string InCustomName = null) :
            base(InCustomName)
        {
            SetLookTargetFn = InSetLookTargetFn;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            if (SetLookTargetFn == null)
                return ESMStateStatus.Failed;

            GameObject POI = null;
            InBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, null);

            if (POI == null)
                return ESMStateStatus.Failed;

            SetLookTargetFn(POI);

            return ESMStateStatus.Running;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return OnEnterInternal(InBlackboard);
        }
    }
}
