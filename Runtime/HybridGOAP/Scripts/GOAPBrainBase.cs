using CommonCore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    class GOAPPlanElement : System.IEquatable<GOAPPlanElement>
    {
        public IGOAPGoal Goal { get; private set; } = null;
        public IGOAPAction Action { get; private set; } = null;

        public int Priority => Goal != null ? Goal.Priority : GoalPriority.DoNotRun;
        public float Cost => Action != null ? Action.CalculateCost() : float.MaxValue;
        public ECharacterResources ResourcesUsed => Action != null ? Action.ResourcesRequired : ECharacterResources.None;

        public bool IsValid => Goal != null ? Goal.IsValid : false;

        public GOAPPlanElement(IGOAPGoal InGoal, IGOAPAction InAction)
        {
            Goal = InGoal;
            Action = InAction;
        }

        public void StartElement()
        {
            Goal.StartGoal();
            Action.StartAction();
        }

        public void ContinueElement()
        {
            Goal.ContinueGoal();
            Action.ContinueAction();
        }

        public void StopElement()
        {
            Goal.StopGoal();
            Action.StopAction();
        }

        public void TickElement(float InDeltaTime)
        {
            Goal.TickGoal(InDeltaTime);
            Action.TickAction(InDeltaTime);
        }

        public override bool Equals(object InOther)
        {
            return Equals(InOther as GOAPPlanElement);
        }

        public bool Equals(GOAPPlanElement InOther)
        {
            return InOther is not null &&
                   EqualityComparer<IGOAPGoal>.Default.Equals(Goal, InOther.Goal) &&
                   EqualityComparer<IGOAPAction>.Default.Equals(Action, InOther.Action);
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Goal, Action);
        }

        public static bool operator ==(GOAPPlanElement InLHS, GOAPPlanElement InRHS)
        {
            return EqualityComparer<GOAPPlanElement>.Default.Equals(InLHS, InRHS);
        }

        public static bool operator !=(GOAPPlanElement InLHS, GOAPPlanElement InRHS)
        {
            return !(InLHS == InRHS);
        }
    }

    class GOAPPlan : System.IEquatable<GOAPPlan>
    {
        public List<GOAPPlanElement> Elements { get; private set; } = new();

        public int BasePriority => Elements.Count > 0 ? Elements[0].Priority : GoalPriority.DoNotRun;

        public int CumulativePriority { get; private set; } = GoalPriority.DoNotRun;
        public float CumulativeCost { get; private set; } = float.MaxValue;

        public ECharacterResources ResourcesAvailable { get; private set; } = ECharacterResources.All;
        public ECharacterResources ResourcesUsed { get; private set; } = ECharacterResources.None;

        public bool IsValid
        {
            get
            {
                foreach(var Element in Elements) 
                { 
                    if (!Element.IsValid)
                        return false;
                }

                return Elements.Count > 0;
            }
        }

        public void Start()
        {
            foreach (var Element in Elements)
                Element.StartElement();
        }

        public void Continue()
        {
            foreach (var Element in Elements)
                Element.ContinueElement();
        }

        public void Stop(bool bSendStopNotifications)
        {
            if (bSendStopNotifications)
            {
                foreach (var Element in Elements)
                    Element.StopElement();
            }

            Elements.Clear();
            CumulativePriority = GoalPriority.DoNotRun;
            CumulativeCost = float.MaxValue;
            ResourcesAvailable = ECharacterResources.All;
            ResourcesUsed = ECharacterResources.None;
        }

        public void Tick(float InDeltaTime)
        {
            foreach (var Element in Elements)
                Element.TickElement(InDeltaTime);
        }

        public void AddElement(GOAPPlanElement InElement)
        {
            Elements.Add(InElement);

            CumulativePriority   = InElement.Priority + (Elements.Count == 1 ? 0 : CumulativePriority);
            CumulativeCost       = InElement.Cost + (Elements.Count == 1 ? 0 : CumulativeCost);
            ResourcesUsed       |= InElement.ResourcesUsed;
            ResourcesAvailable  &= ~InElement.ResourcesUsed;
        }

        public void CopyFrom(GOAPPlan InPlan)
        {
            Stop(false);

            foreach (var Element in InPlan.Elements)
                AddElement(new GOAPPlanElement(Element.Goal, Element.Action));
        }

        public bool ContainsGoal(IGOAPGoal InGoal)
        {
            foreach (var Element in Elements)
            {
                if (Element.Goal == InGoal)
                    return true;
            }

            return false;
        }

        public static void Migrate(GOAPPlan InCurrentPlan, GOAPPlan InNewPlan)
        {
            // are there any elements that are fully stopping or continuing
            foreach(var Element in InCurrentPlan.Elements)
            {
                // element is not continuing?
                if (!InNewPlan.Elements.Contains(Element))
                    Element.StopElement();
                else
                    Element.ContinueElement();
            }

            // are there any elements that are starting?
            foreach(var Element in InNewPlan.Elements)
            {
                // element is starting?
                if (!InCurrentPlan.Elements.Contains(Element))
                    Element.StartElement();
            }
        }

        public override bool Equals(object InOther)
        {
            return Equals(InOther as GOAPPlan);
        }

        public bool Equals(GOAPPlan InOther)
        {
            if (InOther != null)
            {
                if (Elements.Count == InOther.Elements.Count) 
                {
                    for (int Index = 0; Index < Elements.Count; ++Index)
                    {
                        if (Elements[Index] != InOther.Elements[Index])
                            return false;
                    }
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return System.HashCode.Combine(Elements);
        }

        public static bool operator ==(GOAPPlan InLHS, GOAPPlan InRHS)
        {
            return EqualityComparer<GOAPPlan>.Default.Equals(InLHS, InRHS);
        }

        public static bool operator !=(GOAPPlan InLHS, GOAPPlan InRHS)
        {
            return !(InLHS == InRHS);
        }
    }

    public abstract class GOAPBrainBase : MonoBehaviour, IGOAPBrain
    {
        List<IGOAPAction> AvailableActions = new();
        List<IGOAPGoal> AvailableGoals = new();

        public Blackboard<FastName> CurrentBlackboard { get; private set; }

        public string DebugDisplayName => gameObject.name;

        GOAPPlan ActivePlan = new();
        GOAPPlan CandidatePlan = new();

        void Start()
        {
            CurrentBlackboard = BlackboardManager.GetIndividualBlackboard(this);

            CurrentBlackboard.Set(CommonCore.Names.Self, gameObject);

            CurrentBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);
            CurrentBlackboard.Set(CommonCore.Names.MoveToLocation, CommonCore.Constants.InvalidVector3Position);

            CurrentBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Target_Position, CommonCore.Constants.InvalidVector3Position);

            CurrentBlackboard.Set(CommonCore.Names.Awareness_BestTarget, (GameObject)null);

            CurrentBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

            CurrentBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            ConfigureBlackboard();

            // populate all of the actions
            AvailableActions.AddRange(GetComponents<IGOAPAction>());
            foreach(var Action in AvailableActions)
                Action.BindToBrain(this);

            // populate all of the goals
            AvailableGoals.AddRange(GetComponents<IGOAPGoal>());
            foreach(var Goal in AvailableGoals)
                Goal.BindToBrain(this);

            ConfigureBrain();

            GameDebugger.AddSource(this);
        }

        void OnDestroy()
        {
            GameDebugger.RemoveSource(this);
        }

        void Update()
        {
            CurrentBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);

            TickBrain(Time.deltaTime);
        }

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            CurrentBlackboard.GatherDebugData(InDebugger, true);

            foreach(var Element in ActivePlan.Elements)
            {
                Element.Goal.GatherDebugData(InDebugger, true);

                InDebugger.PushIndent();
                Element.Action.GatherDebugData(InDebugger, true);
                InDebugger.PopIndent();
            }

            foreach(var Goal in AvailableGoals)
            {
                if (ActivePlan.ContainsGoal(Goal))
                    continue;

                Goal.GatherDebugData(InDebugger, false);
            }
        }

        protected void TickBrain(float InDeltaTime)
        {
            OnPreTickBrain(InDeltaTime);

            // prepare for planning
            foreach(var Goal in AvailableGoals)
                Goal.PrepareForPlanning();

            // sort the goals in priority
            AvailableGoals.Sort((IGOAPGoal InLHS, IGOAPGoal InRHS) =>
            {
                return InRHS.Priority.CompareTo(InLHS.Priority);
            });

            // do we have at least one goal that can run
            if ((AvailableGoals.Count > 0) && AvailableGoals[0].IsValid)
            {
                BuildCandidatePlan();

                EvaluateCandidatePlan();

                if (ActivePlan.IsValid)
                    ActivePlan.Tick(InDeltaTime);
            }
            else
                ActivePlan.Stop(true);

            OnPostTickBrain(InDeltaTime);
        }

        void BuildCandidatePlan()
        {
            // wipe the candidate plan
            CandidatePlan.Stop(false);

            // build the plan
            foreach (var CandidateGoal in AvailableGoals)
            {
                if (!CandidateGoal.IsValid)
                    continue;

                // try to find the best action possible
                IGOAPAction BestCandidateAction = null;
                float BestCandidateCost = float.MaxValue;
                foreach (var CandidateAction in AvailableActions)
                {
                    // action can't meet goal?
                    if (!CandidateAction.CanSatisfy(CandidateGoal))
                        continue;

                    // are the resources not available?
                    var ResourcesNeeded = CandidateAction.ResourcesRequired;
                    if ((ResourcesNeeded != ECharacterResources.None) &&
                        !CandidatePlan.ResourcesAvailable.HasFlag(ResourcesNeeded))
                    {
                        continue;
                    }

                    // either no current candidate or this is cheaper?
                    float CandidateActionCost = CandidateAction.CalculateCost();
                    if ((BestCandidateAction == null) || (CandidateActionCost < BestCandidateCost))
                    {
                        BestCandidateAction = CandidateAction;
                        BestCandidateCost = CandidateActionCost;
                    } // rare case of matching costs?
                    else if (BestCandidateCost == CandidateActionCost)
                    {
                        // use the one with the lower number of resources
                        if (ResourcesHelper.GetNumResourcesUsed(CandidateAction.ResourcesRequired) <
                            ResourcesHelper.GetNumResourcesUsed(BestCandidateAction.ResourcesRequired))
                        {
                            BestCandidateAction = CandidateAction;
                            BestCandidateCost = CandidateActionCost;
                        }
                    }
                }

                // no valid candidate?
                if (BestCandidateAction == null)
                    continue;

                CandidatePlan.AddElement(new GOAPPlanElement(CandidateGoal, BestCandidateAction));
            }
        }

        void EvaluateCandidatePlan()
        {
            // Scenario 1: No current plan, valid candidate
            if (!ActivePlan.IsValid && CandidatePlan.IsValid)
            {
                // start the plan
                ActivePlan.CopyFrom(CandidatePlan);
                ActivePlan.Start();
            }
            else if (ActivePlan.IsValid && CandidatePlan.IsValid) 
            { 
                // Scenario 2: Identical plans
                if (ActivePlan == CandidatePlan)
                {
                    ActivePlan.Continue();
                } // Scenario 3: Differents plans, determine the best
                else
                {
                    // New plan is more important?
                    if (CandidatePlan.BasePriority > ActivePlan.BasePriority) 
                    {
                        GOAPPlan.Migrate(ActivePlan, CandidatePlan);
                        ActivePlan.CopyFrom(CandidatePlan);
                    } // same base priority but higher cumulative priority
                    else if ((CandidatePlan.BasePriority == ActivePlan.BasePriority) &&
                             (CandidatePlan.CumulativePriority > ActivePlan.CumulativePriority)) 
                    {
                        GOAPPlan.Migrate(ActivePlan, CandidatePlan);
                        ActivePlan.CopyFrom(CandidatePlan);
                    }
                }
            } // Scenario 4: No valid candidate plan
            else
            {
                if (ActivePlan.IsValid)
                    ActivePlan.Stop(true);
            }

            CandidatePlan.Stop(false);
        }

        protected abstract void ConfigureBlackboard();
        protected abstract void ConfigureBrain();

        protected virtual void OnPreTickBrain(float InDeltaTime) { }
        protected virtual void OnPostTickBrain(float InDeltaTime) { }
    }
}
