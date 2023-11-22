using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public abstract class GOAPActionBase : MonoBehaviour, IGOAPAction
    {
        protected IGOAPBrain LinkedBrain;
        protected INavigationInterface Navigation;
        protected Blackboard<FastName> LinkedBlackboard => LinkedBrain.CurrentBlackboard;

        protected System.Type[] SupportedGoalTypes = { };

        public ECharacterResources ResourcesRequired { get => GetRequiredResources(); }

        public string DebugDisplayName { get => GetType().Name; }

        protected GameObject Self => LinkedBlackboard.GetGameObject(CommonCore.Names.Self);

        public void BindToBrain(IGOAPBrain InBrain)
        {
            LinkedBrain = InBrain;
            Navigation = GetComponent<INavigationInterface>();

            PopulateSupportedGoalTypes();
            OnInitialise();
        }

        public bool CanSatisfy(IGOAPGoal InGoal)
        {
            foreach (var GoalType in SupportedGoalTypes)
            {
                if (InGoal.GetType() == GoalType)
                    return true;
            }

            return false;
        }

        public void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            InDebugger.AddSectionHeader($"{DebugDisplayName} [{ResourcesRequired}]");

            GatherDebugDataInternal(InDebugger, bInIsSelected);
        }

        public void StartAction()
        {
            OnStartAction();
        }

        public void ContinueAction()
        {
            OnContinueAction();
        }

        public void StopAction()
        {
            OnStopAction();
        }

        public void TickAction(float InDeltaTime)
        {
            OnTickAction(InDeltaTime);
        }

        protected abstract void PopulateSupportedGoalTypes();
        protected abstract void OnInitialise();

        protected abstract ECharacterResources GetRequiredResources();
        public abstract float CalculateCost();

        protected abstract void OnStartAction();
        protected abstract void OnContinueAction();
        protected abstract void OnStopAction();
        protected abstract void OnTickAction(float InDeltaTime);

        protected virtual void GatherDebugDataInternal(IGameDebugger InDebugger, bool bInIsSelected) { }
    }
}
