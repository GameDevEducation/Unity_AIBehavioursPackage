using CommonCore;
using UnityEngine;

namespace StateMachine
{
    public class SMState_MoveTo : SMStateBase
    {
        INavigationInterface Navigation;
        float StoppingDistance;
        bool bContinuousMode;
        Vector3 LastDestination;
        float RepathThresholdSq;

        public SMState_MoveTo(INavigationInterface InNavInterface, float InStoppingDistance, bool bInContinuousMode = false, float InRepathThreshold = 0.1f, string InDisplayName = null) :
            base(InDisplayName)
        {
            Navigation = InNavInterface;
            StoppingDistance = InStoppingDistance;
            bContinuousMode = bInContinuousMode;
            RepathThresholdSq = InRepathThreshold * InRepathThreshold;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            Vector3 Destination = CommonCore.Constants.InvalidVector3Position;
            if (InBlackboard.TryGet(CommonCore.Names.MoveToLocation, out Destination, CommonCore.Constants.InvalidVector3Position))
            {
                if (Destination == CommonCore.Constants.InvalidVector3Position)
                    return ESMStateStatus.Failed;

                var Self = GetOwner(InBlackboard);

                LastDestination = Destination;
                if (Navigation.SetMoveLocation(Self, Destination, StoppingDistance))
                    return ESMStateStatus.Running;
            }

            return ESMStateStatus.Failed;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
            var Self = GetOwner(InBlackboard);

            if (Navigation.IsPathfindingOrMoving(Self))
                Navigation.StopMoving(Self);
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            var Self = GetOwner(InBlackboard);

            if (bContinuousMode)
            {
                // look for changes to the destination
                Vector3 CurrentDestination = CommonCore.Constants.InvalidVector3Position;
                InBlackboard.TryGet(CommonCore.Names.MoveToLocation, out CurrentDestination, CommonCore.Constants.InvalidVector3Position);

                if (CurrentDestination == CommonCore.Constants.InvalidVector3Position)
                    return ESMStateStatus.Failed;

                // have we moved more than the threshold
                if ((CurrentDestination - LastDestination).sqrMagnitude >= RepathThresholdSq)
                {
                    LastDestination = CurrentDestination;
                    if (Navigation.SetMoveLocation(Self, CurrentDestination, StoppingDistance))
                        return ESMStateStatus.Running;
                }
            }

            if (Navigation.IsPathfindingOrMoving(Self))
                return ESMStateStatus.Running;
            else if (Navigation.HasReachedDestination(Self))
                return bContinuousMode ? ESMStateStatus.Running : ESMStateStatus.Finished;

            return ESMStateStatus.Failed;
        }
    }
}
