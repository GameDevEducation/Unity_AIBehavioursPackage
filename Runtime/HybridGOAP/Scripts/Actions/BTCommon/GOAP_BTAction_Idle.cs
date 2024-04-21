using BehaviourTree;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Idle")]
    public class GOAP_BTAction_Idle : GOAPAction_BehaviourTree
    {
        [SerializeField] float MinWaitTime = 1.0f;
        [SerializeField] float MaxWaitTime = 10.0f;

        public override float CalculateCost()
        {
            return 0.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Idle")) as BTFlowNode_Selector;
            LocalRoot.AddChild(new BTAction_Wait(MinWaitTime, MaxWaitTime));
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Idle) };
        }
    }
}
