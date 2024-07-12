using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    public abstract class LookHandlerBase : MonoBehaviour, ILookHandler
    {
        [Header("Head Configuration")]
        [SerializeField] Transform HeadBoneTransform;
        [SerializeField] Vector3 HeadBoneToEyeOffset;

        [Header("Look At Configuration")]
        [SerializeField] float LookAtSpeed = 90.0f;
        [SerializeField] float MaxYawAngle = 75.0f;
        [SerializeField] float MinPitchAngle = -30.0f;
        [SerializeField] float MaxPitchAngle = 45.0f;

        [Header("DEBUG OPTIONS")]
        [SerializeField] bool DEBUG_DrawLookDirection = false;

        float DesiredYawDelta = 0f;
        float DesiredPitchDelta = 0f;
        float CurrentYawDelta = 0f;
        float CurrentPitchDelta = 0f;

        Transform Target_LookTransform = null;
        ILookTarget Target_LookInterface = null;
        Vector3? Target_LookPosition = null;

        protected bool HasLookTarget => (Target_LookTransform != null) ||
                                        (Target_LookInterface != null) ||
                                        (Target_LookPosition.HasValue);
        protected Vector3 LookAtPosition
        {
            get
            {
                if (Target_LookTransform != null)
                    return Target_LookTransform.position;
                else if (Target_LookInterface != null)
                    return Target_LookInterface.GetLocation();
                else if (Target_LookPosition.HasValue)
                    return Target_LookPosition.Value;

                return CommonCore.Constants.InvalidVector3Position;
            }
        }

        protected virtual void Awake()
        {
            ServiceLocator.RegisterService<ILookHandler>(this, gameObject);
        }

        protected virtual void LateUpdate()
        {
            if (HasLookTarget)
            {
                Vector3 DesiredLookAtPosition = LookAtPosition;

                Vector3 LocalSpaceLookDirection = HeadBoneTransform.InverseTransformPoint(DesiredLookAtPosition) - HeadBoneToEyeOffset;

                float YawDelta = Mathf.Atan2(LocalSpaceLookDirection.x, LocalSpaceLookDirection.z) * Mathf.Rad2Deg;
                float PitchDelta = Mathf.Atan(LocalSpaceLookDirection.y / LocalSpaceLookDirection.z) * Mathf.Rad2Deg;

                bool YawValid = Mathf.Abs(YawDelta) <= MaxYawAngle;
                bool PitchValid = PitchDelta >= MinPitchAngle && PitchDelta <= MaxPitchAngle;

                // if the target is within look limits then set it as the desired pitch and yaw
                if (YawValid && PitchValid)
                {
                    DesiredYawDelta = YawDelta;
                    DesiredPitchDelta = PitchDelta;
                }
                else
                {
                    if (YawValid && !PitchValid)
                    {
                        DesiredYawDelta = YawDelta;
                        DesiredPitchDelta = 0;
                    }
                    else
                    {
                        // return to looking forwards
                        DesiredPitchDelta = DesiredYawDelta = 0f;
                    }
                }
            }
            else
            {
                // return to looking forwards
                DesiredPitchDelta = DesiredYawDelta = 0f;
            }

            if (!Mathf.Approximately(CurrentYawDelta, DesiredYawDelta) ||
                !Mathf.Approximately(CurrentPitchDelta, DesiredPitchDelta))
            {
                CurrentPitchDelta = Mathf.MoveTowardsAngle(CurrentPitchDelta, DesiredPitchDelta, LookAtSpeed * Time.deltaTime);
                CurrentYawDelta = Mathf.MoveTowardsAngle(CurrentYawDelta, DesiredYawDelta, LookAtSpeed * Time.deltaTime);

                HeadBoneTransform.localRotation *= Quaternion.Euler(-CurrentPitchDelta, CurrentYawDelta, 0f);
            }

            if (DEBUG_DrawLookDirection)
            {
                Vector3 worldSpaceEyePosition = HeadBoneTransform.TransformPoint(HeadBoneToEyeOffset);
                Debug.DrawLine(worldSpaceEyePosition, worldSpaceEyePosition + HeadBoneTransform.forward * 10f, Color.red);
            }
        }

        public void ClearLookTarget()
        {
            Target_LookTransform = null;
            Target_LookInterface = null;
            Target_LookPosition = null;
        }

        public abstract void DetermineBestLookTarget(Blackboard<FastName> InBlackboard, out GameObject OutLookTargetGO, out Vector3 OutLookTargetPosition);

        public bool SetLookTarget(Transform InTransform)
        {
            if (InTransform == null)
                return false;

            Target_LookTransform = InTransform;
            Target_LookPosition = null;
            Target_LookInterface = null;

            ServiceLocator.AsyncLocateService<ILookTarget>((ILocatableService InFoundService) =>
            {
                Target_LookInterface = (ILookTarget)InFoundService;
                Target_LookTransform = null;
            }, InTransform.gameObject, EServiceSearchMode.LocalOnly);

            return true;
        }

        public bool SetLookTarget(Vector3 InPosition)
        {
            Target_LookTransform = null;
            Target_LookInterface = null;
            Target_LookPosition = InPosition;

            return true;
        }
    }
}
