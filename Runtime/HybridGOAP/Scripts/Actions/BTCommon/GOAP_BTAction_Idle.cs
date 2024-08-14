using BehaviourTree;
using CharacterCore;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Idle")]
    public class GOAP_BTAction_Idle : GOAPAction_BehaviourTree
    {
        [SerializeField] float MinWaitTime = 1.0f;
        [SerializeField] float MaxWaitTime = 10.0f;

        [SerializeField] AnimationRequest AnimationData;
        [SerializeField] float MinAnimationCooldownTime = 5.0f;
        [SerializeField] float MaxAnimationCooldownTime = 10.0f;

        public override float CalculateCost()
        {
            return 0.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            if ((AnimationData != null) && AnimationData.IsValid())
            {
                var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("Idle")) as BTFlowNode_Parallel;

                LocalRoot.SetPrimary(new BTAction_Wait(MinWaitTime, MaxWaitTime));

                var PerformAnimation = LocalRoot.SetSecondary(new BTFlowNode_Sequence("Perform Animation")) as BTFlowNode_Sequence;
                PerformAnimation.AddChild(new BTAction_SendAnimationRequest(AnimationData, true));
                PerformAnimation.AddChild(new BTAction_Wait(MinAnimationCooldownTime, MaxAnimationCooldownTime));
            }
            else
            {
                var LocalRoot = AddChildToRootNode(new BTFlowNode_Selector("Idle")) as BTFlowNode_Selector;
                LocalRoot.AddChild(new BTAction_Wait(MinWaitTime, MaxWaitTime));
            }
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
