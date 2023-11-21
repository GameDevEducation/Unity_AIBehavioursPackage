using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
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
    [field: SerializeField] public bool DEBUG_ShowSensorBounds { get; protected set; } = true;

    Transform CurrentLookTarget;
    Transform PreviousLookTarget;

    float DesiredYawDelta = 0f;
    float DesiredPitchDelta = 0f;
    float CurrentYawDelta = 0f;
    float CurrentPitchDelta = 0f;

    public Vector3 EyeLocation => HeadBoneTransform.position;
    public Vector3 EyeDirection => HeadBoneTransform.forward;

    public float VisionConeAngle => _VisionConeAngle;
    public float VisionConeRange => _VisionConeRange;
    public Color VisionConeColour => _VisionConeColour;

    public float HearingRange => _HearingRange;
    public Color HearingRangeColour => _HearingRangeColour;

    public float ProximityDetectionRange => _ProximityDetectionRange;
    public Color ProximityDetectionColour => _ProximityRangeColour;

    public float CosVisionConeAngle { get; private set; } = 0f;

    protected AwarenessSystem Awareness;

    void Awake()
    {
        CosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);
        Awareness = GetComponent<AwarenessSystem>();
    }

    public void ReportCanSee(DetectableTarget seen)
    {
        Awareness.ReportCanSee(seen);
    }

    public void ReportCanHear(GameObject source, Vector3 location, EHeardSoundCategory category, float intensity)
    {
        Awareness.ReportCanHear(source, location, category, intensity);
    }

    public void ReportInProximity(DetectableTarget target)
    {
        Awareness.ReportInProximity(target);
    }

    public virtual void OnSuspicious()
    {
    }

    public virtual void OnDetected(GameObject target)
    {
    }

    public virtual void OnFullyDetected(GameObject target)
    {
    }

    public virtual void OnLostDetect(GameObject target)
    {
    }

    public virtual void OnLostSuspicion()
    {
    }

    public virtual void OnFullyLost()
    {
    }

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

#if UNITY_EDITOR
[CustomEditor(typeof(BaseAI))]
public class BaseAIEditor : Editor
{
    public void OnSceneGUI()
    {
        var ai = target as BaseAI;

        if (ai.DEBUG_ShowSensorBounds)
        {
            // draw the detectopm range
            Handles.color = ai.ProximityDetectionColour;
            Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.ProximityDetectionRange);

            // draw the hearing range
            Handles.color = ai.HearingRangeColour;
            Handles.DrawSolidDisc(ai.transform.position, Vector3.up, ai.HearingRange);

            // work out the start point of the vision cone
            Vector3 startPoint = Mathf.Cos(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.forward +
                                 Mathf.Sin(-ai.VisionConeAngle * Mathf.Deg2Rad) * ai.transform.right;

            // draw the vision cone
            Handles.color = ai.VisionConeColour;
            Handles.DrawSolidArc(ai.transform.position, Vector3.up, startPoint, ai.VisionConeAngle * 2f, ai.VisionConeRange);
        }
    }
}
#endif // UNITY_EDITOR