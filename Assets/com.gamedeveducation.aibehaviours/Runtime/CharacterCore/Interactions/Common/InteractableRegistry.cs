using CommonCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interactable Registry")]
    public class InteractableRegistry : MonoBehaviourSingleton<InteractableRegistry>, IInteractableRegistry
    {
        public string DebugDisplayName => "Interactables";

        List<IInteractable> Interactables = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            ServiceLocator.RegisterService<IInteractableRegistry>(this, null);

            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InService) =>
            {
                (InService as IGameDebugger).RegisterSource(this);
            });
        }

        public List<IInteractable> FilterInteractablesByPredicate(Predicate<IInteractable> InFilterPredicate, Comparer<IInteractable> InComparer = null)
        {
            var FilteredList = Interactables.FindAll(InFilterPredicate);

            if ((FilteredList == null) || (FilteredList.Count == 0))
                return null;

            if (InComparer != null)
                FilteredList.Sort(InComparer);

            return FilteredList;
        }

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            foreach (var Interactable in Interactables)
                Interactable.GatherDebugData(InDebugger, bInIsSelected);
        }

        public void RegisterInteractable(IInteractable InInteractable)
        {
            if (Interactables.Contains(InInteractable))
            {
                throw new System.InvalidOperationException($"Attempting to register {InInteractable} multiple times");
            }

            Interactables.Add(InInteractable);
        }

        public void UnregisterInteractable(IInteractable InInteractable)
        {
            Interactables.Remove(InInteractable);
        }
    }
}
