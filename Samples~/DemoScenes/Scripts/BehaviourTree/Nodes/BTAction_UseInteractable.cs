using BehaviourTree;
using CommonCore;

namespace DemoScenes
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

            SmartObject TargetSO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out TargetSO, null);
            if (TargetSO == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

                LastStatus = EBTNodeResult.Failed;
                return;
            }

            BaseInteraction TargetInteraction = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
            if (TargetInteraction == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

                LastStatus = EBTNodeResult.Failed;
                return;
            }

            TargetInteraction.Perform(Self, (BaseInteraction InInteraction) =>
            {
                InInteraction.UnlockInteraction(Self);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
                LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

                bHasFinishedInteraction = true;
            });

            LastStatus = EBTNodeResult.InProgress;
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
