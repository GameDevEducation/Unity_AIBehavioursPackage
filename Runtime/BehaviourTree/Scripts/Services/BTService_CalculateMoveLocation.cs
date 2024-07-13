using UnityEngine;

namespace BehaviourTree
{
    public class BTService_CalculateMoveLocation : BTServiceBase
    {
        public override string DebugDisplayName { get; protected set; } = "Calculate Move Location";

        float SearchRange;
        float RecalculateThresholdSq;
        System.Func<Vector3> GetRequestedDestinationFn;
        System.Func<Vector3> GetEndOrientationFn;

        Vector3 LastRequestedDestination;

        public BTService_CalculateMoveLocation(float InSearchRange, System.Func<Vector3> InGetRequestedDestinationFn,
                                               System.Func<Vector3> InGetEndOrientationFn,
                                               float InRecalculationThreshold = 0.1f, string InDisplayName = null)
        {
            SearchRange = InSearchRange;
            RecalculateThresholdSq = InRecalculationThreshold * InRecalculationThreshold;
            GetRequestedDestinationFn = InGetRequestedDestinationFn;
            GetEndOrientationFn = InGetEndOrientationFn;
            LastRequestedDestination = CommonCore.Constants.InvalidVector3Position;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        public override bool Tick(float InDeltaTime)
        {
            if (GetRequestedDestinationFn == null)
                return false;

            Vector3 RequestedDestination = GetRequestedDestinationFn();
            if (RequestedDestination == CommonCore.Constants.InvalidVector3Position)
                return false;

            // moved far enough to recalculate
            if ((LastRequestedDestination == CommonCore.Constants.InvalidVector3Position) ||
                ((RequestedDestination - LastRequestedDestination).sqrMagnitude >= RecalculateThresholdSq))
            {
                LastRequestedDestination = RequestedDestination;

                RequestedDestination = OwningTree.NavigationInterface.FindNearestNavigableLocation(Self, RequestedDestination, SearchRange);
                LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, RequestedDestination);
                LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, CommonCore.Constants.InvalidVector3Position);

                if (RequestedDestination != CommonCore.Constants.InvalidVector3Position)
                {
                    if (GetEndOrientationFn != null)
                        LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, GetEndOrientationFn());
                }
            }

            return true;
        }
    }
}
