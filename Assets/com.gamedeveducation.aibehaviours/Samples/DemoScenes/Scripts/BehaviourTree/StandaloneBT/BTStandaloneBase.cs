using BehaviourTree;
using CommonCore;
using UnityEngine;
using UnityEngine.Events;

namespace DemoScenes
{
    public abstract class BTStandaloneBase : BTBrainBase
    {
        [SerializeField] float DefaultInteractableSearchRange = 30f;

        [Header("Look at POI")]
        [SerializeField] UnityEvent<Transform> OnSetPOI = new();

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
        public GameObject PickSuitablePOI(GameObject InQuerier)
        {
            // first priority is our awareness target
            GameObject CurrentAwarenessTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);
            if (IsPOIValid(CurrentAwarenessTarget))
                return CurrentAwarenessTarget;

            // next priority is our interaction target
            SmartObject CurrentInteractionSO = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out CurrentInteractionSO, null);
            if ((CurrentInteractionSO != null) && IsPOIValid(CurrentInteractionSO.gameObject))
                return CurrentInteractionSO.gameObject;

            // next priority is our current focus target
            GameObject CurrentTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);
            if (IsPOIValid(CurrentTarget))
                return CurrentTarget;

            // next priority is our current look target
            GameObject CurrentLookAtTarget = null;
            CurrentBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);
            if (IsPOIValid(CurrentLookAtTarget))
                return CurrentLookAtTarget;

            return null;
        }

        bool IsPOIValid(GameObject InPOI)
        {
            return InPOI != null;
        }

        protected void SetPOIFn(GameObject InPOI)
        {
            if (InPOI != null)
                OnSetPOI.Invoke(InPOI.transform);
        }
        #endregion
    }
}
