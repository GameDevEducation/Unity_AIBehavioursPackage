using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class SMState_MoveTo : SMStateBase
    {
        INavigationInterface Navigation;
        float StoppingDistance;

        public SMState_MoveTo(INavigationInterface InNavInterface, float InStoppingDistance, string InCustomName = null) :
            base(InCustomName)
        {
            Navigation = InNavInterface;
            StoppingDistance = InStoppingDistance;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            Vector3 Destination = CommonCore.Constants.InvalidVector3Position;
            if (InBlackboard.TryGet(CommonCore.Names.MoveToLocation, out Destination, CommonCore.Constants.InvalidVector3Position))
            {
                if (Destination == CommonCore.Constants.InvalidVector3Position)
                    return ESMStateStatus.Failed;

                var Self = GetOwner(InBlackboard);

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

            if (Navigation.IsPathfindingOrMoving(Self))
                return ESMStateStatus.Running;
            else if (Navigation.IsAtDestination(Self))
                return ESMStateStatus.Finished;

            return ESMStateStatus.Failed;
        }
    }
}
