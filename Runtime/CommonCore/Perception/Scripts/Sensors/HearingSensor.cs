using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class HearingSensor : SensorBase
    {
        public override void Tick(float InDeltaTime, List<ListenerEntry> InListenerEntries, List<IPerceivable> InPerceivables, IPerceptionManager InManager)
        {
            base.Tick(InDeltaTime, InListenerEntries, InPerceivables, InManager);

            foreach (var Entry in InListenerEntries)
            {
                var Listener = Entry.Listener;
                var Config = Entry.Configuration as HearingSensorConfig;

                foreach (var Perceivable in InPerceivables)
                {
                    // skip if stationary
                    if (!Perceivable.bIsMoving)
                        continue;

                    // moving too slow
                    if (Perceivable.Velocity.sqrMagnitude < (Config.MinSpeedToHear * Config.MinSpeedToHear))
                        continue;

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
            return RunQuery(Time.deltaTime, InListener, InPerceivable, InSensorConfig as HearingSensorConfig, out Strength) && (Strength > 0);
        }

        protected virtual bool RunQuery(float InDeltaTime, IPerceptionListener InListener, IPerceivable InPerceivable, HearingSensorConfig InConfig, out float Strength)
        {
            float DistanceSq = (InListener.SensorLocation - InPerceivable.Position).sqrMagnitude;

            // out of range
            if (DistanceSq > (InConfig.SoundFallOffEnd * InConfig.SoundFallOffEnd))
            {
                Strength = float.MinValue;
                return false;
            }

            // in full detection range?
            if (DistanceSq <= (InConfig.SoundFallOffStart * InConfig.SoundFallOffStart)) 
                Strength = 1.0f;
            else // we are in the partial detection range
            {
                float DistanceFactor = Mathf.InverseLerp(InConfig.SoundFallOffStart, InConfig.SoundFallOffEnd, Mathf.Sqrt(DistanceSq));
                Strength = InConfig.Sensitivity.Evaluate(DistanceFactor);
            }

            Strength *= InConfig.DetectionMultiplier * InDeltaTime;
            return true;
        }
    }
}
