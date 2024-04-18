using HybridGOAP;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Look at Points of Interest")]
    public class GOAPGoal_LookAtPOI : GOAPGoalBase
    {
        public override void PrepareForPlanning()
        {
            Priority = GoalPriority.Ambient;
        }
    }
}
