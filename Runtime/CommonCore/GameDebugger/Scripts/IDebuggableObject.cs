using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IDebuggableObject
    {
        string DebugDisplayName { get; }

        sealed void GatherDebugData(IGameDebugger InDebugger, bool bInIsSelected)
        {
            InDebugger.AddTextLine($"{(bInIsSelected ? "<b>" : "")}{DebugDisplayName}{(bInIsSelected ? "</b>" : "")}");

            InDebugger.PushIndent();

            GetDebuggableObjectContent(InDebugger, bInIsSelected);

            InDebugger.PopIndent();
        }

        void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected);
    }
}
