using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [CreateAssetMenu(menuName = "AI/Perception/Proximity Sensor Configuration", fileName = "SensorConfig_Proximity")]
    public class ProximitySensorConfig : SensorConfigBase
    {
        [field: SerializeField] public float ThresholdDistance { get; protected set; } = 3.0f;

        [field: SerializeField] public AnimationCurve Sensitivity { get; protected set; }
        [field: SerializeField] public float DetectionMultiplier { get; protected set; } = 0.1f;

        public override Type GetSupportedSensorType()
        {
            return typeof(ProximitySensor);
        }
    }
}
