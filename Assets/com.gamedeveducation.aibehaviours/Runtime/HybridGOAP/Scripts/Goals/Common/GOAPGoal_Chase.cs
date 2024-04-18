using HybridGOAP;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Chase Goal")]
    public class GOAPGoal_Chase : GOAPGoalBase
    {
        [SerializeField] int DesireToChaseStartDistance = 1;
        [SerializeField] int DesireToChaseEndDistance = 50;

        public override void PrepareForPlanning()
        {
            Vector3 TargetLocation = GetTargetLocation();

            if (TargetLocation == CommonCore.Constants.InvalidVector3Position)
                Priority = GoalPriority.DoNotRun;
            else
            {
                Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);
                float DistanceToTargetSq = (CurrentLocation - TargetLocation).sqrMagnitude;

                if (DistanceToTargetSq > (DesireToChaseEndDistance * DesireToChaseEndDistance))
                    Priority = GoalPriority.DoNotRun;
                else if (DistanceToTargetSq < (DesireToChaseStartDistance * DesireToChaseStartDistance))
                    Priority = GoalPriority.High;
                else
                {
                    float InterpolationFactor = Mathf.InverseLerp(DesireToChaseStartDistance,
                                                                  DesireToChaseEndDistance,
                                                                  Mathf.Sqrt(DistanceToTargetSq));
                    Priority = Mathf.FloorToInt(Mathf.Lerp(GoalPriority.High, GoalPriority.Low, InterpolationFactor));
                }
            }
        }

        Vector3 GetTargetLocation()
        {
            Vector3 TargetLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to get the target object
            GameObject TargetGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out TargetGO, null);

            if (TargetGO != null)
                TargetLocation = TargetGO.transform.position;
            else
                LinkedBlackboard.TryGet(CommonCore.Names.Target_Position, out TargetLocation, CommonCore.Constants.InvalidVector3Position);

            return TargetLocation;
        }
    }
}
