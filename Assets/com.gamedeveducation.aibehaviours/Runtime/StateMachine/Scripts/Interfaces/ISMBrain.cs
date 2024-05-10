using CommonCore;

namespace StateMachine
{
    public interface ISMBrain : IDebuggableObject
    {
        Blackboard<FastName> LinkedBlackboard { get; }
    }
}
