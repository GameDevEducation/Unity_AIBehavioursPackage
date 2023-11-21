using CommonCore;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class SMState_SelectPOI : SMStateBase
    {
        System.Func<GameObject, GameObject> PickPOIFn;

        public SMState_SelectPOI(System.Func<GameObject, GameObject> InPickPOIFn, string InCustomName = null) :
            base(InCustomName)
        {
            PickPOIFn = InPickPOIFn;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            if (PickPOIFn == null)
            {
                InBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return ESMStateStatus.Failed;
            }

            var Self = GetOwner(InBlackboard);

            // attempt to pick a POI
            var POI = PickPOIFn(Self);
            if (POI == null)
            {
                InBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return ESMStateStatus.Failed;
            }

            InBlackboard.Set(CommonCore.Names.LookAt_GameObject, POI);

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
