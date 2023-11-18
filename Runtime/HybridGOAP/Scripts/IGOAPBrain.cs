using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public interface IGOAPBrain : IDebuggableObject
    {
        Blackboard<FastName> CurrentBlackboard { get; }
    }
}
