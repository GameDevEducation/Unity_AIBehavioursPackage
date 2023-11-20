using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public abstract class SMTransitionBase : ISMTransition
    {
        protected GameObject GetOwner(Blackboard<FastName> InBlackboard)
        {
            return InBlackboard.GetGameObject(CommonCore.Names.Self);
        }

        public ESMTransitionResult Evaluate(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return EvaluateInternal(InCurrentState, InBlackboard);
        }

        protected abstract ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard);
    }
}
