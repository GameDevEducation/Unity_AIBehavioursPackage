using CommonCore;

namespace HybridGOAP
{
    public interface IGOAPBrain : IDebuggableObject
    {
        Blackboard<FastName> CurrentBlackboard { get; }
    }
}
