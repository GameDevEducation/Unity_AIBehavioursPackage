using BehaviourTree;
using CommonCore;

namespace HybridGOAP
{
    public abstract class GOAPAction_BehaviourTree : GOAPActionBase
    {
        protected IBehaviourTree LinkedBehaviourTree = new BTInstance();

        protected override void OnInitialise()
        {
            LinkedBehaviourTree.Initialise(Navigation, LinkedBlackboard);

            ConfigureBehaviourTree();
        }

        protected override void OnStartAction()
        {
            ResetBehaviourTree();
        }

        protected override void OnContinueAction()
        {
        }

        protected override void OnStopAction()
        {
            ResetBehaviourTree();
        }

        protected override void OnTickAction(float InDeltaTime)
        {
            var Result = LinkedBehaviourTree.Tick(InDeltaTime);

            if (Result == EBTNodeResult.Succeeded)
            {
                ResetBehaviourTree();
                OnBehaviourTreeCompleted_Finished();
            }
            else if (Result == EBTNodeResult.Failed)
            {
                ResetBehaviourTree();
                OnBehaviourTreeCompleted_Failed();
            }
        }

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            LinkedBehaviourTree.GatherDebugData(InDebugger, bInIsSelected);
        }

        protected IBTNode AddChildToRootNode(IBTNode InNode)
        {
            return LinkedBehaviourTree.AddChildToRootNode(InNode);
        }

        protected void ResetBehaviourTree()
        {
            LinkedBehaviourTree.Reset();
            OnBehaviourTreeReset();
        }

        protected abstract void ConfigureBehaviourTree();

        protected virtual void OnBehaviourTreeCompleted_Finished() { }
        protected virtual void OnBehaviourTreeCompleted_Failed() { }
        protected virtual void OnBehaviourTreeReset() { }
    }
}
