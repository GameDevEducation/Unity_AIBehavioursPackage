using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    public interface IGOAPNavigationInterface
    {
        bool SetMoveLocation(GameObject InMover, Vector3 InDestination, float InStoppingDistance);
        bool IsPathfindingOrMoving(GameObject InMover);
        bool IsAtDestination(GameObject InMover);
        bool StopMoving(GameObject InMover);
        Vector3 FindNearestNavigableLocation(GameObject InMover, Vector3 InSearchPoint, float InSearchRange);
    }
}
