using BehaviourTree;
using CharacterCore;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Use Interactable")]
    public class GOAP_BTAction_UseInteractable : GOAPAction_BehaviourTree
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Sequence("Use Interactable")) as BTFlowNode_Sequence;

            LocalRoot.AddChild(new BTAction_SelectInteraction());
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetInteractableLocation, GetDestinationOrientation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));
            LocalRoot.AddChild(new BTAction_UseInteractable());
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_UseInteractable) };
        }

        Vector3 GetInteractableLocation()
        {
            // attempt to get the interact point
            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint != null)
                return TargetPoint.PointPosition;

            return CommonCore.Constants.InvalidVector3Position;
        }

        Vector3 GetDestinationOrientation()
        {
            // attempt to get the interact point
            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint != null)
                return TargetPoint.PointTransform.forward;

            return CommonCore.Constants.InvalidVector3Position;
        }

        protected override void OnBehaviourTreeReset()
        {
            base.OnBehaviourTreeReset();

            IInteraction CurrentInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out CurrentInteraction, null);
            if (CurrentInteraction != null)
                CurrentInteraction.AbandonInteraction(PerformerInterface);

            LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);
        }
    }
}
