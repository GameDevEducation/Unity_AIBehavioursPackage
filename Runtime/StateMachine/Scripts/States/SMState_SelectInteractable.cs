using CharacterCore;

namespace StateMachine
{
    public class SMState_SelectInteraction : SMStateBase
    {
        IInteractionSelector InteractionInterface;
        IInteractionPerformer PerformerInterface;

        public SMState_SelectInteraction(IInteractionSelector InInteractionInterface, IInteractionPerformer InPerformerInterface, string InDisplayName = null) :
            base(InDisplayName)
        {
            InteractionInterface = InInteractionInterface;
            PerformerInterface = InPerformerInterface;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

            if (InteractionInterface == null)
                return ESMStateStatus.Failed;
            if (PerformerInterface == null)
                return ESMStateStatus.Failed;

            IInteractable FoundInteractable;
            IInteraction FoundInteraction;
            IInteractionPoint FoundPoint;

            if (InteractionInterface.PickInteraction(PerformerInterface,
                out FoundInteractable, out FoundInteraction))
            {
                if (FoundInteraction.LockInteraction(PerformerInterface, out FoundPoint))
                {
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, FoundInteractable);
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, FoundInteraction);
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, FoundPoint);

                    return ESMStateStatus.Finished;
                }
            }

            return ESMStateStatus.Failed;
        }

        protected override void OnExitInternal()
        {
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            return CurrentStatus;
        }
    }
}
