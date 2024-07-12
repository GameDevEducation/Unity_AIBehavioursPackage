using UnityEngine;

namespace BehaviourTree
{
    public class BTAction_LookAtPOI : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Look at POI";

        public BTAction_LookAtPOI(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (OwningTree.LookAtInterface == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            GameObject POI = null;
            LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out POI, (GameObject)null);

            bool bResult = false;
            if (POI != null)
                bResult = OwningTree.LookAtInterface.SetLookTarget(POI.transform);
            else
            {
                Vector3 POIPosition = CommonCore.Constants.InvalidVector3Position;
                LinkedBlackboard.TryGet(CommonCore.Names.LookAt_Position, out POIPosition, CommonCore.Constants.InvalidVector3Position);

                if (POIPosition != CommonCore.Constants.InvalidVector3Position)
                    bResult = OwningTree.LookAtInterface.SetLookTarget(POIPosition);
                else
                    OwningTree.LookAtInterface.ClearLookTarget();
            }

            return SetStatusAndCalculateReturnValue(bResult ? EBTNodeResult.Succeeded : EBTNodeResult.Failed);
        }
    }
}
