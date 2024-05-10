using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace StateMachine
{
    public abstract class SMStateBase : ISMState
    {
        protected class TransitionEntry
        {
            public ISMTransition Transition;
            public ISMState State;
        }

        List<TransitionEntry> Transitions = new();

        public string DebugDisplayName { get; protected set; }

        public ESMStateStatus CurrentStatus { get; protected set; } = ESMStateStatus.Uninitialised;
        public ISMInstance Owner { get; protected set; }
        public GameObject Self => Owner.Self;
        public Blackboard<FastName> LinkedBlackboard => Owner.LinkedBlackboard;

        public SMStateBase(string InDisplayName = null)
        {
            DebugDisplayName = string.IsNullOrEmpty(InDisplayName) ? GetType().Name : InDisplayName;
        }

        public void AddDefaultTransitions(ISMState InFinishedState, ISMState InFailedState)
        {
            bool bFinishedHandled = false;
            bool bFailedHandled = false;

            foreach (var Entry in Transitions)
            {
                if (Entry.Transition == null)
                    continue;

                bFinishedHandled |= Entry.Transition.HandlesStateStatus(ESMStateStatus.Finished);
                bFailedHandled |= Entry.Transition.HandlesStateStatus(ESMStateStatus.Failed);

                if (bFinishedHandled && bFailedHandled)
                    return;
            }

            if (!bFinishedHandled)
                AddTransition(SMTransition_StateStatus.IfHasFinished(), InFinishedState);
            if (!bFailedHandled)
                AddTransition(SMTransition_StateStatus.IfHasFailed(), InFailedState);
        }

        public ISMState AddTransition(ISMTransition InTransition, ISMState InNewState)
        {
            Transitions.Add(new TransitionEntry() { Transition = InTransition, State = InNewState });

            return this;
        }

        public void EvaluateTransitions(out ISMState OutNextState)
        {
            EvaluateTransitionsInternal(out OutNextState);
        }

        protected virtual void EvaluateTransitionsInternal(out ISMState OutNextState)
        {
            OutNextState = null;

            foreach (var Entry in Transitions)
            {
                if (Entry.Transition.Evaluate(this, LinkedBlackboard) == ESMTransitionResult.Valid)
                {
                    OutNextState = Entry.State;
                    return;
                }
            }
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (bInIsSelected)
                InDebugger.AddTextLine($"* {DebugDisplayName}");
            else
                InDebugger.AddTextLine($"{DebugDisplayName}");

            GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {

        }

        public ESMStateStatus OnEnter()
        {
            CurrentStatus = OnEnterInternal();
            return CurrentStatus;
        }

        protected abstract ESMStateStatus OnEnterInternal();

        public void OnExit()
        {
            OnExitInternal();
        }
        protected abstract void OnExitInternal();

        public ESMStateStatus OnTick(float InDeltaTime)
        {
            if (CurrentStatus == ESMStateStatus.Running)
            {
                CurrentStatus = OnTickInternal(InDeltaTime);
            }

            return CurrentStatus;
        }

        protected abstract ESMStateStatus OnTickInternal(float InDeltaTime);

        public virtual void BindToOwner(ISMInstance InOwner)
        {
            Owner = InOwner;
        }
    }
}
