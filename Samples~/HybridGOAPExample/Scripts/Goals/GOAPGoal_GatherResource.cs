using HybridGOAP;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPGoal_GatherResource : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            if (SimpleResourceWrangler.Instance == null)
            {
                Priority = GoalPriority.DoNotRun;
                return;
            }

            float DesireToGather = float.MinValue;
            SimpleResourceWrangler.Instance.GetGatherResourceDesire(Self, (float InDesire) =>
            {
                DesireToGather = InDesire;
            });

            if (DesireToGather <= 0)
                Priority = GoalPriority.DoNotRun;
            else
                Priority = Mathf.FloorToInt(Mathf.Lerp(GoalPriority.Medium, GoalPriority.High, DesireToGather));
        }
    }
}
