using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_SelectPOI : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select POI";

        System.Func<GameObject, GameObject> PickPOIFn;

        public BTAction_SelectPOI(System.Func<GameObject, GameObject> InPickPOIFn, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            PickPOIFn = InPickPOIFn;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (PickPOIFn == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
            }

            var POI = PickPOIFn(Self);

            LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, POI);

            return SetStatusAndCalculateReturnValue(POI != null ? EBTNodeResult.Succeeded : EBTNodeResult.Failed);
        }
    }
}
