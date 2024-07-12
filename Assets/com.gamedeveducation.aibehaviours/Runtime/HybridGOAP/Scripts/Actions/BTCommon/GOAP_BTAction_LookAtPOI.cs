using BehaviourTree;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Look at POI")]
    public class GOAP_BTAction_LookAtPOI : GOAPAction_BehaviourTree
    {
        public override float CalculateCost()
        {
            return 1.0f; // intentionally low cost
        }

        protected override void ConfigureBehaviourTree()
        {
            // Method 1 - Parallel node
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("Look at POI")) as BTFlowNode_Parallel; ;
            LocalRoot.SetPrimary(new BTAction_SelectPOI());
            LocalRoot.SetSecondary(new BTAction_LookAtPOI());

            // Method 2 - Service
            //var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Look at POI")) as BTFlowNode_Selector;
            //LocalRoot.AddService(new BTService_SelectPOI(), true);
            //LocalRoot.AddChild(new BTAction_LookAtPOI());

            // Method 3 - Service + Decorator
            //var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Look at POI")) as BTFlowNode_Selector;
            //LocalRoot.AddService(new BTService_SelectPOI(), true);
            //LocalRoot.AddDecorator(new BTDecorator_LookAtPOI());
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Head;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_LookAtPOI) };
        }
    }
}
