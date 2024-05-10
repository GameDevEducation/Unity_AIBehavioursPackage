using CommonCore;
using UnityEngine;

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
        ISMInstance Owner { get; }
        Blackboard<FastName> LinkedBlackboard { get; }
        GameObject Self { get; }

        void BindToOwner(ISMInstance InOwner);

        ESMStateStatus CurrentStatus { get; }

        ISMState AddTransition(ISMTransition InTransition, ISMState InNewState);
        void AddDefaultTransitions(ISMState InFinishedState, ISMState InFailedState);

        void EvaluateTransitions(out ISMState OutNextState);

        ESMStateStatus OnEnter();
        ESMStateStatus OnTick(float InDeltaTime);
        void OnExit();
    }
}
