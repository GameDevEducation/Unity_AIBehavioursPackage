using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Cinemachine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCharacterMotor : CharacterMotor
{
    public enum ECameraMode
    {
        FirstPerson,
        ThirdPerson_CameraRotatesAvatar,
        ThirdPerson_MovementRotatesAvatar
    }

    [Header("Player")]
    [SerializeField] protected ECameraMode CameraMode = ECameraMode.FirstPerson;

    [Header("First Person")]
    [SerializeField] protected Transform FirstPersonCameraTransform;
    [SerializeField] protected CinemachineVirtualCamera FirstPersonCamera;

    [Header("Third Person")]
    [SerializeField] protected Transform ThirdPersonCameraTransform;
    [SerializeField] protected CinemachineVirtualCamera ThirdPersonCamera;
    [SerializeField] protected float CameraBoomLength = 15f;
    [SerializeField] protected float CameraBoom_MinHeight = 2f;
    [SerializeField] protected float CameraBoom_DefaultHeight = 5f;
    [SerializeField] protected float CameraBoom_MaxHeight = 15f;
    [SerializeField] protected Animator AnimController;

    protected float CurrentCameraPitch = 0f;
    protected float CurrentCameraYaw = 0f;
    protected float HeadbobProgress = 0f;
    protected float Camera_CurrentTime = 0f;

    protected Transform CameraTransform => CameraMode == ECameraMode.FirstPerson ? FirstPersonCameraTransform : ThirdPersonCameraTransform;
    protected CinemachineVirtualCamera LinkedCamera => CameraMode == ECameraMode.FirstPerson ? FirstPersonCamera : ThirdPersonCamera;

    public bool SendUIInteractions { get; protected set; } = true;

    #region Input System Handling
    protected virtual void OnMove(InputValue value)
    {
        State.Input_Move = value.Get<Vector2>();
    }

    protected virtual void OnLook(InputValue value)
    {
        State.Input_Look = value.Get<Vector2>();
    }

    protected virtual void OnJump(InputValue value)
    {
        State.Input_Jump = value.isPressed;
    }

    protected virtual void OnRun(InputValue value)
    {
        State.Input_Run = value.isPressed;
    }

    protected virtual void OnCrouch(InputValue value)
    {
        State.Input_Crouch = value.isPressed;
    }

    protected virtual void OnPrimaryAction(InputValue value)
    {
        State.Input_PrimaryAction = value.isPressed;

        // need to inject pointer event
        if (State.Input_PrimaryAction && SendUIInteractions)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Mouse.current.position.ReadValue();

            // raycast against the UI
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (RaycastResult result in results)
            {
                if (result.distance < Config.MaxInteractionDistance)
                    ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
            }
        }

        if (State.Input_PrimaryAction)
            OnPrimary.Invoke();
    }

    protected virtual void OnSecondaryAction(InputValue value)
    {
        State.Input_SecondaryAction = value.isPressed;

        if (State.Input_SecondaryAction)
            OnSecondary.Invoke();
    }

    #endregion

    protected override void Awake()
    {
        base.Awake();

        SendUIInteractions = Config.SendUIInteractions;

        if (CameraMode == ECameraMode.ThirdPerson_MovementRotatesAvatar)
            State.MovementFrameTransform = ThirdPersonCameraTransform;
    }

    protected override void Start()
    {
        SetCursorLock(true);

        base.Start();

        ConfigureCameraForMode();
    }

    protected virtual void ConfigureCameraForMode()
    {
        if (CameraMode == ECameraMode.FirstPerson)
        {
            CameraTransform.localPosition = Vector3.up * (State.CurrentHeight + Config.Camera_VerticalOffset);
            LinkedCamera.LookAt = null;
        }
        else
        {
            CameraTransform.localPosition = Vector3.up * CameraBoom_DefaultHeight - 
                                            Vector3.forward * CameraBoomLength;
            LinkedCamera.LookAt = transform;

            if (CameraMode == ECameraMode.ThirdPerson_MovementRotatesAvatar)
            {
                float currentAngle = transform.eulerAngles.y * Mathf.Deg2Rad;
                Vector3 localCameraPos = new Vector3(CameraBoomLength * Mathf.Sin(currentAngle),
                                                     CameraBoom_DefaultHeight,
                                                     CameraBoomLength * Mathf.Cos(currentAngle));

                CameraTransform.localPosition = localCameraPos;

                ThirdPersonCameraTransform.SetParent(null);
            }
        }
    }

    protected override void Update()
    {
        base.Update();

        if (AnimController != null)
        {
            float forwardsSpeed = Vector3.Dot(State.LinkedRB.velocity, transform.forward) / Config.RunSpeed;
            float sidewaysSpeed = Vector3.Dot(State.LinkedRB.velocity, transform.right) / Config.RunSpeed;

            AnimController.SetFloat("ForwardsSpeed", forwardsSpeed);
            AnimController.SetFloat("SidewaysSpeed", sidewaysSpeed);
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        UpdateCamera();
    }

    protected void UpdateCamera()
    {
        // not allowed to look around?
        if (State.IsLookingLocked)
            return;

        // ignore any camera input for a brief time (mostly helps editor side when hitting play button)
        if (Camera_CurrentTime < Config.Camera_InitialDiscardTime)
        {
            Camera_CurrentTime += Time.deltaTime;
            return;
        }

        // allow surface to effect sensitivity
        float hSensitivity = Config.Camera_HorizontalSensitivity;
        float vSensitivity = Config.Camera_VerticalSensitivity;
        if (State.CurrentSurfaceSource != null)
        {
            hSensitivity = State.CurrentSurfaceSource.Effect(hSensitivity, EEffectableParameter.CameraSensitivity);
            vSensitivity = State.CurrentSurfaceSource.Effect(vSensitivity, EEffectableParameter.CameraSensitivity);
        }

        if (CameraMode == ECameraMode.FirstPerson)
            UpdateCamera_FirstPerson(hSensitivity, vSensitivity);
        else
            UpdateCamera_ThirdPerson(hSensitivity, vSensitivity);
    }

    protected void UpdateCamera_FirstPerson(float hSensitivity, float vSensitivity)
    {
        // calculate our camera inputs
        float cameraYawDelta = State.Input_Look.x * hSensitivity * Time.deltaTime;
        float cameraPitchDelta = State.Input_Look.y * vSensitivity * Time.deltaTime * (Config.Camera_InvertY ? 1f : -1f);

        // rotate the character
        transform.localRotation = transform.localRotation * Quaternion.Euler(0f, cameraYawDelta, 0f);

        CameraTransform.localPosition = Vector3.up * (State.CurrentHeight + Config.Camera_VerticalOffset);

        // headbob enabled and on the ground?
        if (Config.Headbob_Enable && State.IsGrounded)
        {
            float currentSpeed = State.LinkedRB.velocity.magnitude;

            // moving fast enough to bob?
            Vector3 defaultCameraOffset = Vector3.up * (State.CurrentHeight + Config.Camera_VerticalOffset);
            if (currentSpeed >= Config.Headbob_MinSpeedToBob)
            {
                float speedFactor = currentSpeed / (Config.CanRun ? Config.RunSpeed : Config.WalkSpeed);

                // update our progress
                HeadbobProgress += Time.deltaTime / Config.Headbob_PeriodVsSpeedFactor.Evaluate(speedFactor);
                HeadbobProgress %= 1f;

                // determine the maximum translations
                float maxVTranslation = Config.Headbob_VTranslationVsSpeedFactor.Evaluate(speedFactor);
                float maxHTranslation = Config.Headbob_HTranslationVsSpeedFactor.Evaluate(speedFactor);

                float sinProgress = Mathf.Sin(HeadbobProgress * Mathf.PI * 2f);

                // update the camera location
                defaultCameraOffset += Vector3.up * sinProgress * maxVTranslation;
                defaultCameraOffset += Vector3.right * sinProgress * maxHTranslation;
            }
            else
                HeadbobProgress = 0f;

            CameraTransform.localPosition = Vector3.MoveTowards(CameraTransform.localPosition,
                                                                defaultCameraOffset,
                                                                Config.Headbob_TranslationBlendSpeed * Time.deltaTime);
        }

        // tilt the camera
        CurrentCameraPitch = Mathf.Clamp(CurrentCameraPitch + cameraPitchDelta,
                                         Config.Camera_MinPitch,
                                         Config.Camera_MaxPitch);
        CameraTransform.localRotation = Quaternion.Euler(CurrentCameraPitch, 0f, 0f);
    }

    protected void UpdateCamera_ThirdPerson(float hSensitivity, float vSensitivity)
    {
        // calculate our camera inputs
        float cameraYawDelta = State.Input_Look.x * hSensitivity * Time.deltaTime;
        float cameraHeightDelta = State.Input_Look.y * vSensitivity * Time.deltaTime * (Config.Camera_InvertY ? 1f : -1f);

        if (CameraMode == ECameraMode.ThirdPerson_CameraRotatesAvatar)
        {
            Vector3 localCameraPos = CameraTransform.localPosition;

            transform.localRotation = transform.localRotation * Quaternion.Euler(0f, cameraYawDelta, 0f);

            localCameraPos.y = Mathf.Clamp(localCameraPos.y + cameraHeightDelta, CameraBoom_MinHeight, CameraBoom_MaxHeight);
            CameraTransform.localPosition = localCameraPos;
        }
        else
        {
            if (!Mathf.Approximately(State.LastRequestedVelocity.sqrMagnitude, 0f)) 
            {
                transform.rotation = Quaternion.LookRotation(State.LastRequestedVelocity, State.UpVector);
            }

            CurrentCameraYaw += cameraYawDelta;

            float currentCameraHeight = Vector3.Dot(CameraTransform.position - transform.position, State.UpVector);
            currentCameraHeight += cameraHeightDelta;
            currentCameraHeight = Mathf.Clamp(currentCameraHeight, CameraBoom_MinHeight, CameraBoom_MaxHeight);

            float currentAngle = CurrentCameraYaw * Mathf.Deg2Rad;
            Vector3 localCameraPos = new Vector3(CameraBoomLength * Mathf.Sin(currentAngle),
                                                 currentCameraHeight,
                                                 CameraBoomLength * Mathf.Cos(currentAngle));

            CameraTransform.position = transform.position + localCameraPos;
        }
    }

    public void SetCursorLock(bool locked)
    {
        Cursor.visible = !locked;
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
    }
}
