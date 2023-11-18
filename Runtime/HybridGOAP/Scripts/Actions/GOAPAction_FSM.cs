using CommonCore;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public abstract class GOAPAction_FSM : GOAPActionBase
    {
        class InternalState_StateMachineFinished : SMStateBase
        {
            System.Action CallbackFn;

            internal InternalState_StateMachineFinished(System.Action InCallbackFn) :
                base(null)
            {
                CallbackFn = InCallbackFn;
            }

            protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
            {
                CallbackFn();

                return ESMStateStatus.Finished;
            }

            protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
            {
            }

            protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
            {
                return ESMStateStatus.Finished;
            }
        }

        protected SMInstance LinkedStateMachine = new();

        protected ISMState InternalState_Failed { get; private set; }
        protected ISMState InternalState_Finished { get; private set; }

        protected override void OnInitialise()
        {
            InternalState_Failed = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Failed);
            InternalState_Finished = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Finished);

            LinkedStateMachine.BindToBlackboard(LinkedBlackboard);

            ConfigureFSM();
        }

        void InternalOnStateMachineCompleted_Failed()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();

            OnStateMachineCompleted_Failed();
        }

        void InternalOnStateMachineCompleted_Finished()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();

            OnStateMachineCompleted_Finished();
        }

        protected override void OnStartAction()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();
        }

        protected override void OnContinueAction()
        {
        }

        protected override void OnStopAction()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();
        }

        protected override void OnTickAction(float InDeltaTime)
        {
            LinkedStateMachine.Tick(InDeltaTime);
        }

        protected override void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            LinkedStateMachine.GatherDebugData(InDebugger, bInIsSelected);
        }

        protected abstract void ConfigureFSM();

        protected virtual void OnStateMachineCompleted_Failed() { }
        protected virtual void OnStateMachineCompleted_Finished() { }
        protected virtual void OnStateMachineReset() { }
    }
}
