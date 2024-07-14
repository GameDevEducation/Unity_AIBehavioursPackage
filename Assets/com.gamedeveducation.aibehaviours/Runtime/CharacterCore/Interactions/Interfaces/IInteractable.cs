using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    public interface IInteractable : IDebuggable, IBlackboardStorable
    {
        IInteractableRegistry Registry { get; }

        List<IInteraction> Interactions { get; }
        List<IInteractionLookTarget> LookTargets { get; }
        List<IInteractionPoint> InteractionPoints { get; }

        List<IInteraction> ActiveInteractions { get; }

        Vector3 QueryLocation { get; }

        bool IsUsable(IInteraction InInteraction = null);
        IInteractionPoint RequestInteractionPoint();
        void ReleaseInteractionPoint(IInteractionPoint InInteractionPoint);

        void Tick(float InDeltaTime);

        List<IInteraction> FilterInteractionsByPredicate(System.Predicate<IInteraction> InFilterPredicate,
                                                         IComparer<IInteraction> InComparer = null);

        void LockedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction);
        void BeganInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction);
        void TickedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction);
        void FinishedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction);
        void AbandonedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction);
    }
}
