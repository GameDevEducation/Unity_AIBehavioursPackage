using BehaviourTree;
using HybridGOAP;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Return Home")]
    public class GOAP_BTAction_ReturnHome : GOAPAction_BehaviourTree
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 5.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 20f;
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Sequence("Return Home")) as BTFlowNode_Sequence;
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetHomeLocation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_ReturnHome) };
        }

        Vector3 GetHomeLocation()
        {
            Vector3 HomeLocation = CommonCore.Constants.InvalidVector3Position;

            LinkedBlackboard.TryGet(CommonCore.Names.HomeLocation, out HomeLocation, CommonCore.Constants.InvalidVector3Position);

            return HomeLocation;
        }
    }
}
