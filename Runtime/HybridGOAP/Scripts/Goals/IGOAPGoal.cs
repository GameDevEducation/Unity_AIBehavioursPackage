using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public interface IGOAPGoal : IDebuggable
    {
        int Priority { get; }
        bool IsValid => (Priority >= GoalPriority.Minimum) && 
                        (Priority <= GoalPriority.Maximum);

        void BindToBrain(IGOAPBrain InBrain);

        void PrepareForPlanning();

        void StartGoal();
        void ContinueGoal();
        void StopGoal();
        void TickGoal(float InDeltaTime);
    }
}
