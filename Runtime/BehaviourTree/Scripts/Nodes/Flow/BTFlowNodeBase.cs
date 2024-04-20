using CommonCore;
using System.Collections;

namespace BehaviourTree
{
    public abstract class BTFlowNodeBase : BTNodeBase, IBTFlowNode
    {
        public override void Reset()
        {
            base.Reset();

            if (HasChildren)
            {
                foreach (IBTNode Child in this)
                    Child.Reset();
            }
        }

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (bInIsSelected)
            {
                string Prefix = LastStatus == EBTNodeResult.InProgress ? "<b>" : "";
                string Suffix = LastStatus == EBTNodeResult.InProgress ? "</b>" : "";

                InDebugger.AddTextLine($"{Prefix}Flow: {DebugDisplayName}{Suffix}");
            }

            base.GatherDebugDataInternal(InDebugger, bInIsSelected);

            if (bInIsSelected)
            {
                InDebugger.PushIndent();

                foreach (IBTNode Child in this)
                    Child.GatherDebugData(InDebugger, bInIsSelected);

                InDebugger.PopIndent();
            }
        }

        public abstract IBTNode AddChild(IBTNode InNode);
        public abstract IEnumerator GetEnumerator();

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            return true;
        }
    }
}
