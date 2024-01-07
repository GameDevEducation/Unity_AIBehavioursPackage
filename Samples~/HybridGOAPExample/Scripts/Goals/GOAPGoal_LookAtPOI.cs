using HybridGOAP;

namespace HybridGOAPExample
{
    public class GOAPGoal_LookAtPOI : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Ambient;
        }
    }
}
