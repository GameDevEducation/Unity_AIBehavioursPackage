using CommonCore;
using StateMachine;

namespace DemoScenes
{
    public class SMState_UseInteractable : SMStateBase
    {
        bool bHasFinishedInteraction = false;

        public SMState_UseInteractable(string InDisplayName = null) : base(InDisplayName) { }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            bHasFinishedInteraction = false;

            SmartObject TargetSO = null;
            InBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out TargetSO, null);
            if (TargetSO == null)
                return ESMStateStatus.Failed;

            BaseInteraction TargetInteraction = null;
            InBlackboard.TryGet(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
            if (TargetInteraction == null)
                return ESMStateStatus.Failed;

            var Self = GetOwner(InBlackboard);

            TargetInteraction.Perform(Self, (BaseInteraction InInteraction) =>
            {
                InInteraction.UnlockInteraction(Self);
                bHasFinishedInteraction = true;
            });

            return ESMStateStatus.Running;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
            InBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            InBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return bHasFinishedInteraction ? ESMStateStatus.Finished : ESMStateStatus.Running;
        }
    }
}
