using CommonCore;

namespace BehaviourTree
{
    public interface IBehaviourTree : IDebuggable
    {
        INavigationInterface NavigationInterface { get; }
        Blackboard<FastName> LinkedBlackboard { get; }
        IBTNode RootNode { get; }

        void Initialise(INavigationInterface InNavigationInterface, Blackboard<FastName> InBlackboard, string InRootNodeName = "Root");

        void Reset();

        IBTNode AddChildToRootNode(IBTNode InNode);

        EBTNodeResult Tick(float InDeltaTime);
    }
}
