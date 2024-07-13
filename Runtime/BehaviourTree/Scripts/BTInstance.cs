using CharacterCore;
using CommonCore;

namespace BehaviourTree
{
    public class BTInstance : IBehaviourTree
    {
        public INavigationInterface NavigationInterface { get; protected set; }
        public ILookHandler LookAtInterface { get; protected set; }
        public IInteractionSelector InteractionInterface { get; protected set; }
        public IInteractionPerformer PerformerInterface { get; protected set; }
        public Blackboard<FastName> LinkedBlackboard { get; protected set; }

        public IBTNode RootNode { get; protected set; }

        public string DebugDisplayName { get; protected set; } = "Behaviour Tree";

        public IBTNode AddChildToRootNode(IBTNode InNode)
        {
            InNode.SetOwningTree(this);
            return (RootNode as IBTFlowNode).AddChild(InNode);
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (bInIsSelected)
            {
                InDebugger.AddTextLine($"Tree: {DebugDisplayName}");
            }

            GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        public void Initialise(INavigationInterface InNavigationInterface,
            ILookHandler InLookAtInterface,
            IInteractionSelector InInteractionInterface,
            IInteractionPerformer InPerformerInterface,
            Blackboard<FastName> InBlackboard,
            string InRootNodeName = "Root")
        {
            if (!string.IsNullOrEmpty(InRootNodeName))
                DebugDisplayName = InRootNodeName;

            LinkedBlackboard = InBlackboard;
            NavigationInterface = InNavigationInterface;
            LookAtInterface = InLookAtInterface;
            InteractionInterface = InInteractionInterface;
            PerformerInterface = InPerformerInterface;
            RootNode = new BTFlowNode_Selector(DebugDisplayName);
            RootNode.SetOwningTree(this);
        }

        public void Reset()
        {
            RootNode.Reset();
        }

        public EBTNodeResult Tick(float InDeltaTime)
        {
            return RootNode.Tick(InDeltaTime);
        }

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            RootNode.GatherDebugData(InDebugger, bInIsSelected);
        }
    }
}
