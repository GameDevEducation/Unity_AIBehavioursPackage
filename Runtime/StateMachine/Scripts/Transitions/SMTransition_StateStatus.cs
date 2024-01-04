using CommonCore;

namespace StateMachine
{
    public class SMTransition_StateStatus : SMTransitionBase
    {
        ESMStateStatus RequiredStatus;

        public SMTransition_StateStatus(ESMStateStatus InRequiredStatus)
        {
            RequiredStatus = InRequiredStatus;
        }

        public bool HandlesStateStatus(ESMStateStatus InStatusToCheck)
        {
            return RequiredStatus == InStatusToCheck;
        }

        protected override ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return InCurrentState.CurrentStatus == RequiredStatus ? ESMTransitionResult.Valid : ESMTransitionResult.Invalid;
        }

        public static ISMTransition IfHasFinished()
        {
            return new SMTransition_StateStatus(ESMStateStatus.Finished);
        }

        public static ISMTransition IfHasFailed()
        {
            return new SMTransition_StateStatus(ESMStateStatus.Failed);
        }
    }
}
