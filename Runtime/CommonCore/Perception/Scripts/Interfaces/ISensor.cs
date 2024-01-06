using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CommonCore.PerceptionManager;

namespace CommonCore
{
    public class ListenerEntry
    {
        public IPerceptionListener Listener;
        public SensorConfigBase Configuration;
    }

    public interface ISensor
    {
        void RegisterListener(IPerceptionListener InListener, SensorConfigBase InSensorConfig);
        void DeregisterListener(IPerceptionListener InListener);

        void RegisterPerceivable(IPerceivable InPerceivable);
        void DeregisterPerceivable(IPerceivable InPerceivable);

        void Tick(float InDeltaTime, List<ListenerEntry> InListenerEntries, List<IPerceivable> InPerceivables, IPerceptionManager InManager);

        bool EvaluateInjectedDetection(IPerceptionListener InListener, SensorConfigBase InSensorConfig, IPerceivable InPerceivable, float InStrength);
    }
}
