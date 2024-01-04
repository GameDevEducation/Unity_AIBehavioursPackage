using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    public enum EInteractionType
    {
        Instantaneous = 0,
        OverTime = 1,
        AfterTime = 2
    }

    [System.Serializable]
    public class InteractionOutcome
    {
        public string Description;
        [Range(0f, 1f)] public float Weighting = 1f;
        public bool AbandonInteraction = false;

        public float NormalisedWeighting { get; set; } = -1f;
    }

    public abstract class BaseInteraction : MonoBehaviour
    {
        [SerializeField] protected string _DisplayName;
        [SerializeField] protected EInteractionType _InteractionType = EInteractionType.Instantaneous;
        [SerializeField] protected float _Duration = 0f;
        [SerializeField]
        InteractionOutcome[] _Outcomes = new InteractionOutcome[] { new InteractionOutcome() {
        Weighting = 1f, Description = ""
    } };

        bool OutcomeWeightingsNormalised = false;

        public string DisplayName => _DisplayName;
        public EInteractionType InteractionType => _InteractionType;
        public float Duration => _Duration;

        public abstract bool CanPerform();
        public abstract bool LockInteraction(GameObject performer);
        public abstract bool Perform(GameObject performer, UnityAction<BaseInteraction> onCompleted);
        public abstract bool UnlockInteraction(GameObject performer);

        public bool ApplyInteractionEffects(GameObject performer, float proportion, bool rollForOutcomes)
        {
            bool abandonInteraction = false;

            InteractionOutcome selectedOutcome = null;
            if (rollForOutcomes && _Outcomes.Length > 0)
            {
                // normalise the weightings if needed
                if (!OutcomeWeightingsNormalised)
                {
                    OutcomeWeightingsNormalised = true;
                    float weightingSum = 0;
                    foreach (var outcome in _Outcomes)
                    {
                        weightingSum += outcome.Weighting;
                    }

                    foreach (var outcome in _Outcomes)
                    {
                        outcome.NormalisedWeighting = outcome.Weighting / weightingSum;
                    }
                }

                // pick an outcome
                float randomRoll = Random.value;
                foreach (var outcome in _Outcomes)
                {
                    if (randomRoll <= outcome.NormalisedWeighting)
                    {
                        selectedOutcome = outcome;
                        if (selectedOutcome.AbandonInteraction)
                            abandonInteraction = true;

                        break;
                    }

                    randomRoll -= outcome.NormalisedWeighting;
                }
            }

            if (selectedOutcome != null)
            {
                if (!string.IsNullOrEmpty(selectedOutcome.Description))
                    Debug.Log($"Outcome was {selectedOutcome.Description}");
            }

            return !abandonInteraction;
        }
    }
}
