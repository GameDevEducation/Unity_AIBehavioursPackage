using CommonCore;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class BTDecoratorBase : IBTDecorator
    {
        public GameObject Self => LinkedBlackboard.GetGameObject(CommonCore.Names.Self);

        public IBehaviourTree OwningTree { get; protected set; }

        public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;

        public abstract bool CanPostProcessTickResult { get; }

        public abstract string DebugDisplayName { get; protected set; }

        public bool bIsInverted { get; protected set; } = false;
        protected bool? bLastResult;

        protected BTDecoratorBase(bool bInIsInverted = false)
        {
            bIsInverted = bInIsInverted;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            if (bLastResult != null)
                InDebugger.AddTextLine($"Decorator: {(bIsInverted ? "NOT " : "")}{DebugDisplayName} = {(bLastResult.Value ? "Pass" : "Fail")}");
            else
                InDebugger.AddTextLine($"Decorator: {(bIsInverted ? "NOT " : "")}{DebugDisplayName} = Not Evaluated");

            GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {

        }

        public virtual EBTNodeResult PostProcessTickResult(EBTNodeResult InResult)
        {
            return InResult;
        }

        public void SetOwningTree(IBehaviourTree InOwningTree)
        {
            OwningTree = InOwningTree;
        }

        public bool Tick(float InDeltaTime)
        {
            bLastResult = OnEvaluate(InDeltaTime);

            return bIsInverted ? !bLastResult.Value : bLastResult.Value;
        }

        protected abstract bool OnEvaluate(float InDeltaTime);
    }
}
