using UnityEngine;

namespace BehaviourTree
{
    public class BTAction_SelectPOI : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select POI";

        public BTAction_SelectPOI(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (OwningTree.LookAtInterface == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
            }

            GameObject LookTargetGO = null;
            Vector3 LookTargetPosition = CommonCore.Constants.InvalidVector3Position;
            OwningTree.LookAtInterface.DetermineBestLookTarget(LinkedBlackboard, out LookTargetGO, out LookTargetPosition);

            if (LookTargetGO != null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, LookTargetGO);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);
            }
            else if (LookTargetPosition != CommonCore.Constants.InvalidVector3Position)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, LookTargetPosition);
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);
            }

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
        }
    }
}
