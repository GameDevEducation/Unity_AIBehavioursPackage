using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class SMTransition_Always : SMTransitionBase
    {
        protected override ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return ESMTransitionResult.Valid;
        }
    }
}
