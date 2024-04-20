using System.Collections;
using System.Collections.Generic;

namespace BehaviourTree
{
    public class BTFlowNode_Sequence : BTFlowNodeBase
    {
        public override bool HasChildren => (Children != null) && (Children.Count > 0);

        public override string DebugDisplayName { get; protected set; } = "Sequence";

        List<IBTNode> Children = new();
        IBTNode LastChild = null;

        public BTFlowNode_Sequence(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        public override IBTNode AddChild(IBTNode InNode)
        {
            InNode.SetOwningTree(OwningTree);
            Children.Add(InNode);

            LastChild = InNode;

            return LastChild;
        }

        public override IEnumerator GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        protected override bool OnTick_Children(float InDeltaTime)
        {
            bool bTickedAnyNodes = false;

            foreach (IBTNode Child in Children)
            {
                // skip if already completed
                if (Child.LastStatus == EBTNodeResult.Succeeded)
                    continue;

                LastStatus = Child.Tick(InDeltaTime);
                bTickedAnyNodes = true;

                // if the child failed to tick then exit
                if (LastStatus == EBTNodeResult.Failed)
                    break;

                // if the child succeeded but is not the last then switch to in progress
                if ((LastStatus == EBTNodeResult.Succeeded) && (Child != LastChild))
                {
                    LastStatus = EBTNodeResult.InProgress;
                    break;
                }

                // if the child succeeded or is in progress then we are done
                if ((LastStatus == EBTNodeResult.Succeeded) || (LastStatus == EBTNodeResult.InProgress))
                    break;
            }

            return bTickedAnyNodes;
        }
    }
}
