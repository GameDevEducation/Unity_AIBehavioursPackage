using CharacterCore;

namespace BehaviourTree
{
    public class BTAction_SelectInteraction : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select Interaction";

        public BTAction_SelectInteraction(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

            if (OwningTree.InteractionInterface == null)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }
            if (OwningTree.PerformerInterface == null)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            IInteractable FoundInteractable;
            IInteraction FoundInteraction;
            IInteractionPoint FoundPoint;

            if (OwningTree.InteractionInterface.PickInteraction(OwningTree.PerformerInterface,
                out FoundInteractable, out FoundInteraction))
            {
                if (FoundInteraction.LockInteraction(OwningTree.PerformerInterface, out FoundPoint))
                {
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, FoundInteractable);
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, FoundInteraction);
                    LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, FoundPoint);

                    LastStatus = EBTNodeResult.Succeeded;
                    return;
                }
            }

            LastStatus = EBTNodeResult.Failed;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            return SetStatusAndCalculateReturnValue(LastStatus);
        }
    }
}
