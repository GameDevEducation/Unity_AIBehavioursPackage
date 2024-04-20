using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Wander Goal")]
    public class GOAPGoal_Wander : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Ambient;
        }
    }
}
