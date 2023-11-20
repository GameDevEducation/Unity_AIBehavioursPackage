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
    }
}
