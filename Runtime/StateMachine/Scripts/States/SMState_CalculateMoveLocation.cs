using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class SMState_CalculateMoveLocation : SMStateBase
    {
        INavigationInterface Navigation;
        float SearchRange;
        System.Func<Vector3> GetSearchLocationFn;
        bool bContinuousMode;
        float RecalculateThresholdSq;

        Vector3 LastSearchLocation;

        public SMState_CalculateMoveLocation(INavigationInterface InNavInterface, float InSearchRange, System.Func<Vector3> InGetSearchLocationFn, bool bInContinuousMode = false, float InRecalculateThreshold = 0.1f, string InCustomName = null) :
            base(InCustomName)
        { 
            Navigation = InNavInterface;
            SearchRange = InSearchRange;
            GetSearchLocationFn = InGetSearchLocationFn;
            bContinuousMode = bInContinuousMode;
            RecalculateThresholdSq = InRecalculateThreshold * InRecalculateThreshold;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            Vector3 MoveLocation = GetSearchLocationFn();
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            LastSearchLocation = MoveLocation;

            var Self = GetOwner(InBlackboard);

            // find the nearest navigable point
            MoveLocation = Navigation.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
            InBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            return bContinuousMode ? ESMStateStatus.Running : ESMStateStatus.Finished;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
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

                    var Self = GetOwner(InBlackboard);

                    // find the nearest navigable point
                    MoveLocation = Navigation.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
                    InBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
                    if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
                    {
                        return ESMStateStatus.Failed;
                    }
                }

                return ESMStateStatus.Running;
            }

            return CurrentStatus;
        }
    }
}
