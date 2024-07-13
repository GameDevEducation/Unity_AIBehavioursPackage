using BehaviourTree;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Chase")]
    public class GOAP_BTAction_Chase : GOAPAction_BehaviourTree
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 1f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            // Method 1 - Using parallel node
            //var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("Chase")) as BTFlowNode_Parallel;
            //LocalRoot.SetPrimary(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetTargetLocation, true));
            //LocalRoot.SetSecondary(new BTAction_Move(StoppingDistance, true));

            // Method 2 - Using a service
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Chase")) as BTFlowNode_Selector;
            LocalRoot.AddService(new BTService_CalculateMoveLocation(ValidNavMeshSearchRange, GetTargetLocation, GetEndOrientation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance, true));
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Chase) };
        }

        Vector3 GetEndOrientation()
        {
            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);
            Vector3 TargetLocation = GetTargetLocation();

            if ((TargetLocation == CommonCore.Constants.InvalidVector3Position) ||
                (CurrentLocation == CommonCore.Constants.InvalidVector3Position))
                return CommonCore.Constants.InvalidVector3Position;

            return (TargetLocation - CurrentLocation).normalized;
        }

        Vector3 GetTargetLocation()
        {
            Vector3 TargetLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to get the target object
            GameObject TargetGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out TargetGO, null);

            if (TargetGO != null)
            {
                TargetLocation = TargetGO.transform.position;
                LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, TargetGO);
            }
            else
            {
                LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
                LinkedBlackboard.TryGet(CommonCore.Names.Target_Position, out TargetLocation, CommonCore.Constants.InvalidVector3Position);
            }

            return TargetLocation;
        }
    }
}
