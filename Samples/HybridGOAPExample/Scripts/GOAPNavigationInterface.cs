using CommonCore;
using UnityEngine;
using UnityEngine.Events;

namespace HybridGOAPExample
{
    public class GOAPNavigationInterface : MonoBehaviour, INavigationInterface
    {
        [SerializeField] UnityEvent<Vector3, float, System.Action<Vector3>> OnFindNearestNavigableLocation;
        [SerializeField] UnityEvent<Vector3, float, System.Action<bool>> OnSetMoveLocation;
        [SerializeField] UnityEvent<System.Action<bool>> OnIsPathfindingOrMovingFn;
        [SerializeField] UnityEvent<System.Action<bool>> OnIsAtDestinationFn;
        [SerializeField] UnityEvent<System.Action<bool>> OnStopMovingFn;

        public Vector3 FindNearestNavigableLocation(GameObject InMover, Vector3 InSearchPoint, float InSearchRange)
        {
            Vector3 FoundPosition = CommonCore.Constants.InvalidVector3Position;
            OnFindNearestNavigableLocation.Invoke(InSearchPoint, InSearchRange, (Vector3 InFoundPosition) =>
            {
                FoundPosition = InFoundPosition;
            });

            return FoundPosition;
        }

        public bool IsAtDestination(GameObject InMover)
        {
            bool bResult = false;

            OnIsAtDestinationFn.Invoke((bool bInResult) =>
            {
                bResult = bInResult;
            });

            return bResult;
        }

        public bool IsPathfindingOrMoving(GameObject InMover)
        {
            bool bResult = false;

            OnIsPathfindingOrMovingFn.Invoke((bool bInResult) =>
            {
                bResult = bInResult;
            });

            return bResult;
        }

        public bool SetMoveLocation(GameObject InMover, Vector3 InDestination, float InStoppingDistance)
        {
            bool bResult = false;

            OnSetMoveLocation.Invoke(InDestination, InStoppingDistance, (bool bInResult) =>
            {
                bResult = bInResult;
            });

            return bResult;
        }

        public bool StopMoving(GameObject InMover)
        {
            bool bResult = false;

            OnStopMovingFn.Invoke((bool bInResult) =>
            {
                bResult = bInResult;
            });

            return bResult;
        }
    }
}
