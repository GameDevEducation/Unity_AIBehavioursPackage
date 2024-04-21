using CommonCore;

namespace BehaviourTree
{
    public interface IBTBrain : IDebuggableObject
    {
        Blackboard<FastName> CurrentBlackboard { get; }
    }
}
