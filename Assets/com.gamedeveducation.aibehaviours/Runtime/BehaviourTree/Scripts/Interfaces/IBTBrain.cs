using CommonCore;

namespace BehaviourTree
{
    public interface IBTBrain : IDebuggableObject
    {
        Blackboard<FastName> LinkedBlackboard { get; }
    }
}
