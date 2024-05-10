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

        protected override ESMStateStatus OnEnterInternal()
        {
            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            if (SelectInteractionFn == null)
                return ESMStateStatus.Failed;

            // attempt to find an interaction
            var Interaction = SelectInteractionFn(Self);
            if ((Interaction == null) || (Interaction.Item1 == null) || (Interaction.Item2 == null))
                return ESMStateStatus.Failed;

            var TargetSO = Interaction.Item1;
            var TargetInteraction = Interaction.Item2;
            if (!TargetInteraction.LockInteraction(Self))
                return ESMStateStatus.Failed;

            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, TargetSO);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, TargetInteraction);

            return ESMStateStatus.Finished;
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
