using CommonCore;
using System.Collections.Generic;

namespace StateMachine
{
    public class SMState_Parallel : SMStateBase
    {
        List<ISMState> ChildStates = null;

        bool bIgnoreFailures;
        bool bExitIfAllFinished;

        public SMState_Parallel(List<ISMState> InChildStates, bool bInIgnoreFailures = false, bool bInExitIfAllAreFinished = true, string InDisplayName = null) :
            base(InDisplayName)
        {
            ChildStates = InChildStates;
            bIgnoreFailures = bInIgnoreFailures;
            bExitIfAllFinished = bInExitIfAllAreFinished;
        }

        public override void BindToOwner(ISMInstance InOwner)
        {
            base.BindToOwner(InOwner);

            if (ChildStates != null)
            {
                foreach (var Child in ChildStates)
                    Child.BindToOwner(InOwner);
            }
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            if (ChildStates == null || ChildStates.Count == 0)
                return ESMStateStatus.Failed;

            bool bAllFinished = true;
            foreach (var Child in ChildStates)
            {
                var Result = Child.OnEnter();

                if (!bIgnoreFailures && (Result == ESMStateStatus.Failed))
                    return ESMStateStatus.Failed;

                if (Result == ESMStateStatus.Running)
                    bAllFinished = false;
            }

            return (bExitIfAllFinished && bAllFinished) ? ESMStateStatus.Finished : ESMStateStatus.Running;
        }

        protected override void OnExitInternal()
        {
            if (ChildStates == null || ChildStates.Count == 0)
                return;

            foreach (var Child in ChildStates)
                Child.OnExit();
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            if (ChildStates == null || ChildStates.Count == 0)
                return ESMStateStatus.Failed;

            bool bAllFinished = true;
            foreach (var Child in ChildStates)
            {
                var Result = Child.OnTick(InDeltaTime);

                if (!bIgnoreFailures && (Result == ESMStateStatus.Failed))
                    return ESMStateStatus.Failed;

                if (Result != ESMStateStatus.Finished)
                    bAllFinished = false;
            }

            return (bExitIfAllFinished && bAllFinished) ? ESMStateStatus.Finished : ESMStateStatus.Running;
        }

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            base.GatherDebugDataInternal(InDebugger, bInIsSelected);

            InDebugger.PushIndent();

            foreach (var Child in ChildStates)
            {
                Child.GatherDebugData(InDebugger, bInIsSelected);
            }

            InDebugger.PopIndent();
        }
    }
}
