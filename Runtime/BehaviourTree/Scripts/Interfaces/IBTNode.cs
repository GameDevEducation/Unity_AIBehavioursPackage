using CommonCore;
using UnityEngine;

namespace BehaviourTree
{
    public enum EBTNodeResult
    {
        Uninitialised,

        ReadyToTick,
        InProgress,

        Succeeded,
        Failed
    }

    public enum EBTNodeTickPhase
    {
        WaitingForNextTick,

        AlwaysOnServices,
        Decorators,
        GeneralServices,
        NodeLogic,
        Children
    }

    public interface IBTNode : IDebuggable
    {
        GameObject Self { get; }
        IBehaviourTree OwningTree { get; }
        Blackboard<FastName> LinkedBlackboard { get; }

        EBTNodeResult LastStatus { get; }
        bool HasChildren { get; }

        bool HasFinished { get; }

        void SetOwningTree(IBehaviourTree InOwningTree);

        bool DoDecoratorsNowPermitRunning(float InDeltaTime);

        void Reset();

        EBTNodeResult Tick(float InDeltaTime);

        IBTNode AddService(IBTService InService, bool bInIsAlwaysOn = false);
        IBTNode AddDecorator(IBTDecorator InDecorator);
    }
}
