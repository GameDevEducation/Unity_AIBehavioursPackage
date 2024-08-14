using BehaviourTree;
using CharacterCore;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Wander")]
    public class GOAP_BTAction_Wander : GOAPAction_BehaviourTree
    {
        [SerializeField] float MinWanderRange = 10.0f;
        [SerializeField] float MaxWanderRange = 50.0f;

        [SerializeField] float MinWaitTime = 4.0f;
        [SerializeField] float MaxWaitTime = 8.0f;

        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        [SerializeField] AnimationRequest AnimationData;
        [SerializeField] float MinAnimationCooldownTime = 5.0f;
        [SerializeField] float MaxAnimationCooldownTime = 10.0f;

        public override float CalculateCost()
        {
            return 1f; // intentionally low cost
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Sequence("Wander")) as BTFlowNode_Sequence;
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetWanderLocation, null));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));

            if ((AnimationData != null) && AnimationData.IsValid())
            {
                var WanderWait = LocalRoot.AddChild(new BTFlowNode_Parallel("Wait")) as BTFlowNode_Parallel;

                WanderWait.SetPrimary(new BTAction_Wait(MinWaitTime, MaxWaitTime));

                var PerformAnimation = WanderWait.SetSecondary(new BTFlowNode_Sequence("Perform Animation")) as BTFlowNode_Sequence;
                PerformAnimation.AddChild(new BTAction_SendAnimationRequest(AnimationData, true));
                PerformAnimation.AddChild(new BTAction_Wait(MinAnimationCooldownTime, MaxAnimationCooldownTime));
            }
            else
            {
                LocalRoot.AddChild(new BTAction_Wait(MinWaitTime, MaxWaitTime));
            }
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Wander) };
        }

        Vector3 GetWanderLocation()
        {
            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);

            // pick random direction and distance
            float Angle = Random.Range(0f, Mathf.PI * 2f);
            float Distance = Random.Range(MinWanderRange, MaxWanderRange);

            // generate a position
            Vector3 WanderTarget = CurrentLocation;
            WanderTarget.x += Distance * Mathf.Sin(Angle);
            WanderTarget.z += Distance * Mathf.Cos(Angle);

            return WanderTarget;
        }
    }
}
