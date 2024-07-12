using UnityEngine;

namespace BehaviourTree
{
    public class BTService_SelectPOI : BTServiceBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select POI";

        public BTService_SelectPOI(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        public override bool Tick(float InDeltaTime)
        {
            if (OwningTree.LookAtInterface == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
                return false;
            }

            GameObject LookTargetGO = null;
            Vector3 LookTargetPosition = CommonCore.Constants.InvalidVector3Position;
            OwningTree.LookAtInterface.DetermineBestLookTarget(LinkedBlackboard, out LookTargetGO, out LookTargetPosition);

            if (LookTargetGO != null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, LookTargetGO);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
            }
            else if (LookTargetPosition != CommonCore.Constants.InvalidVector3Position)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, LookTargetPosition);
            }

            return true;
        }
    }
}
