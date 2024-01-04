using UnityEngine;

[CreateAssetMenu(menuName = "Character Motor/Config", fileName = "CharacterMotorConfig")]
public class CharacterMotorConfig : ScriptableObject
{
    public enum EAutoParentMode
    {
        Anything,
        LookForAutoParentTargetComponent
    }

    [Header("Character")]
    public float Height = 1.8f;
    public float Radius = 0.35f;

    [Header("Grounded Check")]
    public LayerMask GroundedLayerMask = ~0;
    public float GroundedCheckBuffer = 0.2f;

    [Header("Ceiling Check")]
    public LayerMask CeilingCheckLayerMask = ~0;
    public float CeilingCheckRangeBuffer = 0.05f;
    public float CeilingCheckRadiusBuffer = 0.05f;

    [Header("Step Up Check")]
    public float StepCheck_LookAheadRange = 0.1f;
    public float StepCheck_MaxStepHeight = 0.3f;

    [Header("Physics Materials")]
    public PhysicMaterial Material_Default;
    public PhysicMaterial Material_Jumping;

    [Header("Camera")]
    public bool Camera_InvertY = false;
    public float Camera_HorizontalSensitivity = 10f;
    public float Camera_VerticalSensitivity = 10f;
    public float Camera_MinPitch = -75f;
    public float Camera_MaxPitch = 75f;
    public float Camera_InitialDiscardTime = 0.1f;
    public float Camera_VerticalOffset = -0.1f;

    [Header("Headbob")]
    public bool Headbob_Enable = true;
    public float Headbob_MinSpeedToBob = 0.1f;
    public float Headbob_TranslationBlendSpeed = 1f;
    public AnimationCurve Headbob_VTranslationVsSpeedFactor;
    public AnimationCurve Headbob_HTranslationVsSpeedFactor;
    public AnimationCurve Headbob_PeriodVsSpeedFactor;

    [Header("Movement")]
    public float WalkSpeed = 10f;
    public float RunSpeed = 15f;
    public bool CanRun = true;
    public bool IsRunToggle = true;
    public float SlopeLimit = 60f;
    public float Acceleration = 1f;
    public bool AutoParent = false;
    public EAutoParentMode AutoParentMode = EAutoParentMode.Anything;

    [Header("Crouching")]
    public bool CanCrouch = true;
    public bool IsCrouchToggle = true;
    public float CrouchHeight = 0.9f;
    public float CrouchSpeedMultiplier = 0.5f;
    public float CrouchTransitionTime = 0.25f;

    [Header("Falling")]
    public float FallAcceleration = 20f;
    public float CoyoteTime = 0.1f;
    public float MinFallSpeedToTakeDamage = 15f;
    public float FallSpeedForMaximumDamage = 50f;
    [Range(0f, 1f)] public float MinimumFallDamagePercentage = 0.05f;
    [Range(0f, 1f)] public float MaximumFallDamagePercentage = 1.0f;

    [Header("Air Control")]
    public bool CanAirControl = true;
    public float AirControlMaxSpeed = 10f;

    [Header("Jumping")]
    public bool CanJump = true;
    public bool CanDoubleJump = true;
    public float JumpVelocity = 5f;
    public float JumpTime = 0.25f;

    [Header("User Interface")]
    public bool SendUIInteractions = true;
    public float MaxInteractionDistance = 2f;

    [Header("Audio")]
    public float FootstepInterval_Walking = 0.4f;
    public float FootstepInterval_Running = 0.2f;
    public float MinAirTimeForLandedSound = 0.2f;

    [Header("Stamina")]
    public float MaxStamina = 100f;
    public float StaminaCost_Jumping = 10f;
    public float StaminaCost_Running = 4f;
    public float StaminaRecoveryRate = 10f;
    public float StaminaRecoveryDelay = 0.5f;

    [Header("Health")]
    public float MaxHealth = 100f;
    public float HealthRecoveryRate = 10f;
    public float HealthRecoveryDelay = 5f;
}
