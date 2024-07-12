using CommonCore;
using HybridGOAP;
using System.Collections.Generic;
using UnityEngine;

namespace DemoScenes
{
    public class GOAPBrainWrapper : GOAPBrainBase
    {
        [SerializeField] float DefaultInteractableSearchRange = 30f;

        protected override void ConfigureBlackboard()
        {
        }

        protected override void ConfigureBrain()
        {
        }

        protected override void OnPreTickBrain(float InDeltaTime)
        {
            base.OnPreTickBrain(InDeltaTime);

            GameObject CurrentAwarenessTarget = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);

            GameObject PreviousAwarenessTarget = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_PreviousBestTarget, out PreviousAwarenessTarget, null);

            // if we're changing targets?
            if ((PreviousAwarenessTarget != null) && (CurrentAwarenessTarget != PreviousAwarenessTarget))
            {
                // clear the look at target if it matches the old awareness target
                GameObject CurrentLookAtTarget = null;
                LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);

                if (CurrentLookAtTarget == PreviousAwarenessTarget)
                    LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

                // clear the focus/move target if it matches the old awareness target
                GameObject CurrentTarget = null;
                LinkedBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);

                if (CurrentTarget == PreviousAwarenessTarget)
                    LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            }
        }

        #region Interactable Helpers
        public void GetUseInteractableDesire(GameObject InQuerier, float InSearchRange, System.Action<float> InCallbackFn)
        {
            // if we already have an interaction block switching
            SmartObject TargetSO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out TargetSO, null);
            if (TargetSO != null)
            {
                BaseInteraction TargetInteraction = null;
                LinkedBlackboard.TryGet(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
                if (TargetInteraction != null)
                {
                    InCallbackFn(0.25f);
                    return;
                }
            }

            int NumCandidateInteractables = 0;

            float MaxInteractionRange = InSearchRange > 0 ? InSearchRange : DefaultInteractableSearchRange;
            float MaxInteractionRangeSq = MaxInteractionRange * MaxInteractionRange;

            // loop through all of the smart objects
            foreach (var CandidateSO in SmartObjectManager.Instance.RegisteredObjects)
            {
                // loop through all of the interactions
                foreach (var CandidateInteraction in CandidateSO.Interactions)
                {
                    if (!CandidateInteraction.CanPerform())
                        continue;

                    if ((CandidateSO.InteractionPoint - transform.position).sqrMagnitude > MaxInteractionRangeSq)
                        continue;

                    if ((Navigation != null) && !Navigation.IsLocationReachable(transform.position, CandidateSO.InteractionPoint))
                        continue;

                    ++NumCandidateInteractables;
                }
            }

            // no interactions?
            if (NumCandidateInteractables == 0)
            {
                InCallbackFn(float.MinValue);
                return;
            }

            InCallbackFn(0.25f);
        }

        public void SelectRandomInteraction(GameObject InQuerier, float InSearchRange, System.Action<SmartObject, BaseInteraction> InCallbackFn)
        {
            List<System.Tuple<SmartObject, BaseInteraction>> CandidateInteractions = new();

            float MaxInteractionRange = InSearchRange > 0 ? InSearchRange : DefaultInteractableSearchRange;
            float MaxInteractionRangeSq = MaxInteractionRange * MaxInteractionRange;

            // loop through all of the smart objects
            foreach (var CandidateSO in SmartObjectManager.Instance.RegisteredObjects)
            {
                // loop through all of the interactions
                foreach (var CandidateInteraction in CandidateSO.Interactions)
                {
                    if (!CandidateInteraction.CanPerform())
                        continue;

                    if ((CandidateSO.InteractionPoint - transform.position).sqrMagnitude > MaxInteractionRangeSq)
                        continue;

                    if ((Navigation != null) && !Navigation.IsLocationReachable(transform.position, CandidateSO.InteractionPoint))
                        continue;

                    CandidateInteractions.Add(new System.Tuple<SmartObject, BaseInteraction>(CandidateSO, CandidateInteraction));
                }
            }

            // no interactions?
            if (CandidateInteractions.Count == 0)
            {
                InCallbackFn(null, null);
                return;
            }

            // pick a random interaction
            var SelectedIndex = CandidateInteractions.Count == 1 ? 0 : Random.Range(0, CandidateInteractions.Count);
            InCallbackFn(CandidateInteractions[SelectedIndex].Item1, CandidateInteractions[SelectedIndex].Item2);
        }
        #endregion
    }
}
