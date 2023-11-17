using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AICharacterMotor))]
public class Navigation_UnityNavMesh : BaseNavigation
{
    NavMeshAgent LinkedAgent;
    AICharacterMotor AIMotor;

    Vector3[] CurrentPath;
    int TargetPoint = -1;

    protected override void Initialise()
    {
        LinkedAgent = GetComponent<NavMeshAgent>();
        AIMotor = GetComponent<AICharacterMotor>();

        LinkedAgent.updatePosition = false;
        LinkedAgent.updateRotation = false;
    }

    protected override bool RequestPath()
    {
        LinkedAgent.speed = MaxMoveSpeed;
        LinkedAgent.angularSpeed = RotationSpeed;
        LinkedAgent.stoppingDistance = DestinationReachedThreshold;

        LinkedAgent.SetDestination(Destination);
        
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
                OnFailedToFindPath();
        }
    }

    protected override void Tick_PathFollowing()
    {
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

                OnReachedDestination();
                return;
            }

            // refresh the target information
            targetPosition = CurrentPath[TargetPoint];
        }

         AIMotor.SteerTowards(targetPosition, RotationSpeed, DestinationReachedThreshold, MaxMoveSpeed);

        if (DEBUG_ShowHeading)
            Debug.DrawLine(transform.position + Vector3.up, LinkedAgent.steeringTarget, Color.green);
    }

    protected override void Tick_OrientingAtEndOfPath()
    {
        if (AIMotor.LookTowards(LookTarget, RotationSpeed))
            OnFacingLookTarget();
    }

    protected override void Tick_Animation()
    {
        float forwardsSpeed = State != EState.Idle ? Vector3.Dot(LinkedAgent.velocity, transform.forward) / LinkedAgent.speed : 0f;
        float sidewaysSpeed = State != EState.Idle ? Vector3.Dot(LinkedAgent.velocity, transform.right) / LinkedAgent.speed : 0f;
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

        AIMotor.Stop();
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
