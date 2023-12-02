using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public enum ESMTransitionResult
    {
        Invalid,
        Valid
    }

    public interface ISMTransition
    {
        ESMTransitionResult Evaluate(ISMState InCurrentState, Blackboard<FastName> InBlackboard);

        // only override if using this to specifically check for a transition that is looking or one of the state statuses
        bool HandlesStateStatus(ESMStateStatus InStatusToCheck) {  return false; }
    }
}
