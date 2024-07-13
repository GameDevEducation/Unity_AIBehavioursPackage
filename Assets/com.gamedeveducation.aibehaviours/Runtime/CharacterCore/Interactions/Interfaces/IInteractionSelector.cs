using CommonCore;

namespace CharacterCore
{
    public interface IInteractionSelector : ILocatableService
    {
        IInteractableRegistry Registry { get; }

        bool HasMinSearchRange { get; }
        float MinSearchRange { get; }

        bool HasMaxSearchRange { get; }
        float MaxSearchRange { get; }

        bool PickInteraction(IInteractionPerformer InPerformer, out IInteractable OutFoundInteractable,
                             out IInteraction OutFoundInteraction, System.Predicate<IInteraction> InAdditionalFilter = null);
    }
}
