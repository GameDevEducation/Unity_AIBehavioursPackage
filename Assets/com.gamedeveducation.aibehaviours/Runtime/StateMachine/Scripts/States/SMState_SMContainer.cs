using CommonCore;

namespace StateMachine
{
    public class SMState_SMContainer : SMStateBase
    {
        class InternalState_StateMachineFinished : SMStateBase
        {
            System.Action CallbackFn;

            internal InternalState_StateMachineFinished(System.Action InCallbackFn) :
                base(null)
            {
                CallbackFn = InCallbackFn;
            }

            protected override ESMStateStatus OnEnterInternal()
            {
                CallbackFn();

                return ESMStateStatus.Finished;
            }

            protected override void OnExitInternal()
            {
            }

            protected override ESMStateStatus OnTickInternal(float InDeltaTime)
            {
                return ESMStateStatus.Finished;
            }
        }

        protected ISMInstance LinkedStateMachine = new SMInstance();

        protected ISMState InternalState_Failed { get; private set; }
        protected ISMState InternalState_Finished { get; private set; }

        ESMStateStatus? PendingTickResult;

        public SMState_SMContainer(string InDisplayName = null) :
            base(InDisplayName)
        {
            InternalState_Failed = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Failed);
            InternalState_Finished = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Finished);
        }

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            base.GatherDebugDataInternal(InDebugger, bInIsSelected);

            LinkedStateMachine.GatherDebugData(InDebugger, bInIsSelected);
        }

        protected void ResetStateMachine()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();
        }

        void InternalOnStateMachineCompleted_Failed()
        {
            ResetStateMachine();

            PendingTickResult = ESMStateStatus.Failed;

            OnStateMachineCompleted_Failed();
        }

        void InternalOnStateMachineCompleted_Finished()
        {
            ResetStateMachine();

            PendingTickResult = ESMStateStatus.Finished;

            OnStateMachineCompleted_Finished();
        }

        protected virtual void OnStateMachineCompleted_Failed() { }
        protected virtual void OnStateMachineCompleted_Finished() { }
        protected virtual void OnStateMachineReset() { }

        public override void BindToOwner(ISMInstance InOwner)
        {
            base.BindToOwner(InOwner);

            LinkedStateMachine.BindToBlackboard(InOwner.LinkedBlackboard);
        }

        public ISMState AddState(ISMState InState)
        {
            return LinkedStateMachine.AddState(InState);
        }

        public void FinishConstruction()
        {
            LinkedStateMachine.AddDefaultTransitions(InternalState_Finished, InternalState_Failed);
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            LinkedStateMachine.Reset();
            PendingTickResult = null;

            return ESMStateStatus.Running;
        }

        protected override void OnExitInternal()
        {
            LinkedStateMachine.Reset();
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            if (PendingTickResult != null)
                return PendingTickResult.Value;

            var Result = LinkedStateMachine.Tick(InDeltaTime);

            return Result == ESMTickResult.Failed ? ESMStateStatus.Failed : ESMStateStatus.Running;
        }
    }
}
