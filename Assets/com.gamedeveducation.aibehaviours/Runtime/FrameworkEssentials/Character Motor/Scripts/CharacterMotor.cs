using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class CharacterMotor : MonoBehaviour, IDamageable
{
    public enum EParameter
    {
        Height,
        JumpHeight
    }

    public interface IParameterEffector
    {
        float Effect(float currentValue);

        EParameter GetEffectedParameter();

        bool Tick(float deltaTime);
    }

    public class MotorState
    {
        public Rigidbody LinkedRB;
        public GravityTracker LocalGravity;
        public CapsuleCollider LinkedCollider;
        public CharacterMotorConfig LinkedConfig;

        public Transform MovementFrameTransform;

        public Vector2 Input_Move;
        public Vector2 Input_Look;
        public bool Input_Jump;
        public bool Input_Run;
        public bool Input_Crouch;
        public bool Input_PrimaryAction;
        public bool Input_SecondaryAction;

        public bool IsGrounded;
        public bool IsRunning;
        public bool IsCrouched;
        public bool IsMovementLocked;
        public bool IsLookingLocked;

        public Transform CurrentParent;
        public SurfaceEffectSource CurrentSurfaceSource;

        public bool InCrouchTransition = false;
        public bool TargetCrouchState = false;
        public float CrouchTransitionProgress = 1f;

        public float TimeInAir = 0f;

        public Vector3 LastRequestedVelocity = Vector3.zero;

        public Vector3 UpVector => LocalGravity != null ? LocalGravity.Up : Vector3.up;
        public Vector3 DownVector => LocalGravity != null ? LocalGravity.Down : Vector3.down;

        Dictionary<EParameter, List<IParameterEffector>> ActiveEffects = new Dictionary<EParameter, List<IParameterEffector>>();
        Dictionary<EParameter, float> CachedMultipliers = new Dictionary<EParameter, float>();

        public float CurrentHeight
        {
            get
            {
                float heightMultiplier = CachedMultipliers[EParameter.Height];

                if (InCrouchTransition)
                    return heightMultiplier * Mathf.Lerp(LinkedConfig.CrouchHeight, LinkedConfig.Height, CrouchTransitionProgress);

                return heightMultiplier * (IsCrouched ? LinkedConfig.CrouchHeight : LinkedConfig.Height);
            }
            set
            {
                throw new System.NotImplementedException($"CurrentHeight cannot be set directly. Update the motor config to change height.");
            }
        }

        public float JumpVelocity => CachedMultipliers[EParameter.JumpHeight] * LinkedConfig.JumpVelocity;

        public void AddParameterEffector(IParameterEffector newEffector)
        {
            if (!ActiveEffects.ContainsKey(newEffector.GetEffectedParameter()))
                ActiveEffects[newEffector.GetEffectedParameter()] = new List<IParameterEffector>();

            ActiveEffects[newEffector.GetEffectedParameter()].Add(newEffector);

            CacheEffectMultipliers();
        }

        public void Tick(float deltaTime)
        {
            // tick the effects and store if any need to be cleaned up
            List<IParameterEffector> toCleanup = new List<IParameterEffector>();
            foreach (var kvp in ActiveEffects)
            {
                var effectList = kvp.Value;

                foreach (var effect in effectList)
                {
                    if (effect.Tick(deltaTime))
                        toCleanup.Add(effect);
                }
            }

            // perform cleanup
            foreach (var effect in toCleanup)
            {
                ActiveEffects[effect.GetEffectedParameter()].Remove(effect);
            }

            if (toCleanup.Count > 0)
                CacheEffectMultipliers();
        }

        void CacheEffectMultipliers()
        {
            foreach (var rawEnumValue in System.Enum.GetValues(typeof(EParameter)))
            {
                EParameter parameter = (EParameter)rawEnumValue;
                CachedMultipliers[parameter] = 1f;

                if (!ActiveEffects.ContainsKey(parameter))
                    continue;

                foreach (var effector in ActiveEffects[parameter])
                {
                    CachedMultipliers[parameter] = effector.Effect(CachedMultipliers[parameter]);
                }
            }
        }

        public MotorState()
        {
            CacheEffectMultipliers();
        }
    }
    protected MotorState State = new MotorState();

    [SerializeField] protected CharacterMotorConfig Config;

    [SerializeField] protected UnityEvent<float, float> OnStaminaChanged = new();
    [SerializeField] protected UnityEvent<float, float> OnHealthChanged = new();
    [SerializeField] protected UnityEvent<float> OnTookDamage = new();
    [SerializeField] protected UnityEvent<CharacterMotor> OnPlayerDied = new();
    [SerializeField] protected UnityEvent OnPrimary = new();
    [SerializeField] protected UnityEvent OnSecondary = new();
    [SerializeField] protected UnityEvent<Vector3> OnPlayImpactGroundSound = new();

    [Header("Debug Controls")]
    [SerializeField] protected bool DEBUG_OverrideMovement = false;
    [SerializeField] protected Vector2 DEBUG_MovementInput;
    [SerializeField] protected bool DEBUG_ToggleLookLock = false;
    [SerializeField] protected bool DEBUG_ToggleMovementLock = false;

    protected IMovementMode MovementMode;

    protected float PreviousStamina = 0f;
    protected float StaminaRecoveryDelayRemaining = 0f;

    protected float PreviousHealth = 0f;
    protected float HealthRecoveryDelayRemaining = 0f;

    protected float CurrentSurfaceLastTickTime;

    float PreviousHeight;

    public float CurrentStamina { get; protected set; } = 0f;
    public float CurrentHealth { get; protected set; } = 0f;
    public float MaxHealth => Config.MaxHealth;

    public bool CanCurrentlyJump => Config.CanJump && CurrentStamina >= Config.StaminaCost_Jumping;
    public bool CanCurrentlyRun => Config.CanRun && CurrentStamina > 0f;

    protected virtual void Awake()
    {
        State.LinkedRB = GetComponent<Rigidbody>();
        State.LocalGravity = GetComponent<GravityTracker>();
        State.LinkedCollider = GetComponentInChildren<CapsuleCollider>();
        State.LinkedConfig = Config;
        State.MovementFrameTransform = transform;

        SwitchMovementMode<MovementMode_Ground>();

        if (MovementMode == null)
        {
            throw new System.NullReferenceException($"There is no IMovementMode attached to {gameObject.name}");
        }

        PreviousStamina = CurrentStamina = Config.MaxStamina;
        PreviousHealth = CurrentHealth = Config.MaxHealth;
    }

    // Start is called before the first frame update
    protected virtual void Start()
    {
        OnStaminaChanged.Invoke(CurrentStamina, Config.MaxStamina);
        OnHealthChanged.Invoke(CurrentHealth, Config.MaxHealth);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        if (DEBUG_ToggleLookLock)
        {
            DEBUG_ToggleLookLock = false;
            State.IsLookingLocked = !State.IsLookingLocked;
        }
        if (DEBUG_ToggleMovementLock)
        {
            DEBUG_ToggleMovementLock = false;
            State.IsMovementLocked = !State.IsMovementLocked;
        }

        State.Tick(Time.deltaTime);

        UpdateHealth();
        UpdateStamina();

        if (PreviousStamina != CurrentStamina)
        {
            PreviousStamina = CurrentStamina;
            OnStaminaChanged.Invoke(CurrentStamina, Config.MaxStamina);
        }

        if (PreviousHealth != CurrentHealth)
        {
            PreviousHealth = CurrentHealth;
            OnHealthChanged?.Invoke(CurrentHealth, Config.MaxHealth);
        }
    }

    protected void FixedUpdate()
    {
        if (DEBUG_OverrideMovement)
            State.Input_Move = DEBUG_MovementInput;

        // movement locked?
        if (State.IsMovementLocked)
            State.Input_Move = Vector2.zero;

        MovementMode.FixedUpdate_PreGroundedCheck();

        bool wasGrounded = State.IsGrounded;

        RaycastHit groundCheckResult = MovementMode.FixedUpdate_GroundedCheck();

        if (State.IsGrounded)
        {
            // check for a surface effect
            SurfaceEffectSource surfaceEffectSource = null;
            if (groundCheckResult.collider.gameObject.TryGetComponent<SurfaceEffectSource>(out surfaceEffectSource))
                SetSurfaceEffectSource(surfaceEffectSource);
            else
                SetSurfaceEffectSource(null);

            UpdateSurfaceEffects();

            // have we returned to the ground
            if (!wasGrounded)
                MovementMode.FixedUpdate_OnBecameGrounded();
        }
        else
            SetSurfaceEffectSource(null);

        MovementMode.FixedUpdate_TickMovement(groundCheckResult);

        State.LastRequestedVelocity = State.LinkedRB.velocity;
    }

    protected virtual void LateUpdate()
    {
        MovementMode.LateUpdate_Tick();

        float currentHeight = State.CurrentHeight;
        if (PreviousHeight != currentHeight)
        {
            State.LinkedCollider.height = currentHeight;
            State.LinkedCollider.center = Vector3.up * (currentHeight * 0.5f);
            PreviousHeight = currentHeight;
        }
    }

    public void AddParameterEffector(IParameterEffector newEffector)
    {
        State.AddParameterEffector(newEffector);
    }

    public void OnPerformHeal(GameObject source, float amount)
    {
        CurrentHealth = Mathf.Min(CurrentHealth + amount, Config.MaxHealth);
    }

    public void OnTakeDamage(GameObject source, float amount)
    {
        OnTookDamage.Invoke(amount);

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0f);
        HealthRecoveryDelayRemaining = Config.HealthRecoveryDelay;

        // have we died?
        if (CurrentHealth <= 0f && PreviousHealth > 0f)
            OnPlayerDied.Invoke(this);
    }

    protected void UpdateHealth()
    {
        // do we have health to recover?
        if (CurrentHealth < Config.MaxHealth)
        {
            if (HealthRecoveryDelayRemaining > 0f)
                HealthRecoveryDelayRemaining -= Time.deltaTime;

            if (HealthRecoveryDelayRemaining <= 0f)
                CurrentHealth = Mathf.Min(CurrentHealth + Config.HealthRecoveryRate * Time.deltaTime,
                                          Config.MaxHealth);
        }
    }

    protected void UpdateStamina()
    {
        // if we're running consume stamina
        if (State.IsRunning && State.IsGrounded)
            ConsumeStamina(Config.StaminaCost_Running * Time.deltaTime);
        else if (CurrentStamina < Config.MaxStamina) // if we're able to recover
        {
            if (StaminaRecoveryDelayRemaining > 0f)
                StaminaRecoveryDelayRemaining -= Time.deltaTime;

            if (StaminaRecoveryDelayRemaining <= 0f)
                CurrentStamina = Mathf.Min(CurrentStamina + Config.StaminaRecoveryRate * Time.deltaTime,
                                           Config.MaxStamina);
        }
    }

    public void ConsumeStamina(float amount)
    {
        CurrentStamina = Mathf.Max(CurrentStamina - amount, 0f);
        StaminaRecoveryDelayRemaining = Config.StaminaRecoveryDelay;
    }

    public void SetMovementLock(bool locked)
    {
        State.IsMovementLocked = locked;
    }

    public void SetLookLock(bool locked)
    {
        State.IsLookingLocked = locked;
    }

    void UpdateSurfaceEffects()
    {
        // no surface effect
        if (State.CurrentSurfaceSource == null)
            return;

        // time to expire the surface effect?
        if (CurrentSurfaceLastTickTime + State.CurrentSurfaceSource.PersistenceTime < Time.time)
        {
            State.CurrentSurfaceSource = null;
            return;
        }
    }

    void SetSurfaceEffectSource(SurfaceEffectSource newSource)
    {
        // changing to a new effect?
        if (newSource != null && newSource != State.CurrentSurfaceSource)
        {
            State.CurrentSurfaceSource = newSource;
            CurrentSurfaceLastTickTime = Time.time;
        } // on same source?
        else if (newSource != null && newSource == State.CurrentSurfaceSource)
        {
            CurrentSurfaceLastTickTime = Time.time;
        }
    }

    public void SwitchMovementMode<T>() where T : IMovementMode
    {
        MovementMode = GetComponent<T>();

        MovementMode.Initialise(Config, this, State);
    }

    public void OnHitGround(Vector3 location, float impactSpeed)
    {
        if (State.TimeInAir >= Config.MinAirTimeForLandedSound)
        {
            OnPlayImpactGroundSound.Invoke(location);
        }

        if (impactSpeed >= Config.MinFallSpeedToTakeDamage)
        {
            float speedProportion = Mathf.InverseLerp(Config.MinFallSpeedToTakeDamage,
                                                        Config.FallSpeedForMaximumDamage,
                                                        impactSpeed);

            float damagePercentage = Mathf.Lerp(Config.MinimumFallDamagePercentage,
                                                Config.MaximumFallDamagePercentage,
                                                speedProportion);

            float actualDamageToApply = Config.MaxHealth * damagePercentage;
            OnTakeDamage(null, actualDamageToApply);
        }
    }
}
