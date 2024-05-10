using BehaviourTree;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_SelectInteraction : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select Interaction";

        System.Func<GameObject, System.Tuple<SmartObject, BaseInteraction>> SelectInteractionFn;

        public BTAction_SelectInteraction(System.Func<GameObject, System.Tuple<SmartObject, BaseInteraction>> InSelectInteractionFn, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            SelectInteractionFn = InSelectInteractionFn;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            if (SelectInteractionFn == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            // attempt to find an interaction
            var Interaction = SelectInteractionFn(Self);
            if ((Interaction == null) || (Interaction.Item1 == null) || (Interaction.Item2 == null))
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            var TargetSO = Interaction.Item1;
            var TargetInteraction = Interaction.Item2;
            if (!TargetInteraction.LockInteraction(Self))
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, TargetSO);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, TargetInteraction);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);
        }
    }
}
