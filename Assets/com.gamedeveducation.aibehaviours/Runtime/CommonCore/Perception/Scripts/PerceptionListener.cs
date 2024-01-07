using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    public class PerceptionListener : MonoBehaviour, IPerceptionListener
    {
        [SerializeField] protected Transform SensorOrigin;
        [SerializeField] protected SensorConfigBase[] SupportedSensors;
        [SerializeField] protected List<EFactionRelationship> SupportedRelationships;

        [SerializeField, Range(0.0f, 1.0f)] protected float AcquisitionThreshold = 0.6f;
        [SerializeField, Range(0.0f, 1.0f)] protected float LossThreshold = 0.3f;

        [SerializeField] protected bool UpdateBlackboard = true;
        [SerializeField] protected bool SendEvents = false;
        [SerializeField] protected UnityEvent<IPerceivable, float> OnFocusChanged = new();

        public Vector3 SensorLocation => SensorOrigin.position;

        public Vector3 SensorFacing => SensorOrigin.forward;

        public GameObject Owner => gameObject;

        public IFaction Faction { get; protected set; }

        protected IPerceptionManager LinkedPerceptionManager = null;
        protected Blackboard<FastName> LinkedBlackboard = null;

        protected IPerceivable PreviousBestPerceivable = null;
        protected float PreviousBestDetection = float.MinValue;
        protected IPerceivable CurrentBestPerceivable = null;
        protected float CurrentBestDetection = float.MinValue;

        protected void Awake()
        {
            ServiceLocator.AsyncLocateService<IFaction>((ILocatableService InService) =>
            {
                Faction = InService as IFaction;
            }, gameObject, EServiceSearchMode.LocalOnly);

            ServiceLocator.AsyncLocateService<IPerceptionManager>((ILocatableService InService) =>
            {
                LinkedPerceptionManager = InService as IPerceptionManager;

                foreach (var Config in SupportedSensors)
                {
                    LinkedPerceptionManager.RegisterListener(this, Config);
                }
            });

            if (UpdateBlackboard)
            {
                ServiceLocator.AsyncLocateService<Blackboard<FastName>>((ILocatableService InService) =>
                {
                    LinkedBlackboard = InService as Blackboard<FastName>;

                    LinkedBlackboard.Set(CommonCore.Names.Awareness_PreviousBestTarget, (GameObject)null);
                    LinkedBlackboard.Set(CommonCore.Names.Awareness_BestTarget, (GameObject)null);
                }, gameObject, EServiceSearchMode.LocalOnly);
            }
        }

        protected void OnDestroy()
        {
            if (LinkedPerceptionManager != null)
            {
                LinkedPerceptionManager.DeregisterListener(this);
            }
        }

        public bool CanDetect(IPerceivable InPerceivable)
        {
            if (Owner == InPerceivable.Owner)
                return false;

            if ((Faction == null) || (InPerceivable.Faction == null))
                return false;

            if ((SupportedRelationships == null) || (SupportedRelationships.Count == 0))
                return true;

            return SupportedRelationships.Contains(Faction.GetRelationshipTo(InPerceivable.Faction));
        }

        public void OnNotifyBestPerceivable(IPerceivable InPerceivable, float InDetectionStrength, float InLastDetectionTime, Vector3 InLastDetectionLocation)
        {
            // scenario 1 - we have a current best and the new perceivable is different
            if ((CurrentBestPerceivable != null) && (CurrentBestPerceivable != InPerceivable))
            {
                CurrentBestDetection = LinkedPerceptionManager.GetDetectionStrength(this, CurrentBestPerceivable);

                // new detection is not stronger than current best
                if (CurrentBestDetection > InDetectionStrength)
                    return;

                // new detection is not strong enough to switch to
                if (InDetectionStrength < AcquisitionThreshold)
                    return;

                OnNotifyFocusChanged(InPerceivable, InDetectionStrength);
            } // scenario 2 - current best is the same
            else if (CurrentBestPerceivable == InPerceivable)
            {
                CurrentBestDetection = InDetectionStrength;

                // are we above the loss threshold?
                if (CurrentBestDetection >= LossThreshold)
                    return;

                OnNotifyFocusChanged(null, float.MinValue);
            } // scenario 3 - no current target
            else if (CurrentBestPerceivable == null)
            {
                // are we above the acquisition threshold
                if (InDetectionStrength < AcquisitionThreshold)
                    return;

                OnNotifyFocusChanged(InPerceivable, InDetectionStrength);
            }
        }

        public void OnNotifyLostPerceivable(IPerceivable InPerceivable)
        {
            if (InPerceivable == CurrentBestPerceivable)
                OnNotifyFocusChanged(null, float.MinValue);
        }

        protected void OnNotifyFocusChanged(IPerceivable InPerceivable, float InDetectionStrength)
        {
            PreviousBestPerceivable = CurrentBestPerceivable;
            PreviousBestDetection = CurrentBestDetection;

            CurrentBestPerceivable = InPerceivable;
            CurrentBestDetection = InDetectionStrength;

            if (UpdateBlackboard && (LinkedBlackboard != null))
            {
                LinkedBlackboard.Set(CommonCore.Names.Awareness_PreviousBestTarget,
                                     PreviousBestPerceivable != null ? PreviousBestPerceivable.Owner : (GameObject)null);

                LinkedBlackboard.Set(CommonCore.Names.Awareness_BestTarget,
                                     CurrentBestPerceivable != null ? CurrentBestPerceivable.Owner : (GameObject)null);
            }

            if (SendEvents)
                OnFocusChanged.Invoke(CurrentBestPerceivable, CurrentBestDetection);
        }
    }
}
