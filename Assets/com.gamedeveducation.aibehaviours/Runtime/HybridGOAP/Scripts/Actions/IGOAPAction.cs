using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public interface IGOAPAction : IDebuggable
    {
        ECharacterResources ResourcesRequired { get; }

        void BindToBrain(IGOAPBrain InBrain);

        bool CanSatisfy(IGOAPGoal InGoal);

        float CalculateCost();

        void StartAction();
        void ContinueAction();
        void StopAction();
        void TickAction(float InDeltaTime);
    }
}
