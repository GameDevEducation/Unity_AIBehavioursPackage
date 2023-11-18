using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class SMTransition_StateStatus : SMTransitionBase
    {
        ESMStateStatus RequiredStatus;

        public SMTransition_StateStatus(ESMStateStatus InRequiredStatus)
        {
            RequiredStatus = InRequiredStatus;
        }

        public bool Handles(ESMStateStatus InStatusToCheck)
        {
            return RequiredStatus == InStatusToCheck;
        }

        protected override ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return InCurrentState.CurrentStatus == RequiredStatus ? ESMTransitionResult.Valid : ESMTransitionResult.Invalid;
        }
    }
}
