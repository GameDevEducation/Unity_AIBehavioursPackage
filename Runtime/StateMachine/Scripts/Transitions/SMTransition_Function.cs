using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public class SMTransition_Function : SMTransitionBase
    {
        System.Func<ISMState, Blackboard<FastName>, ESMTransitionResult> EvaluateTransitionFn;

        public SMTransition_Function(System.Func<ISMState, Blackboard<FastName>, ESMTransitionResult> InEvaluateTransitionFn)
        {
            EvaluateTransitionFn = InEvaluateTransitionFn;
        }

        protected override ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return EvaluateTransitionFn == null ? ESMTransitionResult.Invalid : EvaluateTransitionFn(InCurrentState, InBlackboard);
        }
    }
}
