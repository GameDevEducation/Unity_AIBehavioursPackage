using System.Collections;
using System.Collections.Generic;

namespace BehaviourTree
{
    public class BTFlowNode_Selector : BTFlowNodeBase
    {
        public override bool HasChildren => (Children != null) && (Children.Count > 0);

        public override string DebugDisplayName { get; protected set; } = "Selector";

        List<IBTNode> Children = new();
        IBTNode LastChild = null;

        public BTFlowNode_Selector(string InDisplayName = null)
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
                // did this child fail previously?
                if (Child.LastStatus == EBTNodeResult.Failed)
                {
                    // are they now able to run solely due to decorators changing
                    if (!Child.DoDecoratorsNowPermitRunning(InDeltaTime))
                        continue;

                    // reset this node and siblings
                    bool bCanReset = false;
                    foreach (IBTNode OtherChild in Children)
                    {
                        if (OtherChild == Child)
                            bCanReset = true;

                        if (bCanReset)
                            OtherChild.Reset();
                    }
                }

                LastStatus = Child.Tick(InDeltaTime);
                bTickedAnyNodes = true;

                // if this node failed and it isn't the last child then go to in progress
                if ((LastStatus == EBTNodeResult.Failed) && (Child != LastChild))
                {
                    LastStatus = EBTNodeResult.InProgress;
                    break;
                }

                if ((LastStatus == EBTNodeResult.InProgress) || (LastStatus == EBTNodeResult.Succeeded))
                    break;
            }

            return bTickedAnyNodes;
        }
    }
}
