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
            CurrentBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);

            GameObject PreviousAwarenessTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Awareness_PreviousBestTarget, out PreviousAwarenessTarget, null);

            // if we're changing targets?
            if ((PreviousAwarenessTarget != null) && (CurrentAwarenessTarget != PreviousAwarenessTarget))
            {
                // clear the look at target if it matches the old awareness target
                GameObject CurrentLookAtTarget = null;
                CurrentBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);

                if (CurrentLookAtTarget == PreviousAwarenessTarget)
                    CurrentBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

                // clear the focus/move target if it matches the old awareness target
                GameObject CurrentTarget = null;
                CurrentBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);

                if (CurrentTarget == PreviousAwarenessTarget)
                    CurrentBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            }
        }

        #region Point of Interest (POI) Helpers
        public void PickSuitablePOI(GameObject InQuerier, System.Action<GameObject> InCallbackFn)
        {
            // first priority is our awareness target
            GameObject CurrentAwarenessTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);
            if (IsPOIValid(CurrentAwarenessTarget))
            {
                InCallbackFn(CurrentAwarenessTarget);
                return;
            }

            // next priority is our interaction target
            SmartObject CurrentInteractionSO = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out CurrentInteractionSO, null);
            if ((CurrentInteractionSO != null) && IsPOIValid(CurrentInteractionSO.gameObject))
            {
                InCallbackFn(CurrentInteractionSO.gameObject);
                return;
            }

            // next priority is our current focus target
            GameObject CurrentTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);
            if (IsPOIValid(CurrentTarget))
            {
                InCallbackFn(CurrentTarget);
                return;
            }

            // next priority is our current look target
            GameObject CurrentLookAtTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);
            if (IsPOIValid(CurrentLookAtTarget))
            {
                InCallbackFn(CurrentLookAtTarget);
                return;
            }

            InCallbackFn(null);
        }

        bool IsPOIValid(GameObject InPOI)
        {
            return InPOI != null;
        }
        #endregion

        #region Interactable Helpers
        public void GetUseInteractableDesire(GameObject InQuerier, float InSearchRange, System.Action<float> InCallbackFn)
        {
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
