using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public enum ESMTickResult
    {
        Running,
        Failed
    }

    public class SMInstance : IDebuggable
    {
        List<ISMState> States = new();
        ISMState CurrentState = null;
        Blackboard<FastName> LinkedBlackboard = null;

        public string DebugDisplayName => "StateMachine";

        public void BindToBlackboard(Blackboard<FastName> InBlackboard)
        {
            LinkedBlackboard = InBlackboard;
        }

        public ISMState AddState(ISMState InState)
        {
            States.Add(InState);
            return InState;
        }

        public void AddDefaultTransitions(ISMState InFinishedState, ISMState InFailedState)
        {
            foreach (var State in States) 
            {
                if ((State == InFinishedState) || (State == InFailedState))
                    continue;

                State.AddDefaultTransitions(InFinishedState, InFailedState);
            }
        }

        public void Reset()
        {
            if ((CurrentState != null) && (CurrentState.CurrentStatus == ESMStateStatus.Running))
                CurrentState.OnExit(LinkedBlackboard);

            CurrentState = null;
        }

        public ESMTickResult Tick(float InDeltaTime)
        {
            // no current state?
            if (CurrentState == null)
            {
                // no states present?
                if (States.Count == 0)
                    return ESMTickResult.Failed;

                // set the initial state
                CurrentState = States[0];

                // null initial state
                if (CurrentState == null)
                    return ESMTickResult.Failed;

                CurrentState.OnEnter(LinkedBlackboard);
            }

            CurrentState.OnTick(LinkedBlackboard, InDeltaTime);

            // check transitions
            ISMState NextState = null;
            CurrentState.EvaluateTransitions(LinkedBlackboard, out NextState);

            // transition required?
            if (NextState != null)
            {
                CurrentState.OnExit(LinkedBlackboard);

                CurrentState = NextState;

                CurrentState.OnEnter(LinkedBlackboard);
            }

            return ESMTickResult.Running;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            InDebugger.AddSectionHeader($"{DebugDisplayName}");

            foreach(var State in States)
            {
                InDebugger.PushIndent();

                State.GatherDebugData(InDebugger, bInIsSelected && (State == CurrentState));

                InDebugger.PopIndent();
            }
        }
    }
}
