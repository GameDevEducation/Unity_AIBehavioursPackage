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
    }
}
