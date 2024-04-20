using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Idle Goal")]
    public class GOAPGoal_Idle : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Minimum + 1;
        }
    }
}
