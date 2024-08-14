using System.Collections;

namespace BehaviourTree
{
    public class BTFlowNode_Parallel : BTFlowNodeBase
    {
        public override bool HasChildren => PrimaryTree != null;

        public override string DebugDisplayName { get; protected set; } = "Parallel";

        protected IBTNode PrimaryTree;
        protected IBTNode SecondaryTree;

        public BTFlowNode_Parallel(string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        public override IBTNode AddChild(IBTNode InNode)
        {
            throw new System.InvalidOperationException($"{this} Parallel BT nodes do not support AddChild. Use SetPrimary or SetSecondary");
        }

        public override IEnumerator GetEnumerator()
        {
            return new BTFlowNode_Parallel_Enumerator(PrimaryTree, SecondaryTree);
        }

        protected override bool OnTick_Children(float InDeltaTime)
        {
            if (PrimaryTree.HasFinished)
                return false;

            LastStatus = PrimaryTree.Tick(InDeltaTime);

            if ((SecondaryTree != null) && (LastStatus != EBTNodeResult.Failed))
            {
                if (SecondaryTree.HasFinished)
                    SecondaryTree.Reset();

                SecondaryTree.Tick(InDeltaTime);
            }

            return true;
        }

        public IBTNode SetPrimary(IBTNode InNode)
        {
            InNode.SetOwningTree(OwningTree);
            PrimaryTree = InNode;

            return PrimaryTree;
        }

        public IBTNode SetSecondary(IBTNode InNode)
        {
            InNode.SetOwningTree(OwningTree);
            SecondaryTree = InNode;

            return SecondaryTree;
        }
    }

    public class BTFlowNode_Parallel_Enumerator : IEnumerator
    {
        IBTNode PrimaryTree;
        IBTNode SecondaryTree;
        int Index = -1;

        public BTFlowNode_Parallel_Enumerator(IBTNode InPrimaryTree, IBTNode InSecondaryTree)
        {
            PrimaryTree = InPrimaryTree;
            SecondaryTree = InSecondaryTree;
        }

        public bool MoveNext()
        {
            ++Index;

            return Index < 2;
        }

        public void Reset()
        {
            Index = -1;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public IBTNode Current
        {
            get
            {
                if (Index == 0)
                    return PrimaryTree;
                else if (Index == 1)
                    return SecondaryTree;

                throw new System.InvalidOperationException();
            }
        }
    }
}
