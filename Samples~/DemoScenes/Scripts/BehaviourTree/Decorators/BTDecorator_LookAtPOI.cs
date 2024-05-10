using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    public class BTDecorator_LookAtPOI : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => false;

        public override string DebugDisplayName { get; protected set; } = "Look at POI";

        System.Action<GameObject> SetLookTargetFn;

        public BTDecorator_LookAtPOI(System.Action<GameObject> InSetLookTargetFn, bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            SetLookTargetFn = InSetLookTargetFn;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            if (SetLookTargetFn == null)
                return false;

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, (GameObject)null);

            SetLookTargetFn(POI);

            return true;
        }
    }
}
