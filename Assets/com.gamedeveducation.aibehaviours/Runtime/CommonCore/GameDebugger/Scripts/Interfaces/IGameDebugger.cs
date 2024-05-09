using System.Collections.Generic;

namespace CommonCore
{
    public interface IGameDebugger : ILocatableService
    {
        IDebuggableObject CurrentSource { get; }
        string DebugTextForCurrentSource { get; }
        string DebugTextForOtherSources { get; }
        int SourceCount { get; }
        List<string> SourceNames { get; }

        void SelectPreviousSource();
        void SelectNextSource();
        void SelectSourceByIndex(int InIndex);

        void RegisterUI(IGameDebuggerUI InDebuggerUI);
        void UnregisterUI(IGameDebuggerUI InDebuggerUI);

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