using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IGameDebugger
    {
        void RegisterSource(IDebuggableObject InObject);
        void UnregisterSource(IDebuggableObject InObject);

        void AddSectionHeader(string InText);

        void AddText(string InText, bool bUseCurrentIndent = true);
        void AddTextLine(string InText, bool bUseCurrentIndent = true);
        void AddEndLine();

        void PushIndent();
        void PopIndent();
    }
}
