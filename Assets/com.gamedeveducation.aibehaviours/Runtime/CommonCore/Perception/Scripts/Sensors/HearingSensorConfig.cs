using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [CreateAssetMenu(menuName = "AI/Perception/Hearing Sensor Configuration", fileName = "SensorConfig_Hearing")]
    public class HearingSensorConfig : SensorConfigBase
    {
        [field: SerializeField] public float SoundFallOffStart { get; protected set; } = 5.0f;
        [field: SerializeField] public float SoundFallOffEnd { get; protected set; } = 30.0f;

        [field: SerializeField] public float MinSpeedToHear { get; protected set; } = 5.0f;

        [field: SerializeField] public AnimationCurve Sensitivity { get; protected set; }
        [field: SerializeField] public float DetectionMultiplier { get; protected set; } = 0.2f;

        public override Type GetSupportedSensorType()
        {
            return typeof(HearingSensor);
        }
    }
}
