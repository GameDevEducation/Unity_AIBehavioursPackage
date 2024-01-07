using UnityEngine;

#if UNITY_EDITOR
#endif // UNITY_EDITOR

public abstract class BaseAI : MonoBehaviour
{
    [Header("Head Configuration")]
    [SerializeField] Transform HeadBoneTransform;
    [SerializeField] Vector3 HeadBoneToEyeOffset;

    [Header("Look At Configuration")]
    [SerializeField] float LookAtSpeed = 90.0f;
    [SerializeField] float MaxYawAngle = 75.0f;
    [SerializeField] float MinPitchAngle = -30.0f;
    [SerializeField] float MaxPitchAngle = 45.0f;

    [Header("Sensors")]
    [SerializeField] protected float _VisionConeAngle = 60f;
    [SerializeField] protected float _VisionConeRange = 30f;
    [SerializeField] protected Color _VisionConeColour = new Color(1f, 0f, 0f, 0.25f);

    [SerializeField] protected float _HearingRange = 20f;
    [SerializeField] protected Color _HearingRangeColour = new Color(1f, 1f, 0f, 0.25f);

    [SerializeField] protected float _ProximityDetectionRange = 3f;
    [SerializeField] protected Color _ProximityRangeColour = new Color(1f, 1f, 1f, 0.25f);

    [Header("DEBUG OPTIONS")]
    [SerializeField] bool DEBUG_DrawLookDirection = true;
    [SerializeField] Transform DEBUG_LookTargetToSet;
    [SerializeField] bool DEBUG_UpdateLookTarget = false;

    Transform CurrentLookTarget;
    Transform PreviousLookTarget;

    float DesiredYawDelta = 0f;
    float DesiredPitchDelta = 0f;
    float CurrentYawDelta = 0f;
    float CurrentPitchDelta = 0f;

    protected virtual void Update()
    {
        if (DEBUG_UpdateLookTarget)
        {
            DEBUG_UpdateLookTarget = false;
            SetLookTarget(DEBUG_LookTargetToSet);
        }
    }

    protected virtual void LateUpdate()
    {
        if (CurrentLookTarget != null)
        {
            Vector3 localSpaceLookDirection = HeadBoneTransform.InverseTransformPoint(CurrentLookTarget.position) - HeadBoneToEyeOffset;

            float yawDelta = Mathf.Atan2(localSpaceLookDirection.x, localSpaceLookDirection.z) * Mathf.Rad2Deg;
            float pitchDelta = Mathf.Atan(localSpaceLookDirection.y / localSpaceLookDirection.z) * Mathf.Rad2Deg;

            bool yawValid = Mathf.Abs(yawDelta) <= MaxYawAngle;
            bool pitchValid = pitchDelta >= MinPitchAngle && pitchDelta <= MaxPitchAngle;

            // if the target is within look limits then set it as the desired pitch and yaw
            if (yawValid && pitchValid)
            {
                DesiredYawDelta = yawDelta;
                DesiredPitchDelta = pitchDelta;
            }
            else
            {
                if (yawValid && !pitchValid)
                {
                    DesiredYawDelta = yawDelta;
                    DesiredPitchDelta = 0;
                }
                else
                {
                    // return to looking forwards
                    DesiredPitchDelta = DesiredYawDelta = 0f;
                }
            }
        }
        else if (PreviousLookTarget != null)
        {
            // return to looking forwards
            DesiredPitchDelta = DesiredYawDelta = 0f;
        }

        if (!Mathf.Approximately(CurrentYawDelta, DesiredYawDelta) ||
            !Mathf.Approximately(CurrentPitchDelta, DesiredPitchDelta))
        {
            CurrentPitchDelta = Mathf.MoveTowardsAngle(CurrentPitchDelta, DesiredPitchDelta, LookAtSpeed * Time.deltaTime);
            CurrentYawDelta = Mathf.MoveTowardsAngle(CurrentYawDelta, DesiredYawDelta, LookAtSpeed * Time.deltaTime);
        }

        HeadBoneTransform.localRotation *= Quaternion.Euler(-CurrentPitchDelta, CurrentYawDelta, 0f);

        PreviousLookTarget = CurrentLookTarget;

        if (DEBUG_DrawLookDirection)
        {
            Vector3 worldSpaceEyePosition = HeadBoneTransform.TransformPoint(HeadBoneToEyeOffset);
            Debug.DrawLine(worldSpaceEyePosition, worldSpaceEyePosition + HeadBoneTransform.forward * 10f, Color.red);
        }
    }

    public void SetLookTarget(Transform newLookTarget)
    {
        CurrentLookTarget = newLookTarget;
    }
}