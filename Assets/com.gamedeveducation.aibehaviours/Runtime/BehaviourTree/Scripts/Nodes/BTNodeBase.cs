using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class BTNodeBase : IBTNode
    {
        public GameObject Self => LinkedBlackboard.GetGameObject(CommonCore.Names.Self);

        public IBehaviourTree OwningTree { get; protected set; }

        public Blackboard<FastName> LinkedBlackboard => OwningTree.LinkedBlackboard;

        public EBTNodeResult LastStatus { get; protected set; } = EBTNodeResult.Uninitialised;
        protected EBTNodeTickPhase CurrentTickPhase { get; set; } = EBTNodeTickPhase.WaitingForNextTick;

        protected List<IBTService> AlwaysOnServices;
        protected List<IBTDecorator> Decorators;
        protected List<IBTService> GeneralServices;

        public bool HasFinished => (LastStatus == EBTNodeResult.Succeeded || LastStatus == EBTNodeResult.Failed);
        public abstract bool HasChildren { get; }

        public abstract string DebugDisplayName { get; protected set; }

        protected bool bDecoratorsAllowRunning = false;
        protected bool bCanSendExitNotification = false;

        public IBTNode AddDecorator(IBTDecorator InDecorator)
        {
            if (Decorators == null)
                Decorators = new();

            InDecorator.SetOwningTree(OwningTree);
            Decorators.Add(InDecorator);

            return this;
        }

        public IBTNode AddService(IBTService InService, bool bInIsAlwaysOn = false)
        {
            InService.SetOwningTree(OwningTree);

            if (bInIsAlwaysOn)
            {
                if (AlwaysOnServices == null)
                    AlwaysOnServices = new();

                AlwaysOnServices.Add(InService);
            }
            else
            {
                if (GeneralServices == null)
                    GeneralServices = new();

                GeneralServices.Add(InService);
            }

            return this;
        }

        public bool DoDecoratorsNowPermitRunning(float InDeltaTime)
        {
            // if the decorators already allowed running then no need to check
            if (bDecoratorsAllowRunning)
                return false;

            // update always on services
            if (!OnTick_AlwaysOnServices(InDeltaTime))
                return false;

            // check decorators
            if (!OnTick_Decorators(InDeltaTime))
                return false;

            return true;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (bInIsSelected)
            {
                InDebugger.PushIndent();

                if (AlwaysOnServices != null)
                {
                    foreach (var Service in AlwaysOnServices)
                        Service.GatherDebugData(InDebugger, bInIsSelected);
                }

                if (Decorators != null)
                {
                    foreach (var Decorator in Decorators)
                        Decorator.GatherDebugData(InDebugger, bInIsSelected);
                }

                if (GeneralServices != null)
                {
                    foreach (var Service in GeneralServices)
                        Service.GatherDebugData(InDebugger, bInIsSelected);

                }

                GatherDebugDataInternal(InDebugger, bInIsSelected);

                InDebugger.PopIndent();
            }
        }

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected)
        {

        }

        public virtual void Reset()
        {
            LastStatus = EBTNodeResult.ReadyToTick;
        }

        public void SetOwningTree(IBehaviourTree InOwningTree)
        {
            OwningTree = InOwningTree;
        }

        public EBTNodeResult Tick(float InDeltaTime)
        {
            // first time running, reset the node
            if (LastStatus == EBTNodeResult.Uninitialised)
                Reset();

            CurrentTickPhase = EBTNodeTickPhase.AlwaysOnServices;
            if (!OnTick_AlwaysOnServices(InDeltaTime))
            {
                LastStatus = EBTNodeResult.Failed;
                return OnTickReturn(LastStatus);
            }

            CurrentTickPhase = EBTNodeTickPhase.Decorators;
            if (!OnTick_Decorators(InDeltaTime))
            {
                LastStatus = EBTNodeResult.Failed;

                // node has previously run and now is not permitted to?
                if (bDecoratorsAllowRunning && bCanSendExitNotification)
                    OnExit();

                bDecoratorsAllowRunning = false;

                return OnTickReturn(LastStatus);
            }

            // if the decorators have changed to permit running then reset the node
            if (!bDecoratorsAllowRunning)
            {
                Reset();

                bDecoratorsAllowRunning = true;
            }

            // have we already finished?
            if (HasFinished)
                return OnTickReturn(LastStatus);

            CurrentTickPhase = EBTNodeTickPhase.GeneralServices;
            if (!OnTick_GeneralServices(InDeltaTime))
                return OnTickReturn(EBTNodeResult.Failed);

            // node has never been ticked?
            if (LastStatus == EBTNodeResult.ReadyToTick)
            {
                OnEnter();

                if (HasFinished)
                {
                    OnExit();

                    return OnTickReturn(LastStatus);
                }
            }

            CurrentTickPhase = EBTNodeTickPhase.NodeLogic;
            if (!OnTick_NodeLogic(InDeltaTime))
                return OnTickReturn(EBTNodeResult.Failed);

            if (HasChildren)
            {
                CurrentTickPhase = EBTNodeTickPhase.Children;
                if (!OnTick_Children(InDeltaTime))
                    return OnTickReturn(LastStatus);
            }

            return OnTickReturn(LastStatus);
        }

        protected virtual EBTNodeResult OnTickReturn(EBTNodeResult InProvisionalResult)
        {
            EBTNodeResult FinalResult = InProvisionalResult;
            CurrentTickPhase = EBTNodeTickPhase.WaitingForNextTick;

            if (Decorators != null)
            {
                foreach (var Decorator in Decorators)
                {
                    if (Decorator.CanPostProcessTickResult)
                        FinalResult = Decorator.PostProcessTickResult(FinalResult);
                }
            }

            if (bCanSendExitNotification && HasFinished)
                OnExit();

            return FinalResult;
        }

        protected virtual bool OnTick_AlwaysOnServices(float InDeltaTime)
        {
            if (AlwaysOnServices != null)
            {
                foreach (var Service in AlwaysOnServices)
                {
                    if (!Service.Tick(InDeltaTime))
                        return false;
                }
            }

            return true;
        }

        protected virtual bool OnTick_GeneralServices(float InDeltaTime)
        {
            if (GeneralServices != null)
            {
                foreach (var Service in GeneralServices)
                {
                    if (!Service.Tick(InDeltaTime))
                        return false;
                }
            }

            return true;
        }

        protected virtual bool OnTick_Decorators(float InDeltaTime)
        {
            if (Decorators != null)
            {
                foreach (var Decorator in Decorators)
                {
                    if (!Decorator.Tick(InDeltaTime))
                        return false;
                }
            }

            return true;
        }

        protected abstract bool OnTick_NodeLogic(float InDeltaTime);
        protected abstract bool OnTick_Children(float InDeltaTime);

        protected virtual void OnEnter()
        {
            bCanSendExitNotification = true;
        }

        protected virtual void OnExit()
        {
            bCanSendExitNotification = false;
        }
    }
}
