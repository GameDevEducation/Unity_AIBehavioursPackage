using BehaviourTree;
using HybridGOAP;
using UnityEngine;
using UnityEngine.Events;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Look at POI")]
    public class GOAP_BTAction_LookAtPOI : GOAPAction_BehaviourTree
    {
        [SerializeField] UnityEvent<GameObject, System.Action<GameObject>> OnPickPOIFn = new();
        [SerializeField] UnityEvent<Transform> OnSetPOIFn = new();

        public override float CalculateCost()
        {
            return 1.0f; // intentionally low cost
        }

        protected override void ConfigureBehaviourTree()
        {
            // Method 1 - Parallel node
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("Look at POI")) as BTFlowNode_Parallel; ;
            LocalRoot.SetPrimary(new BTAction_SelectPOI(PickPOIFn));
            LocalRoot.SetSecondary(new BTAction_LookAtPOI(SetPOIFn));

            // Method 2 - Service
            //var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Look at POI")) as BTFlowNode_Selector;
            //LocalRoot.AddService(new BTService_SelectPOI(PickPOIFn), true);
            //LocalRoot.AddChild(new BTAction_LookAtPOI(SetPOIFn));

            // Method 3 - Service + Decorator
            //var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Look at POI")) as BTFlowNode_Selector;
            //LocalRoot.AddService(new BTService_SelectPOI(PickPOIFn), true);
            //LocalRoot.AddDecorator(new BTDecorator_LookAtPOI(SetPOIFn));
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Head;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_LookAtPOI) };
        }

        GameObject PickPOIFn(GameObject InQuerier)
        {
            var POI = (GameObject)null;
            OnPickPOIFn.Invoke(InQuerier, (GameObject InFoundPOI) =>
            {
                POI = InFoundPOI;
            });

            return POI;
        }

        void SetPOIFn(GameObject InPOI)
        {
            OnSetPOIFn.Invoke(InPOI.transform);
        }
    }
}
