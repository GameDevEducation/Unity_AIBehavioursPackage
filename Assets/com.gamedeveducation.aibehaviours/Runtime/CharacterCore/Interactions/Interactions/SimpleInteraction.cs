using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interaction: Simple")]
    public class SimpleInteraction : MonoBehaviour, IInteraction
    {
        protected class PerformerInfo
        {
            public bool bHasStarted = false;
            public float Duration = 0f;
            public float TimeElapsed = 0f;
            public IInteractionPoint LinkedPoint;
            public System.Func<IInteraction, float, float, bool> OnTickCallbackFn;
            public System.Action<IInteraction> OnCompletedCallbackFn;
        }

        [field: SerializeField] public bool IsMutuallyExclusive { get; protected set; } = false;
        [field: SerializeField] public int MaxSimultaneousPerformers { get; protected set; } = 1;
        [field: SerializeField] public float MinDuration { get; protected set; } = 0.0f;
        [field: SerializeField] public float MaxDuration { get; protected set; } = 0.0f;

        [SerializeField] List<GameObject> LookTargetGameObjects;
        [SerializeField] List<GameObject> InteractionPointGameObjects;

        public int CurrentPerformerCount => CurrentPerformers.Count;

        public IInteractable Owner { get; protected set; }
        public List<IInteractionLookTarget> LookTargets { get; protected set; }
        public List<IInteractionPoint> InteractionPoints { get; protected set; }

        public string DebugDisplayName { get; protected set; }

        Dictionary<IInteractionPerformer, PerformerInfo> CurrentPerformers = new();

        protected void Awake()
        {
            DebugDisplayName = gameObject.name;

            // Transfer our LookTargets from the GameObjects
            if ((LookTargetGameObjects != null) && (LookTargetGameObjects.Count > 0))
            {
                LookTargets = new List<IInteractionLookTarget>(LookTargetGameObjects.Count);

                foreach (var CandidateGO in LookTargetGameObjects)
                {
                    IInteractionLookTarget LookTarget = null;

                    if (CandidateGO.TryGetComponent<IInteractionLookTarget>(out LookTarget))
                        LookTargets.Add(LookTarget);
                }
            }

            // Transfer our InteractionPoints from the GameObjects
            if ((InteractionPointGameObjects != null) && (InteractionPointGameObjects.Count > 0))
            {
                InteractionPoints = new List<IInteractionPoint>(InteractionPointGameObjects.Count);

                foreach (var CandidateGO in InteractionPointGameObjects)
                {
                    IInteractionPoint FoundPoint = null;

                    if (CandidateGO.TryGetComponent<IInteractionPoint>(out FoundPoint))
                        InteractionPoints.Add(FoundPoint);
                }
            }
        }

        public void Bind(IInteractable InOwner)
        {
            Owner = InOwner;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            InDebugger.AddTextLine($"{DebugDisplayName}: {CurrentPerformerCount} of {MaxSimultaneousPerformers} performers");
        }

        public bool IsUsable()
        {
            // already at performer count cap?
            if (CurrentPerformerCount >= MaxSimultaneousPerformers)
                return false;

            // owner considers interaction blocked
            if (!Owner.IsUsable(this))
                return false;

            return true;
        }

        public bool LockInteraction(IInteractionPerformer InPerformer, out IInteractionPoint OutFoundPoint)
        {
            OutFoundPoint = null;

            // interaction unusable?
            if (!IsUsable())
            {
                Debug.LogError($"{InPerformer.DisplayName} attempting to lock an unusable interaction {DebugDisplayName} on {Owner.DebugDisplayName}");
                return false;
            }

            // already performing interaction?
            if (CurrentPerformers.ContainsKey(InPerformer))
            {
                Debug.LogError($"{InPerformer.DisplayName} attempting to lock an interaction {DebugDisplayName} multiple times on {Owner.DebugDisplayName}");
                return false;
            }

            OutFoundPoint = Owner.RequestInteractionPoint();

            // no interaction point?
            if (OutFoundPoint == null)
            {
                Debug.LogError($"{InPerformer.DisplayName} could not find usable point for interaction {DebugDisplayName} on {Owner.DebugDisplayName}");
                return false;
            }

            float WorkingDuration = ((MinDuration > 0) && (MaxDuration > MinDuration)) ? Random.Range(MinDuration, MaxDuration) : MinDuration;

            CurrentPerformers.Add(InPerformer, new PerformerInfo()
            {
                bHasStarted = false,
                Duration = WorkingDuration,
                TimeElapsed = 0.0f,
                LinkedPoint = OutFoundPoint
            });

            return true;
        }

        public bool UnlockInteraction(IInteractionPerformer InPerformer)
        {
            PerformerInfo Entry;

            // not currently performing the interaction?
            if (!CurrentPerformers.TryGetValue(InPerformer, out Entry))
            {
                Debug.LogError($"{InPerformer.DisplayName} attempting to unlock an interaction {DebugDisplayName} it does not have locked on {Owner.DebugDisplayName}");
                return false;
            }

            Owner.ReleaseInteractionPoint(Entry.LinkedPoint);
            CurrentPerformers.Remove(InPerformer);

            return true;
        }

        public bool BeginInteraction(IInteractionPerformer InPerformer, System.Action<IInteraction> InOnBeganCallbackFn = null, System.Func<IInteraction, float, float, bool> InOnTickCallbackFn = null, System.Action<IInteraction> InOnCompletedCallbackFn = null)
        {
            PerformerInfo Entry;

            // does not have the interaction locked?
            if (!CurrentPerformers.TryGetValue(InPerformer, out Entry))
            {
                Debug.LogError($"{InPerformer.DisplayName} attempting to start an interaction {DebugDisplayName} it does not have locked on {Owner.DebugDisplayName}");
                return false;
            }

            Entry.bHasStarted = true;
            Entry.OnCompletedCallbackFn = InOnCompletedCallbackFn;
            Entry.OnTickCallbackFn = InOnTickCallbackFn;

            // report interaction as begun?
            Owner.BeganInteraction(InPerformer, this);
            if (InOnBeganCallbackFn != null)
                InOnBeganCallbackFn.Invoke(this);

            // instantaneous?
            if (Entry.Duration <= 0f)
            {
                // send tick notifications even if finishing immediately
                if (InOnTickCallbackFn != null)
                    InOnTickCallbackFn.Invoke(this, 0.0f, 1.0f);
                Owner.TickedInteraction(InPerformer, this);

                Owner.FinishedInteraction(InPerformer, this);

                CompleteInteraction(InPerformer, Entry);
            }

            return true;
        }

        public bool AbandonInteraction(IInteractionPerformer InPerformer)
        {
            PerformerInfo Entry;

            // not currently performing interaction?
            if (!CurrentPerformers.TryGetValue(InPerformer, out Entry))
            {
                Debug.LogError($"{InPerformer.DisplayName} attempting to abando an interaction {DebugDisplayName} it does not have locked on {Owner.DebugDisplayName}");
                return false;
            }

            Owner.ReleaseInteractionPoint(Entry.LinkedPoint);
            CurrentPerformers.Remove(InPerformer);

            Owner.AbandonedInteraction(InPerformer, this);

            return true;
        }

        public void Tick(float InDeltaTime)
        {
            List<IInteractionPerformer> PerformersToCleanup = new();

            foreach (var PerformerKVP in CurrentPerformers)
            {
                IInteractionPerformer Performer = PerformerKVP.Key;
                PerformerInfo Entry = PerformerKVP.Value;

                // not started yet?
                if (!Entry.bHasStarted)
                    continue;

                Entry.TimeElapsed += InDeltaTime;

                float Progress = Mathf.Min(1.0f, Entry.TimeElapsed / Entry.Duration);

                Owner.TickedInteraction(Performer, this);

                // run the tick callback and see if we're abandoning the interaction?
                bool bAbandon = false;
                if (Entry.OnTickCallbackFn != null)
                {
                    bAbandon = Entry.OnTickCallbackFn(this, Entry.TimeElapsed, Progress);

                    if (bAbandon)
                        Owner.AbandonedInteraction(Performer, this);
                }

                if (bAbandon)
                    PerformersToCleanup.Add(Performer);

                // interaction finished?
                if (Entry.TimeElapsed >= Entry.Duration)
                {
                    Owner.FinishedInteraction(Performer, this);
                    PerformersToCleanup.Add(Performer);
                }
            }

            foreach (var Performer in PerformersToCleanup)
                CompleteInteraction(Performer, CurrentPerformers[Performer]);
        }

        protected void CompleteInteraction(IInteractionPerformer InPerformer, PerformerInfo InEntry)
        {
            if (InEntry.OnCompletedCallbackFn != null)
                InEntry.OnCompletedCallbackFn.Invoke(this);

            if (CurrentPerformers.ContainsKey(InPerformer))
                UnlockInteraction(InPerformer);
        }
    }
}
