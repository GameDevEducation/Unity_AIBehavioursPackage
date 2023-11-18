using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public abstract class GOAPBrainBase : MonoBehaviour, IGOAPBrain
    {
        public Blackboard<FastName> CurrentBlackboard { get; private set; }

        public string DebugDisplayName => gameObject.name;

        void Start()
        {
            CurrentBlackboard = BlackboardManager.GetIndividualBlackboard(this);

            CurrentBlackboard.Set(CommonCore.Names.Self, gameObject);

            CurrentBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);
            CurrentBlackboard.Set(CommonCore.Names.MoveToLocation, CommonCore.Constants.InvalidVector3Position);

            GameDebugger.AddSource(this);
        }

        void OnDestroy()
        {
            GameDebugger.RemoveSource(this);
        }

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            CurrentBlackboard.GatherDebugData(InDebugger, true);
        }
    }
}
