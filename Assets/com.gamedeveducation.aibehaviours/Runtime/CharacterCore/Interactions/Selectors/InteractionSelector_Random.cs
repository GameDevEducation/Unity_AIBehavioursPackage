using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interaction Selector: Random")]
    public class InteractionSelector_Random : InteractionSelectorBase
    {
        protected override bool PickInteractionInternal(IInteractionPerformer InPerformer, List<IInteractable> InCandidateInteractables, out IInteractable OutFoundInteractable, out IInteraction OutFoundInteraction, System.Predicate<IInteraction> InAdditionalFilter = null)
        {
            OutFoundInteractable = null;
            OutFoundInteraction = null;

            List<IInteraction> CandidateInteractions = new();
            foreach (var Interactable in InCandidateInteractables)
            {
                foreach (var Interaction in Interactable.Interactions)
                {
                    if (!Interaction.IsUsable())
                        continue;

                    if ((InAdditionalFilter != null) && !InAdditionalFilter(Interaction))
                        continue;

                    CandidateInteractions.Add(Interaction);
                }
            }

            if (CandidateInteractions.Count > 0)
            {
                OutFoundInteraction = CandidateInteractions[Random.Range(0, CandidateInteractions.Count)];
                OutFoundInteractable = OutFoundInteraction.Owner;

                return true;
            }

            return false;
        }
    }
}
