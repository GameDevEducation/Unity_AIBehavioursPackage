using UnityEngine;

namespace BehaviourTree
{
    public class BTAction_Move : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Move To";

        float StoppingDistance;
        bool bContinuousMode;
        float RecalculateThresholdSq;
        Vector3 LastDestination;

        public BTAction_Move(float InStoppingDistance, bool bInContinuousMode = false, float InRecalculateThreshold = 0.1f, string InDisplayName = null)
        {
            StoppingDistance = InStoppingDistance;
            bContinuousMode = bInContinuousMode;
            RecalculateThresholdSq = InRecalculateThreshold * InRecalculateThreshold;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            Vector3 Destination = CommonCore.Constants.InvalidVector3Position;
            if (LinkedBlackboard.TryGet(CommonCore.Names.MoveToLocation, out Destination, CommonCore.Constants.InvalidVector3Position))
            {
                if (Destination == CommonCore.Constants.InvalidVector3Position)
                {
                    LastStatus = EBTNodeResult.Failed;
                    return;
                }

                Vector3 DestinationOrientation = CommonCore.Constants.InvalidVector3Position;
                LinkedBlackboard.TryGet(CommonCore.Names.MoveToEndOrientation, out DestinationOrientation, CommonCore.Constants.InvalidVector3Position);

                Vector3? EndOrientation = (DestinationOrientation != CommonCore.Constants.InvalidVector3Position) ? DestinationOrientation : null;

                if (OwningTree.NavigationInterface.SetMoveLocation(Self, Destination, EndOrientation, StoppingDistance))
                {
                    LastDestination = Destination;
                    LastStatus = EBTNodeResult.InProgress;
                    return;
                }
            }

            LastStatus = EBTNodeResult.Failed;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (bContinuousMode)
            {
                Vector3 Destination = CommonCore.Constants.InvalidVector3Position;
                if (LinkedBlackboard.TryGet(CommonCore.Names.MoveToLocation, out Destination, CommonCore.Constants.InvalidVector3Position))
                {
                    if (Destination == CommonCore.Constants.InvalidVector3Position)
                        return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

                    // destination has moved far enough for a repath?
                    if ((LastDestination - Destination).sqrMagnitude >= RecalculateThresholdSq)
                    {
                        Vector3 DestinationOrientation = CommonCore.Constants.InvalidVector3Position;
                        LinkedBlackboard.TryGet(CommonCore.Names.MoveToEndOrientation, out DestinationOrientation, CommonCore.Constants.InvalidVector3Position);

                        Vector3? EndOrientation = (DestinationOrientation != CommonCore.Constants.InvalidVector3Position) ? DestinationOrientation : null;

                        if (OwningTree.NavigationInterface.SetMoveLocation(Self, Destination, EndOrientation, StoppingDistance))
                        {
                            LastDestination = Destination;
                            return SetStatusAndCalculateReturnValue(EBTNodeResult.InProgress);
                        }

                        return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
                    }
                }
            }

            if (OwningTree.NavigationInterface.IsPathfindingOrMoving(Self))
                return SetStatusAndCalculateReturnValue(EBTNodeResult.InProgress);
            else if (OwningTree.NavigationInterface.HasReachedDestination(Self))
                return SetStatusAndCalculateReturnValue(bContinuousMode ? EBTNodeResult.InProgress : EBTNodeResult.Succeeded);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);
        }

        protected override void OnExit()
        {
            base.OnExit();

            if (OwningTree.NavigationInterface.IsPathfindingOrMoving(Self))
                OwningTree.NavigationInterface.StopMoving(Self);
        }
    }
}
