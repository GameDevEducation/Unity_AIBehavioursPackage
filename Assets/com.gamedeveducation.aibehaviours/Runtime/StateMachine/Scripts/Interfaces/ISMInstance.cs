using CommonCore;
using UnityEngine;

namespace StateMachine
{
    public interface ISMInstance : IDebuggable
    {
        Blackboard<FastName> LinkedBlackboard { get; }
        GameObject Self { get; }

        void BindToBlackboard(Blackboard<FastName> InBlackboard);

        ISMState AddState(ISMState InState);
        void AddDefaultTransitions(ISMState InFinishedState, ISMState InFailedState);

        void Reset();
        ESMTickResult Tick(float InDeltaTime);
    }
}
