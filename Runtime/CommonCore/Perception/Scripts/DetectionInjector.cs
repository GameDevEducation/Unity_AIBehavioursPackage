using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [RequireComponent(typeof(Perceivable))]
    public class DetectionInjector : MonoBehaviour
    {
        [SerializeField] SerializableType<SensorBase> TargetSensor;
        Perceivable Owner;

        private void Awake()
        {
            Owner = GetComponent<Perceivable>();
        }

        public void TriggerInjection(float InStrength)
        {
            Owner.ManuallyInjectDetection(TargetSensor.Type, InStrength);
        }
    }
}
