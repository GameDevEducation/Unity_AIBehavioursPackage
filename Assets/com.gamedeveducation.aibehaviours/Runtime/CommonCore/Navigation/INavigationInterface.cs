using UnityEngine;

namespace CommonCore
{
    public interface INavigationInterface : ILocatableService
    {
        bool HasDestination { get; }
        Vector3 Destination { get; }

        bool SetMoveLocation(GameObject InMover, Vector3 InDestination, float InStoppingDistance);
        bool IsPathfindingOrMoving(GameObject InMover);
        bool HasReachedDestination(GameObject InMover);
        bool StopMoving(GameObject InMover);

        bool IsLocationReachable(Vector3 InStartPosition, Vector3 InDestinationPosition, float InSearchRange = -1.0f);

        Vector3 FindNearestNavigableLocation(GameObject InMover, Vector3 InSearchPoint, float InSearchRange);
    }
}
