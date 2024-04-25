using CommonCore;
using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_SelectInteraction : SMStateBase
    {
        System.Func<GameObject, System.Tuple<SmartObject, BaseInteraction>> SelectInteractionFn;

        public SMState_SelectInteraction(System.Func<GameObject, System.Tuple<SmartObject, BaseInteraction>> InSelectInteractionFn, string InDisplayName = null) :
            base(InDisplayName)
        {
            SelectInteractionFn = InSelectInteractionFn;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            InBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            InBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            if (SelectInteractionFn == null)
                return ESMStateStatus.Failed;

            var Self = GetOwner(InBlackboard);

            // attempt to find an interaction
            var Interaction = SelectInteractionFn(Self);
            if ((Interaction == null) || (Interaction.Item1 == null) || (Interaction.Item2 == null))
                return ESMStateStatus.Failed;

            var TargetSO = Interaction.Item1;
            var TargetInteraction = Interaction.Item2;
            if (!TargetInteraction.LockInteraction(Self))
                return ESMStateStatus.Failed;

            InBlackboard.Set(CommonCore.Names.Interaction_SmartObject, TargetSO);
            InBlackboard.Set(CommonCore.Names.Interaction_Type, TargetInteraction);

            return ESMStateStatus.Finished;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return CurrentStatus;
        }
    }
}
