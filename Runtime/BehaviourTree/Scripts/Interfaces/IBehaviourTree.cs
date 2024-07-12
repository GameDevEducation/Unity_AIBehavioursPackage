using CharacterCore;
using CommonCore;

namespace BehaviourTree
{
    public interface IBehaviourTree : IDebuggable
    {
        INavigationInterface NavigationInterface { get; }
        ILookHandler LookAtInterface { get; }
        Blackboard<FastName> LinkedBlackboard { get; }
        IBTNode RootNode { get; }

        void Initialise(INavigationInterface InNavigationInterface, ILookHandler InLookAtInterface,
                        Blackboard<FastName> InBlackboard, string InRootNodeName = "Root");

        void Reset();

        IBTNode AddChildToRootNode(IBTNode InNode);

        EBTNodeResult Tick(float InDeltaTime);
    }
}
