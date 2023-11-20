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

        public SMState_CalculateMoveLocation(INavigationInterface InNavInterface, float InSearchRange, System.Func<Vector3> InGetSearchLocationFn, string InCustomName = null) :
            base(InCustomName)
        { 
            Navigation = InNavInterface;
            SearchRange = InSearchRange;
            GetSearchLocationFn = InGetSearchLocationFn;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            Vector3 MoveLocation = GetSearchLocationFn();
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            var Self = GetOwner(InBlackboard);

            // find the nearest navigable point
            MoveLocation = Navigation.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
            InBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                return ESMStateStatus.Failed;
            }

            return ESMStateStatus.Finished;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return CurrentStatus;
        }
    }
}
