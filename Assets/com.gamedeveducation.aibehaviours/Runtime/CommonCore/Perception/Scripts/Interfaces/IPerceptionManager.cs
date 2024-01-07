namespace CommonCore
{
    public interface IPerceptionManager : ILocatableService
    {
        void RegisterListener(IPerceptionListener InListener, SensorConfigBase InSensorConfig);
        void DeregisterListener(IPerceptionListener InListener);

        void RegisterPerceivable(IPerceivable InPerceivable);
        void DeregisterPerceivable(IPerceivable InPerceivable);

        float GetDetectionStrength(IPerceptionListener InListener, IPerceivable InPerceivable);

        void ReportDetection(IPerceptionListener InListener, IPerceivable InPerceivable, ISensor InSensor, float InStrength);
        void InjectDetection(IPerceivable InPerceivable, System.Type InSensorType, float InStrength);
    }
}
