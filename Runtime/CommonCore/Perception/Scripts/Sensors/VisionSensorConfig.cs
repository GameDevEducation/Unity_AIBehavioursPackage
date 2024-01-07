using System;
using UnityEngine;

namespace CommonCore
{
    [CreateAssetMenu(menuName = "AI/Perception/Vision Sensor Configuration", fileName = "SensorConfig_Vision")]
    public class VisionSensorConfig : SensorConfigBase
    {
        [field: SerializeField] public LayerMask LayersToCheck { get; protected set; } = ~0;
        [field: SerializeField] public float VisionConeAngle { get; protected set; } = 60f;
        [field: SerializeField] public float VisionConeRange { get; protected set; } = 30f;

        [field: SerializeField] public AnimationCurve Sensitivity { get; protected set; }
        [field: SerializeField] public float DetectionMultiplier { get; protected set; } = 10.0f;

        float _CachedCosVisionConeAngle = float.MinValue;
        public float CosVisionConeAngle
        {
            get
            {
                if (_CachedCosVisionConeAngle == float.MinValue)
                    _CachedCosVisionConeAngle = Mathf.Cos(VisionConeAngle * Mathf.Deg2Rad);

                return _CachedCosVisionConeAngle;
            }
        }

        public override Type GetSupportedSensorType()
        {
            return typeof(VisionSensor);
        }
    }
}
