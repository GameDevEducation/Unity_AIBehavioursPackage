using UnityEngine;
using CommonCore;

public abstract class BaseNavigation : MonoBehaviour, INavigationInterface
{
    public enum EState
    {
        Idle = 0,
        FindingPath = 1,
        OrientingAtStartOfPath = 2,
        FollowingPath = 3,
        OrientingAtEndOfPath = 4,

        Failed_NoPathExists = 100
    }

    [Header("Path Following")]
    [SerializeField] protected float DestinationReachedThreshold = 0.25f;
    [SerializeField] protected float MaxMoveSpeed = 5f;
    [SerializeField] protected float RotationSpeed = 120f;

    [Header("Animation")]
    [SerializeField] protected Animator AnimController;

    [Header("Debug Tools")]
    [SerializeField] protected bool DEBUG_UseMoveTarget;
    [SerializeField] protected Transform DEBUG_MoveTarget;
    [SerializeField] protected bool DEBUG_ShowHeading;

    public Vector3 Destination { get; private set; }
    public EState State { get; private set; } = EState.Idle;
    public Transform LookTarget { get; private set; } = null;

    public bool IsFindingOrFollowingPath => State == EState.FindingPath || State == EState.OrientingAtStartOfPath || State == EState.FollowingPath;
    public bool HasLookTarget => LookTarget != null;
    public bool IsAtDestination
    {
        get
        {
            if (State != EState.Idle)
                return false;

            Vector3 vecToDestination = Destination - transform.position;
            vecToDestination.y = 0f;

            return vecToDestination.magnitude <= DestinationReachedThreshold;
        }
    }

    void Awake()
    {
        ServiceLocator.RegisterService<INavigationInterface>(this, gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    // Update is called once per frame
    void Update()
    {
        if (DEBUG_UseMoveTarget)
            SetDestination(DEBUG_MoveTarget.position);

        if (State == EState.FindingPath)
            Tick_Pathfinding();
        if (State == EState.OrientingAtStartOfPath)
            Tick_OrientingAtStartOfPath();
        if (State == EState.OrientingAtEndOfPath)
            Tick_OrientingAtEndOfPath();

        Tick_Default();

        if (AnimController != null)
            Tick_Animation();
    }

    void FixedUpdate()
    {
        if (State == EState.FollowingPath)
            Tick_PathFollowing();
    }

    public void SetDestinationReachedThreshold(float newThreshold)
    {
        DestinationReachedThreshold = newThreshold;
    }

    public bool SetDestination(Vector3 newDestination)
    {
        return SetDestination(newDestination, LookTarget);
    }

    public bool SetDestination(Vector3 newDestination, Transform lookTarget)
    {
        LookTarget = lookTarget;

        // location is already our destination?
        Vector3 destinationDelta = newDestination - Destination;
        destinationDelta.y = 0f;
        if (IsFindingOrFollowingPath && (destinationDelta.magnitude <= DestinationReachedThreshold))
            return true;

        // are we already near the destination
        destinationDelta = newDestination - transform.position;
        destinationDelta.y = 0f;
        if (destinationDelta.magnitude <= DestinationReachedThreshold)
        {
            if (HasLookTarget)
                State = EState.OrientingAtEndOfPath;

            return true;
        }

        Destination = newDestination;

        return RequestPath();
    }

    public virtual void StopMovement()
    {
        State = EState.Idle;
    }

    public abstract bool FindNearestPoint(Vector3 searchPos, float range, out Vector3 foundPos);

    protected abstract void Initialise();

    protected abstract bool RequestPath();

    protected virtual void OnBeganPathFinding()
    {
        State = EState.FindingPath;
    }

    protected virtual void OnPathFound()
    {
        State = EState.OrientingAtStartOfPath;
    }

    protected virtual void OnFailedToFindPath()
    {
        State = EState.Failed_NoPathExists;
    }

    protected virtual void OnFacingPathStart()
    {
        State = EState.FollowingPath;
    }

    protected virtual void OnReachedDestination()
    {
        State = HasLookTarget ? EState.OrientingAtEndOfPath : EState.Idle;
    }

    protected virtual void OnFacingLookTarget()
    {
        State = EState.Idle;
    }

    protected abstract void Tick_Default();
    protected abstract void Tick_Pathfinding();
    protected abstract void Tick_PathFollowing();
    protected abstract void Tick_OrientingAtStartOfPath();
    protected abstract void Tick_OrientingAtEndOfPath();
    protected abstract void Tick_Animation();

    #region INavigationInterface Interfaces

    public bool SetMoveLocation(GameObject InMover, Vector3 InDestination, float InStoppingDistance)
    {
        if (SetDestination(InDestination))
        {
            SetDestinationReachedThreshold(InStoppingDistance);
            return true;
        }

        return false;
    }

    public bool IsPathfindingOrMoving(GameObject InMover)
    {
        return IsFindingOrFollowingPath;
    }

    public bool HasReachedDestination(GameObject InMover)
    {
        return IsAtDestination;
    }

    public bool StopMoving(GameObject InMover)
    {
        StopMovement();
        return true;
    }

    public Vector3 FindNearestNavigableLocation(GameObject InMover, Vector3 InSearchPoint, float InSearchRange)
    {
        Vector3 FoundPosition = CommonCore.Constants.InvalidVector3Position;

        if (FindNearestPoint(InSearchPoint, InSearchRange, out FoundPosition))
            return FoundPosition;

        return CommonCore.Constants.InvalidVector3Position;
    }

    #endregion
}
