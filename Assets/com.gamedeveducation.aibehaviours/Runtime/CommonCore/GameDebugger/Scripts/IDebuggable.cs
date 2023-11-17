using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IDebuggable
    {
        string DebugDisplayName { get; }

        void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected);
    }
}
