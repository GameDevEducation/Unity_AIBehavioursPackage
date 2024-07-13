using CommonCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    public abstract class InteractionSelectorBase : MonoBehaviour, IInteractionSelector
    {
        public IInteractableRegistry Registry { get; protected set; }

        [field: SerializeField] public bool HasMinSearchRange { get; protected set; } = false;
        [field: SerializeField] public float MinSearchRange { get; protected set; } = 0f;
        [field: SerializeField] public bool HasMaxSearchRange { get; protected set; } = true;
        [field: SerializeField] public float MaxSearchRange { get; protected set; } = 20f;

        protected void Awake()
        {
            ServiceLocator.RegisterService<IInteractionSelector>(this, gameObject);

            ServiceLocator.AsyncLocateService<IInteractableRegistry>((ILocatableService InService) =>
            {
                Registry = (IInteractableRegistry)InService;
            }, null, EServiceSearchMode.GlobalOnly);

            OnAwake();
        }

        public bool PickInteraction(IInteractionPerformer InPerformer, out IInteractable OutFoundInteractable, out IInteraction OutFoundInteraction, Predicate<IInteraction> InAdditionalFilter = null)
        {
            OutFoundInteractable = null;
            OutFoundInteraction = null;

            // no performer?
            if (InPerformer == null)
                return false;

            Vector3 PerformerLocation = InPerformer.PerformerLocation;
            var FilteredInteractables = Registry.FilterInteractablesByPredicate((IInteractable InCandidate) =>
            {
                float DistanceSq = (HasMinSearchRange || HasMaxSearchRange) ? (PerformerLocation - InCandidate.QueryLocation).sqrMagnitude : 0f;

                if (HasMinSearchRange && (DistanceSq < (MinSearchRange * MinSearchRange)))
                    return false;

                if (HasMaxSearchRange && (DistanceSq > (MaxSearchRange * MaxSearchRange)))
                    return false;

                if (!InCandidate.IsUsable())
                    return false;

                return true;
            });

            if ((FilteredInteractables == null) || (FilteredInteractables.Count == 0))
                return false;

            return PickInteractionInternal(InPerformer, FilteredInteractables, out OutFoundInteractable, out OutFoundInteraction, InAdditionalFilter);
        }

        protected virtual void OnAwake() { }
        protected abstract bool PickInteractionInternal(IInteractionPerformer InPerformer,
                                                        List<IInteractable> InCandidateInteractables,
                                                        out IInteractable OutFoundInteractable,
                                                        out IInteraction OutFoundInteraction,
                                                        Predicate<IInteraction> InAdditionalFilter = null);
    }
}
