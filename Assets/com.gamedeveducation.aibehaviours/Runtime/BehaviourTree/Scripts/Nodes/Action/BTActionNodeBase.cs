using CommonCore;

namespace BehaviourTree
{
    public abstract class BTActionNodeBase : BTNodeBase, IBTActionNode
    {
        public override bool HasChildren => false;

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (bInIsSelected)
            {
                string Prefix = LastStatus == EBTNodeResult.InProgress ? "<b>* " : "";
                string Suffix = LastStatus == EBTNodeResult.InProgress ? "</b>" : "";

                InDebugger.AddTextLine($"{Prefix}Action: {DebugDisplayName}{Suffix}");
            }

            base.GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        protected override bool OnTick_Children(float InDeltaTime)
        {
            return false;
        }

        protected bool SetStatusAndCalculateReturnValue(EBTNodeResult InResult, bool? bOverrideReturnValue = null)
        {
            LastStatus = InResult;

            if (bOverrideReturnValue.HasValue)
                return bOverrideReturnValue.Value;

            return InResult != EBTNodeResult.Failed;
        }
    }
}
