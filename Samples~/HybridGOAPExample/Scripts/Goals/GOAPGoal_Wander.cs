using HybridGOAP;

namespace HybridGOAPExample
{
    public class GOAPGoal_Wander : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Ambient;
        }
    }
}
