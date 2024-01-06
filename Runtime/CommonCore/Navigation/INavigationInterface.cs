using UnityEngine;

namespace CommonCore
{
    public interface INavigationInterface : ILocatableService
    {
        bool SetMoveLocation(GameObject InMover, Vector3 InDestination, float InStoppingDistance);
        bool IsPathfindingOrMoving(GameObject InMover);
        bool HasReachedDestination(GameObject InMover);
        bool StopMoving(GameObject InMover);
        Vector3 FindNearestNavigableLocation(GameObject InMover, Vector3 InSearchPoint, float InSearchRange);
    }
}
