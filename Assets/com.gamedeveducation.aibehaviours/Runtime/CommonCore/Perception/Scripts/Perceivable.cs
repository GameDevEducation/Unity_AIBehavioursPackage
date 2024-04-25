using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [AddComponentMenu("AI/Perception/Perceivable")]
    public class Perceivable : MonoBehaviour, IPerceivable
    {
        protected enum EVelocitySource
        {
            Calculated2D,
            Calculated3D
        }

        [SerializeField] protected Vector3 PerceptionOffset = Vector3.zero;
        [SerializeField] protected List<SerializableType<SensorBase>> DetectableBy;
        [SerializeField] protected EVelocitySource VelocitySource = EVelocitySource.Calculated2D;

        public Vector3 Position => transform.position + PerceptionOffset;

        public Vector3 Velocity { get; protected set; } = Vector3.zero;

        public bool bIsMoving => !Mathf.Approximately(Velocity.sqrMagnitude, 0.0f);

        public GameObject Owner => gameObject;

        public IFaction Faction { get; protected set; }

        IPerceptionManager LinkedPerceptionManager = null;
        Vector3 LastPosition;

        protected void Start()
        {
            LastPosition = transform.position;

            ServiceLocator.AsyncLocateService<IFaction>((ILocatableService InService) =>
            {
                Faction = InService as IFaction;
            }, gameObject, EServiceSearchMode.LocalOnly);

            ServiceLocator.AsyncLocateService<IPerceptionManager>((ILocatableService InService) =>
            {
                LinkedPerceptionManager = InService as IPerceptionManager;

                LinkedPerceptionManager.RegisterPerceivable(this);
            });
        }

        protected void OnDestroy()
        {
            if (LinkedPerceptionManager != null)
            {
                LinkedPerceptionManager.DeregisterPerceivable(this);
            }
        }

        public bool IsPerceivableBy(Type InSensorType)
        {
            if ((DetectableBy == null) || (DetectableBy.Count == 0))
                return true;

            foreach (var SupportedType in DetectableBy)
            {
                if (InSensorType == SupportedType.Type)
                    return true;
            }

            return false;
        }

        protected void FixedUpdate()
        {
            if ((VelocitySource == EVelocitySource.Calculated2D) || (VelocitySource == EVelocitySource.Calculated3D))
            {
                Vector3 CurrentPosition = transform.position;

                if (VelocitySource == EVelocitySource.Calculated2D)
                    CurrentPosition.y = 0f;

                Velocity = (CurrentPosition - LastPosition) / Time.deltaTime;

                LastPosition = CurrentPosition;
            }
        }

        public void ManuallyInjectDetection(System.Type InSensorType, float InStrength)
        {
            LinkedPerceptionManager.InjectDetection(this, InSensorType, InStrength);
        }
    }
}
