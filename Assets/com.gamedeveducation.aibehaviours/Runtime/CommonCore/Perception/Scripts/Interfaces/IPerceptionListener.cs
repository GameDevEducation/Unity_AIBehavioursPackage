using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IPerceptionListener
    {
        Vector3 SensorLocation { get; }
        Vector3 SensorFacing { get; }
        GameObject Owner { get; }
        IFaction Faction { get; }

        bool CanDetect(IPerceivable InPerceivable);

        void OnNotifyBestPerceivable(IPerceivable InPerceivable, float InDetectionStrength, 
                                     float InLastDetectionTime, Vector3 InLastDetectionLocation);
        void OnNotifyLostPerceivable(IPerceivable InPerceivable);
    }
}
