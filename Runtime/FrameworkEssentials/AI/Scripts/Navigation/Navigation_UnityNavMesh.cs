using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AICharacterMotor))]
public class Navigation_UnityNavMesh : BaseNavigation
{
    [SerializeField] bool OverrideAgentLocomotion = false;

    NavMeshAgent LinkedAgent;
    AICharacterMotor AIMotor;

    Vector3[] CurrentPath;
    int TargetPoint = -1;

    public override bool HasDestination { get; protected set; } = false;

    protected override void Initialise()
    {
        LinkedAgent = GetComponent<NavMeshAgent>();
        AIMotor = GetComponent<AICharacterMotor>();

        SetIsAgentControllingLocomotion(!OverrideAgentLocomotion);
    }

    void SetIsAgentControllingLocomotion(bool bInAgentHasControl)
    {
        LinkedAgent.updatePosition = LinkedAgent.updateRotation = bInAgentHasControl;
    }

    public override void SetDestinationReachedThreshold(float newThreshold)
    {
        base.SetDestinationReachedThreshold(newThreshold);

        LinkedAgent.stoppingDistance = newThreshold;
    }

    protected override bool RequestPath()
    {
        LinkedAgent.speed = MaxMoveSpeed;
        LinkedAgent.angularSpeed = RotationSpeed;
        LinkedAgent.stoppingDistance = DestinationReachedThreshold;

        LinkedAgent.SetDestination(Destination);
        HasDestination = true;

        OnBeganPathFinding();

        return true;
    }

    protected override void Tick_Default()
    {

    }

    protected override void Tick_Pathfinding()
    {
        // no pathfinding in progress?
        if (!LinkedAgent.pathPending)
        {
            if (LinkedAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                CurrentPath = LinkedAgent.path.corners;
                TargetPoint = 0;
                OnPathFound();
            }
            else
            {
                OnFailedToFindPath();
                HasDestination = false;
            }
        }
    }

    protected override void Tick_PathFollowing()
    {
        if (OverrideAgentLocomotion)
            Tick_PathFollowing_LocalLocomotionControl();
        else
            Tick_PathFollowing_AgentLocomotionControl();

        if (DEBUG_ShowHeading)
            Debug.DrawLine(transform.position + Vector3.up, LinkedAgent.steeringTarget, Color.green);
    }

    protected void Tick_PathFollowing_LocalLocomotionControl()
    {
        // make sure the agent is not controlling the locomotion
        SetIsAgentControllingLocomotion(false);

        Vector3 targetPosition = CurrentPath[TargetPoint];

        // get the 2D vector to the target
        Vector3 vecToTarget = targetPosition - transform.position;
        vecToTarget.y = 0f;

        // reached the target point?
        if (vecToTarget.magnitude <= DestinationReachedThreshold)
        {
            // advance to next point
            ++TargetPoint;

            // reached destination?
            if (TargetPoint == CurrentPath.Length)
            {
                AIMotor.Stop();
                HasDestination = false;

                OnReachedDestination();
                return;
            }

            // refresh the target information
            targetPosition = CurrentPath[TargetPoint];
        }

        AIMotor.SteerTowards(targetPosition, RotationSpeed, DestinationReachedThreshold, MaxMoveSpeed);
    }

    protected void Tick_PathFollowing_AgentLocomotionControl()
    {
        // make sure the agent is in control
        SetIsAgentControllingLocomotion(true);

        if (LinkedAgent.hasPath && LinkedAgent.remainingDistance <= LinkedAgent.stoppingDistance)
        {
            AIMotor.Stop();
            HasDestination = false;

            // take control back from the agent
            SetIsAgentControllingLocomotion(false);

            OnReachedDestination();
            return;
        }
    }

    protected override void Tick_OrientingAtStartOfPath()
    {
        if ((CurrentPath == null) || (CurrentPath.Length < 2) || AIMotor.LookTowards(CurrentPath[1], RotationSpeed, true))
            OnFacingPathStart();
    }

    protected override void Tick_OrientingAtEndOfPath()
    {
        if (AIMotor.LookTowards(LookTarget, RotationSpeed, false))
            OnFacingLookTarget();
    }

    protected override void Tick_Animation()
    {
        float forwardsSpeed = State == EState.FollowingPath ? Vector3.Dot(LinkedAgent.velocity, transform.forward) / LinkedAgent.speed : 0f;
        float sidewaysSpeed = State == EState.FollowingPath ? Vector3.Dot(LinkedAgent.velocity, transform.right) / LinkedAgent.speed : 0f;
        AnimController.SetFloat("ForwardsSpeed", forwardsSpeed);
        AnimController.SetFloat("SidewaysSpeed", sidewaysSpeed);
    }

    private void LateUpdate()
    {
        LinkedAgent.nextPosition = transform.position;
    }

    public override void StopMovement()
    {
        base.StopMovement();

        LinkedAgent.ResetPath();

        CurrentPath = null;
        TargetPoint = -1;
        HasDestination = false;

        AIMotor.Stop();
    }

    public override bool IsLocationReachable(Vector3 startPosition, Vector3 destinationPosition, float searchRange = -1.0f)
    {
        float workingDistance = Mathf.Max(searchRange, LinkedAgent.height * 0.5f);

        NavMeshHit hitResult;
        return NavMesh.SamplePosition(destinationPosition, out hitResult, workingDistance, NavMesh.AllAreas);
    }

    public override bool FindNearestPoint(Vector3 searchPos, float range, out Vector3 foundPos)
    {
        NavMeshHit hitResult;
        if (NavMesh.SamplePosition(searchPos, out hitResult, range, NavMesh.AllAreas))
        {
            foundPos = hitResult.position;
            return true;
        }

        foundPos = searchPos;

        return false;
    }

}
