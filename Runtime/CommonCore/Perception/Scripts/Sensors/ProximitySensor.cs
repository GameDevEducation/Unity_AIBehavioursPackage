using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class ProximitySensor : SensorBase
    {
        public override void Tick(float InDeltaTime, List<ListenerEntry> InListenerEntries, List<IPerceivable> InPerceivables, IPerceptionManager InManager)
        {
            base.Tick(InDeltaTime, InListenerEntries, InPerceivables, InManager);

            foreach (var Entry in InListenerEntries)
            {
                var Listener = Entry.Listener;
                var Config = Entry.Configuration as ProximitySensorConfig;

                foreach (var Perceivable in InPerceivables)
                {
                    // is the listener not able to detect this perceivable?
                    if (!Listener.CanDetect(Perceivable))
                        continue;

                    float Strength = float.MinValue;
                    if (RunQuery(InDeltaTime, Listener, Perceivable, Config, out Strength) && (Strength > 0))
                        InManager.ReportDetection(Listener, Perceivable, this, Strength);
                }
            }
        }

        public override bool EvaluateInjectedDetection(IPerceptionListener InListener, SensorConfigBase InSensorConfig, IPerceivable InPerceivable, float InStrength)
        {
            if (!base.EvaluateInjectedDetection(InListener, InSensorConfig, InPerceivable, InStrength))
                return false;

            float Strength = float.MinValue;
            return RunQuery(Time.deltaTime, InListener, InPerceivable, InSensorConfig as ProximitySensorConfig, out Strength) && (Strength > 0);
        }

        protected virtual bool RunQuery(float InDeltaTime, IPerceptionListener InListener, IPerceivable InPerceivable, ProximitySensorConfig InConfig, out float Strength)
        {
            float DistanceSq = (InListener.SensorLocation - InPerceivable.Position).sqrMagnitude;

            // out of range
            if (DistanceSq > (InConfig.ThresholdDistance * InConfig.ThresholdDistance))
            {
                Strength = float.MinValue;
                return false;
            }

            float DistanceFactor = (DistanceSq > 0) ? (Mathf.Sqrt(DistanceSq) / InConfig.ThresholdDistance) : 0f;
            Strength = InConfig.Sensitivity.Evaluate(DistanceFactor) * InConfig.DetectionMultiplier * InDeltaTime;
            return true;
        }
    }
}
