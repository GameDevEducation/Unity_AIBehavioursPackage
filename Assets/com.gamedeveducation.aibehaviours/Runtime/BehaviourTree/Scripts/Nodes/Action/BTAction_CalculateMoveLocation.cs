using UnityEngine;

namespace BehaviourTree
{
    public class BTAction_CalculateMoveLocation : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Calculate Move Location";

        float SearchRange;
        System.Func<Vector3> GetSearchLocationFn;
        System.Func<Vector3> GetEndOrientationFn;
        bool bContinuousMode;
        float RecalculateThresholdSq;

        Vector3 LastSearchLocation;

        public BTAction_CalculateMoveLocation(float InSearchRange, System.Func<Vector3> InGetSearchLocationFn,
                                              System.Func<Vector3> InGetEndOrientationFn,
                                              bool bInContinuousMode = false, float InRecalculateThreshold = 0.1f, string InDisplayName = null)
        {
            SearchRange = InSearchRange;
            GetSearchLocationFn = InGetSearchLocationFn;
            GetEndOrientationFn = InGetEndOrientationFn;
            bContinuousMode = bInContinuousMode;
            RecalculateThresholdSq = InRecalculateThreshold * InRecalculateThreshold;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            if (GetSearchLocationFn == null)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            Vector3 MoveLocation = GetSearchLocationFn();
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            // find the nearest navigable point
            MoveLocation = OwningTree.NavigationInterface.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
            LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
            LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, CommonCore.Constants.InvalidVector3Position);
            if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            if (GetEndOrientationFn != null)
                LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, GetEndOrientationFn());

            LastSearchLocation = MoveLocation;

            LastStatus = bContinuousMode ? EBTNodeResult.InProgress : EBTNodeResult.Succeeded;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (bContinuousMode)
            {
                Vector3 MoveLocation = GetSearchLocationFn();
                if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
                    return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

                // moved far enough to recalculate?
                if ((LastSearchLocation - MoveLocation).sqrMagnitude >= RecalculateThresholdSq)
                {
                    // find the nearest navigable point
                    MoveLocation = OwningTree.NavigationInterface.FindNearestNavigableLocation(Self, MoveLocation, SearchRange);
                    LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, MoveLocation);
                    if (MoveLocation == CommonCore.Constants.InvalidVector3Position)
                        return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

                    if (GetEndOrientationFn != null)
                        LinkedBlackboard.Set(CommonCore.Names.MoveToEndOrientation, GetEndOrientationFn());

                    LastSearchLocation = MoveLocation;
                }

                return SetStatusAndCalculateReturnValue(EBTNodeResult.InProgress);
            }

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
        }
    }
}
