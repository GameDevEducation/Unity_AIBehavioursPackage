using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IPerceivable
    {
        Vector3 Position { get; }
        Vector3 Velocity { get; }
        bool bIsMoving { get; }
        GameObject Owner { get; }
        IFaction Faction { get; }

        bool IsPerceivableBy(System.Type InSensorType);
    }
}
