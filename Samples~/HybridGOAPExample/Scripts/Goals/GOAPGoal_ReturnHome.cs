using HybridGOAP;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPGoal_ReturnHome : GOAPGoalBase
    {
        [SerializeField] float MissingHomeStartDistance = 50;
        [SerializeField] float MissingHomePeaksDistance = 100;
        [SerializeField] float MissingHomeResetDistance = 10;

        public override void PrepareForPlanning()
        {
            Vector3 HomeLocation = GetHomeLocation();

            if (HomeLocation == CommonCore.Constants.InvalidVector3Position)
                Priority = GoalPriority.DoNotRun;
            else
            {
                Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);

                float DistanceToHomeSq = (HomeLocation - CurrentLocation).sqrMagnitude;

                int NewPriority = Priority;
                if (DistanceToHomeSq < (MissingHomeStartDistance * MissingHomeStartDistance))
                    NewPriority = GoalPriority.DoNotRun;
                else if (DistanceToHomeSq >= (MissingHomePeaksDistance * MissingHomePeaksDistance))
                    NewPriority = GoalPriority.High;
                else
                {
                    float InterpolationFactor = Mathf.InverseLerp(MissingHomeStartDistance, MissingHomePeaksDistance, Mathf.Sqrt(DistanceToHomeSq));
                    NewPriority = Mathf.FloorToInt(Mathf.Lerp(GoalPriority.Low, GoalPriority.High, InterpolationFactor));
                }

                if (DistanceToHomeSq >= (MissingHomeResetDistance * MissingHomeResetDistance))
                    Priority = Mathf.Max(Priority, NewPriority);
                else
                    Priority = NewPriority;
            }
        }

        Vector3 GetHomeLocation()
        {
            Vector3 HomeLocation = CommonCore.Constants.InvalidVector3Position;

            LinkedBlackboard.TryGet(CommonCore.Names.HomeLocation, out HomeLocation, CommonCore.Constants.InvalidVector3Position);

            return HomeLocation;
        }
    }
}
