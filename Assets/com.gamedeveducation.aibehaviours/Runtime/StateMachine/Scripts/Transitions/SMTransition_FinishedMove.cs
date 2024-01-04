using CommonCore;

namespace StateMachine
{
    public class SMTransition_FinishedMove : SMTransitionBase
    {
        INavigationInterface Navigation;

        public SMTransition_FinishedMove(INavigationInterface InNavInterface)
        {
            Navigation = InNavInterface;
        }

        protected override ESMTransitionResult EvaluateInternal(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            var Self = GetOwner(InBlackboard);

            return Navigation.IsAtDestination(Self) ? ESMTransitionResult.Valid : ESMTransitionResult.Invalid;
        }
    }
}
