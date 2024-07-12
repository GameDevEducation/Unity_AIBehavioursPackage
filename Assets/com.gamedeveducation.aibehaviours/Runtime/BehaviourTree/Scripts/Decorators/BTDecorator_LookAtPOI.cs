using UnityEngine;

namespace BehaviourTree
{
    public class BTDecorator_LookAtPOI : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => false;

        public override string DebugDisplayName { get; protected set; } = "Look at POI";

        public BTDecorator_LookAtPOI(bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            if (OwningTree.LookAtInterface == null)
                return false;

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, (GameObject)null);

            if (POI != null)
                OwningTree.LookAtInterface.SetLookTarget(POI.transform);
            else
            {
                Vector3 POIPosition = CommonCore.Constants.InvalidVector3Position;
                LinkedBlackboard.TryGet(CommonCore.Names.LookAt_Position, out POIPosition, CommonCore.Constants.InvalidVector3Position);

                if (POIPosition != CommonCore.Constants.InvalidVector3Position)
                    OwningTree.LookAtInterface.SetLookTarget(POIPosition);
                else
                    OwningTree.LookAtInterface.ClearLookTarget();
            }

            return true;
        }
    }
}
