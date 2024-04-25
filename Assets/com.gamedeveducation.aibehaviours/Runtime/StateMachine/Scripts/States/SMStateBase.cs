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

        public void EvaluateTransitions(Blackboard<FastName> InBlackboard, out ISMState OutNextState)
        {
            EvaluateTransitionsInternal(InBlackboard, out OutNextState);
        }

        protected virtual void EvaluateTransitionsInternal(Blackboard<FastName> InBlackboard, out ISMState OutNextState)
        {
            OutNextState = null;

            foreach (var Entry in Transitions)
            {
                if (Entry.Transition.Evaluate(this, InBlackboard) == ESMTransitionResult.Valid)
                {
                    OutNextState = Entry.State;
                    return;
                }
            }
        }

        protected GameObject GetOwner(Blackboard<FastName> InBlackboard)
        {
            return InBlackboard.GetGameObject(CommonCore.Names.Self);
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

        public ESMStateStatus OnEnter(Blackboard<FastName> InBlackboard)
        {
            CurrentStatus = OnEnterInternal(InBlackboard);
            return CurrentStatus;
        }

        protected abstract ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard);

        public void OnExit(Blackboard<FastName> InBlackboard)
        {
            OnExitInternal(InBlackboard);
        }
        protected abstract void OnExitInternal(Blackboard<FastName> InBlackboard);

        public ESMStateStatus OnTick(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            if (CurrentStatus == ESMStateStatus.Running)
            {
                CurrentStatus = OnTickInternal(InBlackboard, InDeltaTime);
            }

            return CurrentStatus;
        }

        protected abstract ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime);
    }
}
