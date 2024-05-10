using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class GameDebugger : MonoBehaviourSingleton<GameDebugger>, IGameDebugger
    {
        string IndentString = "  ";

        List<IDebuggableObject> Sources = new();

        int CurrentSourceIndex = -1;
        public IDebuggableObject CurrentSource => ((CurrentSourceIndex < 0) || (CurrentSourceIndex >= Sources.Count)) ? null : Sources[CurrentSourceIndex];
        public string DebugTextForCurrentSource { get; protected set; } = null;
        public string DebugTextForOtherSources { get; protected set; } = null;
        public int SourceCount => Sources != null ? Sources.Count : 0;
        public List<string> SourceNames { get; protected set; } = new();

        System.Text.StringBuilder DebugTextBuilder = new();
        int IndentLevel = 0;

        List<IGameDebuggerUI> LinkedUIs = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            ServiceLocator.RegisterService<IGameDebugger>(this);
        }

        void Update()
        {
            if (Sources.Count == 0)
            {
                DebugTextForCurrentSource = DebugTextForOtherSources = null;
                return;
            }

            if (CurrentSource == null)
                CurrentSourceIndex = 0;

            var PrimarySource = CurrentSource;

            // Refresh the text for the current source
            DebugTextBuilder.Clear();
            foreach (var Source in Sources)
            {
                if (Source != PrimarySource)
                    continue;

                Source.GatherDebugData(this, true);
            }
            DebugTextForCurrentSource = DebugTextBuilder.ToString();

            // refresh the text for inactive sources
            DebugTextBuilder.Clear();
            foreach (var Source in Sources)
            {
                if (Source == PrimarySource)
                    continue;

                Source.GatherDebugData(this, false);
            }
            DebugTextForOtherSources = DebugTextBuilder.ToString();

            // notify any linked UIs of the update
            foreach (var LinkedUI in LinkedUIs)
            {
                if (LinkedUI != null)
                    LinkedUI.OnDebugOutputUpdated();
            }
        }

        public void SelectPreviousSource()
        {
            if (Sources.Count == 0)
                return;

            CurrentSourceIndex = (CurrentSourceIndex - 1 + Sources.Count) % Sources.Count;
        }

        public void SelectNextSource()
        {
            if (Sources.Count == 0)
                return;

            CurrentSourceIndex = (CurrentSourceIndex + 1) % Sources.Count;
        }

        public void SelectSourceByIndex(int InIndex)
        {
            if (SourceCount == 0)
                return;

            CurrentSourceIndex = Mathf.Clamp(InIndex, 0, SourceCount - 1);
        }

        public void AddEndLine()
        {
            DebugTextBuilder.AppendLine();
        }

        public void AddSectionHeader(string InText)
        {
            AddTextLine($"<color=orange>{InText}</color>");
        }

        public void AddText(string InText, bool bUseCurrentIndent = true)
        {
            if (bUseCurrentIndent)
            {
                for (int Indent = 0; Indent < IndentLevel; Indent++)
                    DebugTextBuilder.Append(IndentString);
            }

            DebugTextBuilder.Append(InText);
        }

        public void AddTextLine(string InText, bool bUseCurrentIndent = true)
        {
            AddText(InText, bUseCurrentIndent);
            AddEndLine();
        }

        public void PopIndent()
        {
            IndentLevel = Mathf.Max(0, IndentLevel - 1);
        }

        public void PushIndent()
        {
            ++IndentLevel;
        }

        public void RegisterUI(IGameDebuggerUI InDebuggerUI)
        {
            if (LinkedUIs.Contains(InDebuggerUI))
                return;

            LinkedUIs.RemoveAll(InLinkedUI => InLinkedUI == null);

            LinkedUIs.Add(InDebuggerUI);
        }

        public void UnregisterUI(IGameDebuggerUI InDebuggerUI)
        {
            LinkedUIs.Remove(InDebuggerUI);
        }

        public void RegisterSource(IDebuggableObject InObject)
        {
            if (Sources.Contains(InObject))
                return;

            Sources.Add(InObject);
            SourceNames.Add(InObject.DebugDisplayName);

            if (CurrentSource == null)
                CurrentSourceIndex = 0;

            OnSourceListModified();
        }

        public void UnregisterSource(IDebuggableObject InObject)
        {
            if (CurrentSource == InObject)
                CurrentSourceIndex = -1;

            int Index = Sources.IndexOf(InObject);
            if (Index >= 0)
            {
                Sources.RemoveAt(Index);
                SourceNames.RemoveAt(Index);
            }

            if (CurrentSource == null)
            {
                if (Sources.Count > 0)
                    CurrentSourceIndex = 0;
            }

            OnSourceListModified();
        }

        void OnSourceListModified()
        {
            foreach (var LinkedUI in LinkedUIs)
            {
                if (LinkedUI == null)
                    continue;

                LinkedUI.OnSourceListModified();
            }
        }
    }
    public static class GameDebuggerBootstrapper
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void Initialize()
        {
            if (GameDebugger.Instance != null)
                GameDebugger.Instance.OnBootstrapped();
        }
    }
}