using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_LookAtPOI : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Look at POI";

        System.Action<GameObject> SetLookTargetFn;

        public BTAction_LookAtPOI(System.Action<GameObject> InSetLookTargetFn, string InDisplayName = null)
        {
            SetLookTargetFn = InSetLookTargetFn;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (SetLookTargetFn == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, (GameObject)null);

            SetLookTargetFn(POI);

            return SetStatusAndCalculateReturnValue(POI != null ? EBTNodeResult.Succeeded : EBTNodeResult.Failed);
        }
    }
}
