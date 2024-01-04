using CommonCore;

namespace StateMachine
{
    public enum ESMStateStatus
    {
        Uninitialised,
        Running,
        Failed,
        Finished
    }

    public interface ISMState : IDebuggable
    {
        ESMStateStatus CurrentStatus { get; }

        ISMState AddTransition(ISMTransition InTransition, ISMState InNewState);
        void AddDefaultTransitions(ISMState InFinishedState, ISMState InFailedState);

        void EvaluateTransitions(Blackboard<FastName> InBlackboard, out ISMState OutNextState);

        ESMStateStatus OnEnter(Blackboard<FastName> InBlackboard);
        ESMStateStatus OnTick(Blackboard<FastName> InBlackboard, float InDeltaTime);
        void OnExit(Blackboard<FastName> InBlackboard);
    }
}
