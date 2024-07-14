using CharacterCore;

namespace BehaviourTree
{
    public class BTAction_UseInteractable : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Use Interactable";

        bool bHasFinishedInteraction = false;

        public BTAction_UseInteractable(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            bHasFinishedInteraction = false;

            IInteractable TargetInteractable;
            LinkedBlackboard.TryGetStorable<IInteractable>(CommonCore.Names.Interaction_Interactable, out TargetInteractable, null);
            if (TargetInteractable == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                LastStatus = EBTNodeResult.Failed;
                return;
            }

            IInteraction TargetInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
            if (TargetInteractable == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                LastStatus = EBTNodeResult.Failed;
                return;
            }

            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);

                LastStatus = EBTNodeResult.Failed;
                return;
            }

            bool bDidStart = TargetInteraction.BeginInteraction(OwningTree.PerformerInterface, null, null, (IInteraction InInteraction) =>
            {
                InInteraction.UnlockInteraction(OwningTree.PerformerInterface);

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

            LastStatus = bDidStart ? EBTNodeResult.InProgress : EBTNodeResult.Failed;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (LastStatus == EBTNodeResult.Failed)
                return false;

            LastStatus = bHasFinishedInteraction ? EBTNodeResult.Succeeded : EBTNodeResult.InProgress;

            return true;
        }
    }
}
