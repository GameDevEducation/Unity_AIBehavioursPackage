using CommonCore;
using UnityEngine;

namespace StateMachine
{
    public class SMState_CalculateMoveLocation : SMStateBase
    {
        INavigationInterface Navigation;
        float SearchRange;
        System.Func<Vector3> GetSearchLocationFn;
        System.Func<Vector3> GetEndOrientationFn;
        bool bContinuousMode;
        float RecalculateThresholdSq;

        Vector3 LastSearchLocation;

        public SMState_CalculateMoveLocation(INavigationInterface InNavInterface, float InSearchRange, 
                                             System.Func<Vector3> InGetSearchLocationFn,
                                             System.Func<Vector3> InGetEndOrientationFn,
                                             bool bInContinuousMode = false, float InRecalculateThreshold = 0.1f, string InDisplayName = null) :
            base(InDisplayName)
        {
            Navigation = InNavInterface;
            SearchRange = InSearchRange;
            GetSearchLocationFn = InGetSearchLocationFn;
            GetEndOrientationFn = InGetEndOrientationFn;
            bContinuousMode = bInContinuousMode;
            RecalculateThresholdSq = InRecalculateThreshold * InRecalculateThreshold;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            Vector3 MoveLocation = GetSearchLocationFn();
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            LastSearchLocation = MoveLocation;

            // find the nearest navigable point
            MoveLocation = Navigation.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
            LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
            LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, CommonCore.Constants.InvalidVector3Position);
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            if (GetEndOrientationFn != null)
                LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, GetEndOrientationFn());

            return bContinuousMode ? ESMStateStatus.Running : ESMStateStatus.Finished;
        }

        protected override void OnExitInternal()
        {
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            if (bContinuousMode)
            {
                Vector3 MoveLocation = GetSearchLocationFn();
                if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
                {
                    return ESMStateStatus.Failed;
                }

                // if moved far enough recalculate
                if ((MoveLocation - LastSearchLocation).sqrMagnitude >= RecalculateThresholdSq)
                {
                    LastSearchLocation = MoveLocation;

                    // find the nearest navigable point
                    MoveLocation = Navigation.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
                    LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
                    LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, CommonCore.Constants.InvalidVector3Position);
                    if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
                    {
                        return ESMStateStatus.Failed;
                    }

                    if (GetEndOrientationFn != null)
                        LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, GetEndOrientationFn());
                }

                return ESMStateStatus.Running;
            }

            return CurrentStatus;
        }
    }
}
