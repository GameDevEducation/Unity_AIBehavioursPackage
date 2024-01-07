using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class PerceptionManager : MonoBehaviourSingleton<PerceptionManager>, IPerceptionManager
    {
        protected class DetectionData
        {
            public float DetectionStrength;
            public float LastDetectionTime;
            public Vector3 LastDetectionLocation;
        }

        [SerializeField] protected float DetectionDecayDelay = 0.1f;
        [SerializeField] protected float DetectionDecayRate = 0.1f;

        public float CurrentTime => Time.time;

        Dictionary<ISensor, List<ListenerEntry>> ActiveSensors;
        Dictionary<ISensor, List<IPerceivable>> ActivePerceivables;
        Dictionary<IPerceptionListener, Dictionary<IPerceivable, DetectionData>> AllDetectionData;

        protected override void OnAwake()
        {
            base.OnAwake();

            ActiveSensors = new();
            ActivePerceivables = new();
            AllDetectionData = new();

            ServiceLocator.RegisterService<IPerceptionManager>(this);
        }

        protected void Update()
        {
            // Stage 1: Tick any active sensors
            foreach (var SensorKVP in ActiveSensors)
            {
                var Sensor = SensorKVP.Key;
                var ListenerEntries = SensorKVP.Value;

                List<IPerceivable> Perceivables = null;
                if (!ActivePerceivables.TryGetValue(Sensor, out Perceivables))
                    continue;

                Sensor.Tick(Time.deltaTime, ListenerEntries, Perceivables, this);
            }

            float AgeThresholdToDecay = CurrentTime - DetectionDecayDelay;
            float DecayAmount = Time.deltaTime * DetectionDecayRate;

            // Stage 2: Decay any old detections
            // Stage 3: Identify the best detection
            List<IPerceivable> PerceivablesToRemove = new(ActivePerceivables.Count);
            List<IPerceptionListener> ListenersToRemove = new(AllDetectionData.Count);
            foreach (var DetectionKVP in AllDetectionData)
            {
                var Listener = DetectionKVP.Key;
                var DetectionEntries = DetectionKVP.Value;

                IPerceivable BestPerceivable = null;
                DetectionData BestDetection = null;

                foreach (var DetectionDataKVP in DetectionEntries)
                {
                    var Perceivable = DetectionDataKVP.Key;
                    var Data = DetectionDataKVP.Value;

                    // can we decay?
                    if (Data.LastDetectionTime <= AgeThresholdToDecay)
                    {
                        Data.DetectionStrength = Mathf.Clamp01(Data.DetectionStrength - DecayAmount);

                        if (Data.DetectionStrength <= 0)
                        {
                            PerceivablesToRemove.Add(Perceivable);
                            continue;
                        }
                    }

                    // new best detection
                    if ((BestPerceivable == null) || (Data.DetectionStrength > BestDetection.DetectionStrength))
                    {
                        BestPerceivable = Perceivable;
                        BestDetection = Data;
                    }
                }

                // need to notify listener about the target
                if (BestPerceivable != null)
                {
                    Listener.OnNotifyBestPerceivable(BestPerceivable,
                                                     BestDetection.DetectionStrength,
                                                     BestDetection.LastDetectionTime,
                                                     BestDetection.LastDetectionLocation);
                }

                // are there entries to remove?
                foreach (var Perceivable in PerceivablesToRemove)
                {
                    Listener.OnNotifyLostPerceivable(Perceivable);
                    DetectionEntries.Remove(Perceivable);
                }
                PerceivablesToRemove.Clear();

                // flag empty listeners for removal
                if (DetectionEntries.Count == 0)
                    ListenersToRemove.Add(Listener);
            }

            // cleanup any listeners
            foreach (var Listener in ListenersToRemove)
                AllDetectionData.Remove(Listener);
        }

        public void RegisterListener(IPerceptionListener InListener, SensorConfigBase InSensorConfig)
        {
            // is this sensor already present?
            foreach (var SensorKVP in ActiveSensors)
            {
                var Sensor = SensorKVP.Key;

                // sensor already is present
                if (Sensor.GetType() == InSensorConfig.GetSupportedSensorType())
                {
                    // is there a double registration?
                    foreach (var Entry in SensorKVP.Value)
                    {
                        if (Entry.Listener == InListener)
                        {
                            Debug.LogError($"{InListener.Owner} is attempting to register itself multiple times for {InSensorConfig}");
                            return;
                        }
                    }

                    SensorKVP.Value.Add(new ListenerEntry() { Listener = InListener, Configuration = InSensorConfig });
                    Sensor.RegisterListener(InListener, InSensorConfig);
                    return;
                }
            }

            // create the new sensor
            var NewSensor = (ISensor)System.Activator.CreateInstance(InSensorConfig.GetSupportedSensorType());

            ActiveSensors[NewSensor] = new List<ListenerEntry>() { new ListenerEntry() { Listener = InListener, Configuration = InSensorConfig } };
            NewSensor.RegisterListener(InListener, InSensorConfig);
        }

        public void DeregisterListener(IPerceptionListener InListener)
        {
            foreach (var SensorKVP in ActiveSensors)
            {
                var Sensor = SensorKVP.Key;
                var ListenerEntries = SensorKVP.Value;

                // remove any entries for this listener
                for (int Index = ListenerEntries.Count - 1; Index >= 0; Index--)
                {
                    var Entry = ListenerEntries[Index];

                    if (Entry.Listener == InListener)
                    {
                        Sensor.DeregisterListener(InListener);
                        ListenerEntries.RemoveAt(Index);
                        break;
                    }
                }
            }
        }

        public void RegisterPerceivable(IPerceivable InPerceivable)
        {
            foreach (var Sensor in ActiveSensors.Keys)
            {
                if (!InPerceivable.IsPerceivableBy(Sensor.GetType()))
                    continue;

                // if the list doesn't already exist then build it
                List<IPerceivable> Perceivables = null;
                if (!ActivePerceivables.TryGetValue(Sensor, out Perceivables))
                {
                    Perceivables = new();
                    ActivePerceivables[Sensor] = Perceivables;
                }

                // are we already in the list?
                if (Perceivables.Contains(InPerceivable))
                {
                    Debug.LogError($"Attempting to register {InPerceivable.Owner} multiple times for sensor {Sensor}");
                    continue;
                }

                Perceivables.Add(InPerceivable);
                Sensor.RegisterPerceivable(InPerceivable);
            }
        }

        public void DeregisterPerceivable(IPerceivable InPerceivable)
        {
            foreach (var PerceivableKVP in ActivePerceivables)
            {
                if (PerceivableKVP.Value.Remove(InPerceivable))
                    PerceivableKVP.Key.DeregisterPerceivable(InPerceivable);
            }
        }

        public float GetDetectionStrength(IPerceptionListener InListener, IPerceivable InPerceivable)
        {
            if ((InListener == null) || (InPerceivable == null))
                return float.MinValue;

            Dictionary<IPerceivable, DetectionData> DetectionEntries = null;
            if (AllDetectionData.TryGetValue(InListener, out DetectionEntries))
            {
                DetectionData Data = null;
                if (DetectionEntries.TryGetValue(InPerceivable, out Data))
                {
                    return Data.DetectionStrength;
                }
            }

            return float.MinValue;
        }

        public void InjectDetection(IPerceivable InPerceivable, Type InSensorType, float InStrength)
        {
            foreach (var SensorKVP in ActiveSensors)
            {
                var Sensor = SensorKVP.Key;

                // skip if the type doesn't match
                if (Sensor.GetType() != InSensorType)
                    continue;

                // allow each sensor to determine if we use the detection
                var ListenerEntries = SensorKVP.Value;
                foreach (var Entry in ListenerEntries)
                {
                    if (Sensor.EvaluateInjectedDetection(Entry.Listener, Entry.Configuration, InPerceivable, InStrength))
                        ReportDetection(Entry.Listener, InPerceivable, Sensor, InStrength);
                }
            }
        }

        public void ReportDetection(IPerceptionListener InListener, IPerceivable InPerceivable, ISensor InSensor, float InStrength)
        {
            Dictionary<IPerceivable, DetectionData> DetectionEntries = null;
            if (!AllDetectionData.TryGetValue(InListener, out DetectionEntries))
            {
                DetectionEntries = new();
                AllDetectionData[InListener] = DetectionEntries;
            }

            DetectionData Data = null;
            if (!DetectionEntries.TryGetValue(InPerceivable, out Data))
            {
                Data = new();
                DetectionEntries[InPerceivable] = Data;
            }

            Data.LastDetectionLocation = InPerceivable.Position;
            Data.LastDetectionTime = CurrentTime;
            Data.DetectionStrength = Mathf.Clamp01(Data.DetectionStrength + InStrength);
        }
    }

    public static class PerceptionManagerBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            if (PerceptionManager.Instance != null)
                PerceptionManager.Instance.OnBootstrapped();
        }
    }
}
