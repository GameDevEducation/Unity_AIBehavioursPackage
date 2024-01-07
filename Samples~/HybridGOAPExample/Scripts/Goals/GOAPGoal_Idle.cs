using HybridGOAP;

namespace HybridGOAPExample
{
    public class GOAPGoal_Idle : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Minimum + 1;
        }
    }
}
