using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_SelectPOI : SMStateBase
    {
        System.Func<GameObject, GameObject> PickPOIFn;

        public SMState_SelectPOI(System.Func<GameObject, GameObject> InPickPOIFn, string InDisplayName = null) :
            base(InDisplayName)
        {
            PickPOIFn = InPickPOIFn;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            if (PickPOIFn == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return ESMStateStatus.Failed;
            }

            // attempt to pick a POI
            var POI = PickPOIFn(Self);
            if (POI == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return ESMStateStatus.Failed;
            }

            LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, POI);

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
