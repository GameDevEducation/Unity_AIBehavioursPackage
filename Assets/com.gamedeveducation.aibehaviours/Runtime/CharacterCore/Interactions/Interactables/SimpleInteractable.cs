using CommonCore;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interactable: Simple")]
    public class SimpleInteractable : MonoBehaviour, IInteractable
    {
        public IInteractableRegistry Registry { get; protected set; }

        public List<IInteraction> Interactions { get; protected set; }

        public List<IInteractionLookTarget> LookTargets { get; protected set; }

        public List<IInteractionPoint> InteractionPoints { get; protected set; }

        public List<IInteraction> ActiveInteractions { get; protected set; } = new();

        public Vector3 QueryLocation => transform.position + QueryLocationOffset;
        Vector3 QueryLocationOffset;

        public string DebugDisplayName { get; protected set; }

        Dictionary<IInteractionPoint, bool> InteractionPointUsability = new();

        protected void Awake()
        {
            DebugDisplayName = gameObject.name;

            ServiceLocator.AsyncLocateService<IInteractableRegistry>((ILocatableService InService) =>
            {
                Registry = (IInteractableRegistry)InService;
                Registry.RegisterInteractable(this);
            });
        }

        protected void OnDestroy()
        {
            PrepareToDestroy();

            if (Registry != null)
                Registry.UnregisterInteractable(this);
        }

        protected void Start()
        {
            Interactions = new List<IInteraction>(GetComponentsInChildren<IInteraction>());
            LookTargets = new List<IInteractionLookTarget>(GetComponentsInChildren<IInteractionLookTarget>());
            InteractionPoints = new List<IInteractionPoint>(GetComponentsInChildren<IInteractionPoint>());

            // link all interactions to this interactable
            foreach (var Interaction in Interactions)
                Interaction.Bind(this);

            // figure out the interaction point offset and mark all as usable
            Vector3 AveragedInteractionPoint = Vector3.zero;
            foreach (var Point in InteractionPoints)
            {
                AveragedInteractionPoint += Point.PointPosition - transform.position;

                InteractionPointUsability[Point] = true;
            }

            QueryLocationOffset = InteractionPoints.Count > 0 ? (AveragedInteractionPoint / InteractionPoints.Count) : Vector3.zero;

            Initialise();
        }

        protected void Update()
        {
            Tick(Time.deltaTime);
        }

        public virtual void Tick(float InDeltaTime)
        {
            foreach (var Interaction in Interactions)
                Interaction.Tick(InDeltaTime);
        }

        public virtual void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            InDebugger.AddSectionHeader(DebugDisplayName);
            InDebugger.PushIndent();

            foreach (var Interaction in Interactions)
                Interaction.GatherDebugData(InDebugger, bInIsSelected);

            InDebugger.PopIndent();
        }

        public virtual bool IsUsable(IInteraction InInteraction = null)
        {
            // trying to start a mutually exclusive interaction while other interactions are active?
            if ((InInteraction != null) && InInteraction.IsMutuallyExclusive && (ActiveInteractions.Count > 0))
            {
                // we aren't one of the active interactions?
                if (!ActiveInteractions.Contains(InInteraction))
                    return false;

                // insufficient capacity for us to join?
                if (InInteraction.CurrentPerformerCount >= InInteraction.MaxSimultaneousPerformers)
                    return false;
            }

            // no interactions available?
            if ((Interactions == null) || (Interactions.Count == 0))
                return false;

            // check if any active interactions are blocking us?
            foreach (var Interaction in ActiveInteractions)
            {
                // current interaction is mutually exclusive and not the requesting one
                if (Interaction.IsMutuallyExclusive && (InInteraction != null) && (Interaction != InInteraction))
                    return false;

                // insufficient space to join the interaction?
                if (Interaction.CurrentPerformerCount >= Interaction.MaxSimultaneousPerformers)
                    return false;
            }

            // look for usable points
            bool bUsablePoints = false;
            foreach (var UsabilityKVP in InteractionPointUsability)
            {
                if (UsabilityKVP.Value)
                {
                    bUsablePoints = true;
                    break;
                }
            }
            if (!bUsablePoints)
                return false;

            // if the supplied interaction is null look for any valid ones
            if (InInteraction == null)
            {
                bool bAnyUsable = false;
                foreach (var Interaction in Interactions)
                {
                    if (Interaction.IsUsable())
                    {
                        bAnyUsable = true;
                        break;
                    }
                }

                if (!bAnyUsable)
                    return false;
            }

            return true;
        }

        public virtual IInteractionPoint RequestInteractionPoint()
        {
            IInteractionPoint FoundPoint = null;

            foreach (var UsabilityKVP in InteractionPointUsability)
            {
                if (UsabilityKVP.Value)
                {
                    FoundPoint = UsabilityKVP.Key;
                    break;
                }
            }

            if (FoundPoint != null)
                InteractionPointUsability[FoundPoint] = false;

            return FoundPoint;
        }

        public virtual void ReleaseInteractionPoint(IInteractionPoint InInteractionPoint)
        {
            InteractionPointUsability[InInteractionPoint] = true;
        }

        public virtual void LockedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction)
        {
            ActiveInteractions.Add(InInteraction);
        }

        public virtual void BeganInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction)
        {
        }

        public virtual void TickedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction)
        {
        }

        public virtual void AbandonedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction)
        {
            ActiveInteractions.Remove(InInteraction);
        }

        public virtual void FinishedInteraction(IInteractionPerformer InPerformer, IInteraction InInteraction)
        {
            ActiveInteractions.Remove(InInteraction);
        }

        public virtual List<IInteraction> FilterInteractionsByPredicate(Predicate<IInteraction> InFilterPredicate, IComparer<IInteraction> InComparer = null)
        {
            var FilteredList = Interactions.FindAll(InFilterPredicate);

            if ((FilteredList == null) || (FilteredList.Count == 0))
                return null;

            if (InComparer != null)
                FilteredList.Sort(InComparer);

            return FilteredList;
        }

        protected virtual void Initialise() { }
        protected virtual void PrepareToDestroy() { }
    }
}
