using CommonCore;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class BTServiceBase : IBTService
    {
        public GameObject Self => LinkedBlackboard.GetGameObject(CommonCore.Names.Self);

        public IBehaviourTree OwningTree { get; protected set; }

        public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;

        public abstract string DebugDisplayName { get; protected set; }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            InDebugger.AddTextLine($"Service: {DebugDisplayName}");

            GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
        }

        public void SetOwningTree(IBehaviourTree InOwningTree)
        {
            OwningTree = InOwningTree;
        }

        public abstract bool Tick(float InDeltaTime);
    }
}
