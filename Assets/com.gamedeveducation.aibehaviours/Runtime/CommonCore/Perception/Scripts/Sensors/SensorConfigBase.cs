using UnityEngine;

namespace CommonCore
{
    public abstract class SensorConfigBase : ScriptableObject
    {
        public abstract System.Type GetSupportedSensorType();
    }
}
