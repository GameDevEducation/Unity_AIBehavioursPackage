using CommonCore;
using System.Collections.Generic;

namespace CharacterCore
{
    public interface IInteractableRegistry : IDebuggableObject, ILocatableService
    {
        void RegisterInteractable(IInteractable InInteractable);
        void UnregisterInteractable(IInteractable InInteractable);

        List<IInteractable> FilterInteractablesByPredicate(System.Predicate<IInteractable> InFilterPredicate, Comparer<IInteractable> InComparer = null);
    }
}
