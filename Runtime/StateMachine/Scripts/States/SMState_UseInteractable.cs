using CharacterCore;

namespace StateMachine
{
    public class SMState_UseInteractable : SMStateBase
    {
        IInteractionPerformer PerformerInterface;

        bool bHasFinishedInteraction = false;

        public SMState_UseInteractable(IInteractionPerformer InPerformerInterface, string InDisplayName = null) :
            base(InDisplayName)
        {
            PerformerInterface = InPerformerInterface;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            bHasFinishedInteraction = false;

            IInteractable TargetInteractable;
            LinkedBlackboard.TryGetStorable<IInteractable>(CommonCore.Names.Interaction_Interactable, out TargetInteractable, null);
            if (TargetInteractable == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                return ESMStateStatus.Failed;
            }

            IInteraction TargetInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
            if (TargetInteractable == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                return ESMStateStatus.Failed;
            }

            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                return ESMStateStatus.Failed;
            }

            bool bDidStart = TargetInteraction.BeginInteraction(PerformerInterface, null, null, (IInteraction InInteraction) =>
            {
                InInteraction.UnlockInteraction(PerformerInterface);

                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                bHasFinishedInteraction = true;
            });

            if (!bDidStart)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);
            }

            return bDidStart ? ESMStateStatus.Running : ESMStateStatus.Failed;
        }

        protected override void OnExitInternal()
        {
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            return bHasFinishedInteraction ? ESMStateStatus.Finished : ESMStateStatus.Running;
        }
    }
}
