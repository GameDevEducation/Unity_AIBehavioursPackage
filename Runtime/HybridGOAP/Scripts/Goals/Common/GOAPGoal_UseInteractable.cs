using CharacterCore;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Use Interactable")]
    public class GOAPGoal_UseInteractable : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            IInteraction CurrentInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out CurrentInteraction, null);

            if (CurrentInteraction != null)
                Priority = GoalPriority.High;
            else
            {
                IInteractable FoundInteractable;
                IInteraction FoundInteraction;

                bool bFoundInteraction = InteractionInterface.PickInteraction(PerformerInterface, out FoundInteractable, out FoundInteraction);

                Priority = bFoundInteraction ? GoalPriority.Medium : GoalPriority.DoNotRun;
            }
        }
    }
}
