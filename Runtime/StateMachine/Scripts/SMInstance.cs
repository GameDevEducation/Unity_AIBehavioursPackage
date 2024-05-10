using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public enum ESMTickResult
    {
        Running,
        Failed
    }

    public class SMInstance : ISMInstance
    {
        List<ISMState> States = new();
        ISMState CurrentState = null;

        public Blackboard<FastName> LinkedBlackboard { get; protected set; } = null;
        public GameObject Self => LinkedBlackboard.GetGameObject(CommonCore.Names.Self);

        public string DebugDisplayName => "StateMachine";

        public void BindToBlackboard(Blackboard<FastName> InBlackboard)
        {
            LinkedBlackboard = InBlackboard;
        }

        public ISMState AddState(ISMState InState)
        {
            InState.BindToOwner(this);
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
                CurrentState.OnExit();

            CurrentState = null;

            // force a rebind to owner in case any changes to the state machine
            foreach (var State in States)
                State.BindToOwner(this);
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

                CurrentState.OnEnter();
            }

            CurrentState.OnTick(InDeltaTime);

            // check transitions
            ISMState NextState = null;
            CurrentState.EvaluateTransitions(out NextState);

            // transition required?
            if (NextState != null)
            {
                CurrentState.OnExit();

                CurrentState = NextState;

                CurrentState.OnEnter();
            }

            return ESMTickResult.Running;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            InDebugger.AddSectionHeader($"{DebugDisplayName}");

            foreach (var State in States)
            {
                InDebugger.PushIndent();

                State.GatherDebugData(InDebugger, bInIsSelected && (State == CurrentState));

                InDebugger.PopIndent();
            }
        }
    }
}
