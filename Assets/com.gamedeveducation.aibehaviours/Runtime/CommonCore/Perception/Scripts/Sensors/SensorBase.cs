using System.Collections.Generic;

namespace CommonCore
{
    public abstract class SensorBase : ISensor
    {
        public virtual void RegisterListener(IPerceptionListener InListener, SensorConfigBase InSensorConfig) { }
        public virtual void DeregisterListener(IPerceptionListener InListener) { }

        public virtual void RegisterPerceivable(IPerceivable InPerceivable) { }
        public virtual void DeregisterPerceivable(IPerceivable InPerceivable) { }

        public virtual void Tick(float InDeltaTime, List<ListenerEntry> InListenerEntries, List<IPerceivable> InPerceivables, IPerceptionManager InManager) { }

        public virtual bool EvaluateInjectedDetection(IPerceptionListener InListener, SensorConfigBase InSensorConfig, IPerceivable InPerceivable, float InStrength)
        {
            return InListener.CanDetect(InPerceivable);
        }
    }
}
