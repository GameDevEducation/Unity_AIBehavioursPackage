using CharacterCore;
using StateMachine;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/FSM Actions/FSM Action: Use Interactable")]
    public class GOAP_SMAction_UseInteractable : GOAPAction_FSM
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureFSM()
        {
            var State_SelectInteractable = AddState(new SMState_SelectInteraction(InteractionInterface, PerformerInterface));
            var State_GetInteractableLocation = AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetInteractableLocation, GetDestinationOrientation));
            var State_MoveToInteractable = AddState(new SMState_MoveTo(Navigation, StoppingDistance));
            var State_UseInteractable = AddState(new SMState_UseInteractable(PerformerInterface));

            State_SelectInteractable.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetInteractableLocation);
            State_GetInteractableLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToInteractable);
            State_MoveToInteractable.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_UseInteractable);

            AddDefaultTransitions();
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

        protected override void OnStateMachineReset()
        {
            base.OnStateMachineReset();

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
