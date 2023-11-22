using CommonCore;
using HybridGOAP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HybridGOAPExample
{
    public class GOAPBrainWrapper : GOAPBrainBase
    {
        BaseNavigation NavSystem;

        protected override void ConfigureBlackboard()
        {
        }

        protected override void ConfigureBrain()
        {
            NavSystem = GetComponent<BaseNavigation>();
        }

        #region Navigation Helpers
        public void FindNearestNavigableLocation(Vector3 InSearchLocation, float InSearchRange, System.Action<Vector3> InCallbackFn)
        {
            if (NavSystem == null)
                InCallbackFn(CommonCore.Constants.InvalidVector3Position);

            Vector3 FoundPosition = CommonCore.Constants.InvalidVector3Position;

            if (NavSystem.FindNearestPoint(InSearchLocation, InSearchRange, out FoundPosition))
                InCallbackFn(FoundPosition);
            else
                InCallbackFn(CommonCore.Constants.InvalidVector3Position);
        }

        public void SetMoveLocation(Vector3 InDestination, float InStoppingDistance, System.Action<bool> InCallbackFn)
        {
            if (NavSystem == null)
                InCallbackFn(false);

            bool bResult = NavSystem.SetDestination(InDestination);
            if (bResult)
            {
                NavSystem.SetDestinationReachedThreshold(InStoppingDistance);
                InCallbackFn(true);
            }
            else
                InCallbackFn(false);
        }

        public void IsPathfindingOrMovingFn(System.Action<bool> InCallbackFn)
        {
            if (NavSystem == null)
                InCallbackFn(false);

            InCallbackFn(NavSystem.IsFindingOrFollowingPath);
        }

        public void IsAtDestinationFn(System.Action<bool> InCallbackFn)
        {
            if (NavSystem == null)
                InCallbackFn(false);

            InCallbackFn(NavSystem.IsAtDestination);
        }

        public void StopMovingFn(System.Action<bool> InCallbackFn)
        {
            if (NavSystem == null)
                InCallbackFn(false);

            NavSystem.StopMovement();
            InCallbackFn(true);
        }
        #endregion

        #region Awareness Helpers
        public void SetDetectedTarget(GameObject InTarget)
        {
            GameObject CurrentAwarenessTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);

            // if we're changing targets?
            if ((CurrentAwarenessTarget != null) && (CurrentAwarenessTarget != InTarget))
            {
                // clear the look at target if it matches the old awareness target
                GameObject CurrentLookAtTarget = null;
                CurrentBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);

                if (CurrentLookAtTarget == CurrentAwarenessTarget)
                    CurrentBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

                // clear the focus/move target if it matches the old awareness target
                GameObject CurrentTarget = null;
                CurrentBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);

                if (CurrentTarget == CurrentAwarenessTarget)
                    CurrentBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            }

            CurrentBlackboard.Set(CommonCore.Names.Awareness_BestTarget, InTarget);
        }
        #endregion

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
        public void GetUseInteractableDesire(GameObject InQuerier, System.Action<float> InCallbackFn)
        {
            InCallbackFn(0.25f);
        }

        public void SelectRandomInteraction(GameObject InQuerier, System.Action<SmartObject, BaseInteraction> InCallbackFn) 
        {
            List<System.Tuple<SmartObject, BaseInteraction>> CandidateInteractions = new();

            // loop through all of the smart objects
            foreach(var CandidateSO in SmartObjectManager.Instance.RegisteredObjects)
            {
                // loop through all of the interactions
                foreach(var CandidateInteraction in CandidateSO.Interactions)
                {
                    if (!CandidateInteraction.CanPerform())
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
