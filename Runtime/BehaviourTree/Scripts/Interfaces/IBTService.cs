using CommonCore;
using UnityEngine;

namespace BehaviourTree
{
    public interface IBTService : IDebuggable
    {
        GameObject Self { get; }
        IBehaviourTree OwningTree { get; }
        Blackboard<FastName> LinkedBlackboard { get; }

        void SetOwningTree(IBehaviourTree InOwningTree);

        bool Tick(float InDeltaTime);
    }
}
