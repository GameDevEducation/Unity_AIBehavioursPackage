using HybridGOAP;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
