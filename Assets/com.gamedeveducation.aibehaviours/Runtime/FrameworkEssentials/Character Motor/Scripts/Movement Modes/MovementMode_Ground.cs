using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MovementMode_Ground : MonoBehaviour, IMovementMode
{
    [SerializeField] protected UnityEvent<bool> OnRunChanged = new UnityEvent<bool>();
    [SerializeField] protected UnityEvent<Vector3, float> OnHitGround = new UnityEvent<Vector3, float>();
    [SerializeField] protected UnityEvent<Vector3> OnBeginJump = new UnityEvent<Vector3>();
    [SerializeField] protected UnityEvent<Vector3, float> OnFootstep = new UnityEvent<Vector3, float>();

    protected CharacterMotor.MotorState State;
    protected CharacterMotorConfig Config;
    protected CharacterMotor Motor;

    protected float JumpTimeRemaining = 0f;
    protected float TimeSinceLastFootstepAudio = 0f;
    protected float TimeFalling = 0f;
    protected float OriginalDrag;

    public bool IsJumping => IsInJumpingRisePhase || IsInJumpingFallPhase;
    public bool IsInJumpingFallPhase { get; protected set; } = false;
    public bool IsInJumpingRisePhase { get; protected set; } = false;
    public int JumpCount { get; protected set; } = 0;

    public float CoyoteTimeRemaining { get; protected set; } = 0f;
    public bool InCoyoteTime => CoyoteTimeRemaining > 0f;
    public bool IsGroundedOrInCoyoteTime => State.IsGrounded || InCoyoteTime;

    public float CurrentMaxSpeed
    {
        get
        {
            float speed = 0f;

            if (IsGroundedOrInCoyoteTime || IsJumping)
                speed = (State.IsRunning ? Config.RunSpeed : Config.WalkSpeed) * (State.IsCrouched ? Config.CrouchSpeedMultiplier : 1f);
            else
                speed = Config.CanAirControl ? Config.AirControlMaxSpeed : 0f;

            return State.CurrentSurfaceSource != null ? State.CurrentSurfaceSource.Effect(speed, EEffectableParameter.Speed) : speed;
        }
        set
        {
            throw new System.NotImplementedException($"CurrentMaxSpeed cannot be set directly. Update the motor config to change speed.");
        }
    }

    public void Initialise(CharacterMotorConfig config, CharacterMotor motor, CharacterMotor.MotorState state)
    {
        Config = config;
        Motor = motor;
        State = state;

        State.LinkedCollider.material = Config.Material_Default;
        State.LinkedCollider.radius = Config.Radius;
        State.LinkedCollider.height = state.CurrentHeight;
        State.LinkedCollider.center = Vector3.up * (state.CurrentHeight * 0.5f);

        OriginalDrag = State.LinkedRB.drag;
    }

    public void FixedUpdate_PreGroundedCheck()
    {
        if (State.LocalGravity == null)
            State.LinkedRB.AddForce(Physics.gravity, ForceMode.Acceleration);

        // align to the local gravity vector
        transform.rotation = Quaternion.FromToRotation(transform.up, State.UpVector) * transform.rotation;
    }

    public RaycastHit FixedUpdate_GroundedCheck()
    { 
        bool wasGrounded = State.IsGrounded;
        bool wasRunning = State.IsRunning;

        RaycastHit groundCheckResult = UpdateIsGrounded();

        // activate coyote time?
        if (wasGrounded && !State.IsGrounded)
            CoyoteTimeRemaining = Config.CoyoteTime;
        else
        {
            // reduce the coyote time
            if (CoyoteTimeRemaining > 0)
                CoyoteTimeRemaining -= Time.deltaTime;
        }

        UpdateRunning(groundCheckResult);

        if (wasRunning != State.IsRunning)
            OnRunChanged.Invoke(State.IsRunning);

        return groundCheckResult;
    }

    public void FixedUpdate_OnBecameGrounded()
    {
        State.LinkedCollider.material = Config.Material_Default;
        State.LinkedRB.drag = OriginalDrag;
        TimeSinceLastFootstepAudio = 0f;
        CoyoteTimeRemaining = 0f;
       
        OnHitGround.Invoke(State.LinkedRB.position, State.LastRequestedVelocity.magnitude);
    }

    protected RaycastHit UpdateIsGrounded()
    {
        RaycastHit hitResult;

        // currently performing a jump
        if (JumpTimeRemaining > 0)
        {
            State.IsGrounded = false;
            return new RaycastHit();
        }

        Vector3 startPos = State.LinkedRB.position + State.UpVector * State.CurrentHeight * 0.5f;
        float groundCheckDistance = (State.CurrentHeight * 0.5f) + Config.GroundedCheckBuffer;

        // perform our spherecast
        float radius = Config.Radius * 0.25f;
        if (Physics.SphereCast(startPos, radius, State.DownVector, out hitResult, groundCheckDistance,
                            Config.GroundedLayerMask, QueryTriggerInteraction.Ignore))
        {
            State.IsGrounded = true;
            TimeFalling = 0f;
            JumpCount = 0;
            JumpTimeRemaining = 0f;
            IsInJumpingFallPhase = false;

            // is autoparenting enabled?
            if (Config.AutoParent)
            {
                // auto parent to anything!
                if (Config.AutoParentMode == CharacterMotorConfig.EAutoParentMode.Anything)
                {
                    if (hitResult.transform != State.CurrentParent)
                    {
                        State.CurrentParent = hitResult.transform;
                        transform.SetParent(State.CurrentParent, true);
                    }
                }
                else
                {
                    // search for our autoparent script
                    var target = hitResult.transform.gameObject.GetComponentInParent<CharacterMotorAutoParentTarget>();
                    if (target != null && target.transform != State.CurrentParent)
                    {
                        State.CurrentParent = target.transform;
                        transform.SetParent(State.CurrentParent, true);
                    }
                }
            }
        }
        else
            State.IsGrounded = false;

        return hitResult;
    }

    public void FixedUpdate_TickMovement(RaycastHit groundCheckResult)
    {
        if (!IsGroundedOrInCoyoteTime)
        {
            // track how long we have been in the air
            State.TimeInAir += Time.deltaTime;

            if (!IsJumping || IsInJumpingFallPhase)
                TimeFalling += Time.deltaTime;
        }

        UpdateMovement(groundCheckResult);
    }

    public void LateUpdate_Tick()
    {
        UpdateCrouch();
    }

    protected void UpdateMovement(RaycastHit groundCheckResult)
    {
        Vector3 forwardVector = Vector3.ProjectOnPlane(State.MovementFrameTransform.forward, State.UpVector).normalized;
        Vector3 sideVector = Vector3.ProjectOnPlane(State.MovementFrameTransform.right, State.UpVector).normalized;

        // calculate our movement input
        Vector3 movementVector = forwardVector * State.Input_Move.y + 
                                 sideVector * State.Input_Move.x;
        movementVector *= CurrentMaxSpeed;

        // are we on the ground?
        if (IsGroundedOrInCoyoteTime)
        {
            // project onto the current surface
            movementVector = Vector3.ProjectOnPlane(movementVector, groundCheckResult.normal);

            // trying to move up too steep a slope
            if (movementVector.y > 0 && Vector3.Angle(State.UpVector, groundCheckResult.normal) > Config.SlopeLimit)
                movementVector = Vector3.zero;
        } // in the air
        else
        {
            movementVector += State.DownVector * Config.FallAcceleration * TimeFalling;
        }

        UpdateJumping(ref movementVector);

        if (IsGroundedOrInCoyoteTime && !IsJumping)
        {
            CheckForStepUp(ref movementVector);

            UpdateFootstepAudio(State.Input_Move);
        }

        // update the velocity
        State.LinkedRB.velocity = Vector3.MoveTowards(State.LinkedRB.velocity, movementVector, Config.Acceleration);
    }

    protected void UpdateFootstepAudio(Vector2 movementInput)
    {
        // is the player attempting to move?
        if (movementInput.magnitude > float.Epsilon)
        {
            // update time since last audio
            TimeSinceLastFootstepAudio += Time.deltaTime;

            // time for footstep audio?
            float footstepInterval = State.IsRunning ? Config.FootstepInterval_Running : Config.FootstepInterval_Walking;
            if (TimeSinceLastFootstepAudio >= footstepInterval)
            {
                OnFootstep.Invoke(State.LinkedRB.position, State.LinkedRB.velocity.magnitude);

                TimeSinceLastFootstepAudio -= footstepInterval;
            }
        }
    }

    protected void CheckForStepUp(ref Vector3 movementVector)
    {
        Vector3 lookAheadStartPoint = State.LinkedRB.position + State.UpVector * (Config.StepCheck_MaxStepHeight * 0.5f);
        Vector3 lookAheadDirection = movementVector.normalized;
        float lookAheadDistance = Config.Radius + Config.StepCheck_LookAheadRange;

        // check if there is a potential step ahead
        if (Physics.Raycast(lookAheadStartPoint, lookAheadDirection, lookAheadDistance,
                            Config.GroundedLayerMask, QueryTriggerInteraction.Ignore))
        {
            lookAheadStartPoint = State.LinkedRB.position + State.UpVector * Config.StepCheck_MaxStepHeight;

            // check if there is clear space above the step
            if (!Physics.Raycast(lookAheadStartPoint, lookAheadDirection, lookAheadDistance,
                                Config.GroundedLayerMask, QueryTriggerInteraction.Ignore))
            {
                Vector3 candidatePoint = lookAheadStartPoint + lookAheadDirection * lookAheadDistance;

                // check the surface of the step
                RaycastHit hitResult;
                if (Physics.Raycast(candidatePoint, State.DownVector, out hitResult, Config.StepCheck_MaxStepHeight * 2f,
                                    Config.GroundedLayerMask, QueryTriggerInteraction.Ignore))
                {
                    // is the step shallow enough in slope
                    if (Vector3.Angle(State.UpVector, hitResult.normal) <= Config.SlopeLimit)
                    {
                        State.LinkedRB.position = hitResult.point;
                    }
                }
            }
        }
    }

    protected void UpdateJumping(ref Vector3 movementVector)
    {
        // jump requested?
        bool triggeredJumpThisFrame = false;
        if (State.Input_Jump && Motor.CanCurrentlyJump)
        {
            State.Input_Jump = false;

            // check if we can jump
            bool triggerJump = true;
            int numJumpsPermitted = Config.CanDoubleJump ? 2 : 1;
            if (JumpCount >= numJumpsPermitted)
                triggerJump = false;
            if (!IsGroundedOrInCoyoteTime && !IsJumping)
                triggerJump = false;

            // jump is permitted?
            if (triggerJump)
            {
                if (JumpCount == 0)
                    triggeredJumpThisFrame = true;

                float jumpTime = Config.JumpTime;
                if (State.CurrentSurfaceSource != null)
                    jumpTime = State.CurrentSurfaceSource.Effect(jumpTime, EEffectableParameter.JumpTime);

                State.LinkedCollider.material = Config.Material_Jumping;
                State.LinkedRB.drag = 0;
                JumpTimeRemaining += jumpTime;
                IsInJumpingRisePhase = true;
                IsInJumpingFallPhase = false;
                CoyoteTimeRemaining = 0f;
                ++JumpCount;

                OnBeginJump.Invoke(State.LinkedRB.position);

                Motor.ConsumeStamina(Config.StaminaCost_Jumping);
            }
        }

        if (IsInJumpingRisePhase)
        {
            // update remaining jump time if not jumping this frame
            if (!triggeredJumpThisFrame)
                JumpTimeRemaining -= Time.deltaTime;

            // jumping finished
            if (JumpTimeRemaining <= 0)
            {
                IsInJumpingRisePhase = false;
                IsInJumpingFallPhase = true;
                TimeFalling = 0;
            }
            else
            {
                Vector3 startPos = State.LinkedRB.position + State.UpVector * State.CurrentHeight * 0.5f;
                float ceilingCheckRadius = Config.Radius + Config.CeilingCheckRadiusBuffer;
                float ceilingCheckDistance = (State.CurrentHeight * 0.5f) - Config.Radius + Config.GroundedCheckBuffer;

                // perform our spherecast
                RaycastHit ceilingHitResult;
                if (Physics.SphereCast(startPos, ceilingCheckRadius, State.UpVector, out ceilingHitResult,
                                       ceilingCheckDistance, Config.GroundedLayerMask, QueryTriggerInteraction.Ignore))
                {
                    IsInJumpingRisePhase = false;
                    IsInJumpingFallPhase = true;
                    JumpTimeRemaining = 0f;
                    movementVector.y = 0f;
                }
                else
                {
                    float jumpVelocity = State.JumpVelocity;

                    if (State.CurrentSurfaceSource != null)
                        jumpVelocity = State.CurrentSurfaceSource.Effect(jumpVelocity, EEffectableParameter.JumpVelocity);

                    movementVector += State.UpVector * (jumpVelocity + Vector3.Dot(movementVector, State.DownVector));
                }
            }
        }
    }

    protected void UpdateRunning(RaycastHit groundCheckResult)
    {
        // no longer able to run?
        if (!Motor.CanCurrentlyRun)
        {
            State.IsRunning = false;
            return;
        }

        // stop running if no input
        if (State.Input_Move.magnitude < float.Epsilon)
            State.IsRunning = false;

        // not grounded AND not jumping
        if (!IsGroundedOrInCoyoteTime && !IsJumping)
        {
            State.IsRunning = false;
            return;
        }

        // cannot run?
        if (!Config.CanRun)
        {
            State.IsRunning = false;
            return;
        }

        // setup our run toggle
        if (Config.IsRunToggle)
        {
            if (State.Input_Run && !State.IsRunning)
                State.IsRunning = true;
        }
        else
            State.IsRunning = State.Input_Run;
    }

    protected void UpdateCrouch()
    {
        // do nothing if either movement or looking are locked
        if (State.IsMovementLocked || State.IsLookingLocked)
            return;

        // not allowed to crouch?
        if (!Config.CanCrouch)
            return;

        // are we jumping or in the air due to falling etc
        if (IsJumping || !IsGroundedOrInCoyoteTime)
        {
            // crouched or transitioning to crouched
            if (State.IsCrouched || State.TargetCrouchState)
            {
                State.TargetCrouchState = false;
                State.InCrouchTransition = true;
            }
        }
        else if (Config.IsCrouchToggle)
        {
            // toggle crouch state?
            if (State.Input_Crouch)
            {
                State.Input_Crouch = false;

                State.TargetCrouchState = !State.TargetCrouchState;
                State.InCrouchTransition = true;
            }
        }
        else
        {
            // request crouch state different to current target
            if (State.Input_Crouch != State.TargetCrouchState)
            {
                State.TargetCrouchState = State.Input_Crouch;
                State.InCrouchTransition = true;
            }
        }

        // update crouch if mid transition
        if (State.InCrouchTransition)
        {
            // Update the progress
            State.CrouchTransitionProgress = Mathf.MoveTowards(State.CrouchTransitionProgress,
                                                               State.TargetCrouchState ? 0f : 1f,
                                                               Time.deltaTime / Config.CrouchTransitionTime);

            // finished changing crouch state
            if (Mathf.Approximately(State.CrouchTransitionProgress, State.TargetCrouchState ? 0f : 1f))
            {
                State.IsCrouched = State.TargetCrouchState;
                State.InCrouchTransition = false;
            }
        }
    }
}
