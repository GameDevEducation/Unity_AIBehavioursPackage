using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    public class GameDebugger : MonoBehaviourSingleton<GameDebugger>, IGameDebugger
    {
        [SerializeField] string IndentString = "  ";
        [SerializeField] UnityEvent<string> OnPopulateUI_SelectedObjectName = new();
        [SerializeField] UnityEvent<string> OnPopulateUI_DebugText = new();

        List<IDebuggableObject> Sources = new();

        int CurrentSourceIndex = -1;
        IDebuggableObject CurrentSource => ((CurrentSourceIndex < 0) || (CurrentSourceIndex >= Sources.Count)) ? null : Sources[CurrentSourceIndex];

        System.Text.StringBuilder DebugTextBuilder = new();
        int IndentLevel = 0;

        void Start()
        {
            RefreshUI();
        }

        void Update()
        {
            RefreshUI();
        }

        public void UI_OnSelectPrevious()
        {
            if (Sources.Count == 0)
                return;

            CurrentSourceIndex = (CurrentSourceIndex - 1 + Sources.Count) % Sources.Count;

            RefreshUI();
        }

        public void UI_OnSelectNext()
        {
            if (Sources.Count == 0)
                return;

            CurrentSourceIndex = (CurrentSourceIndex + 1) % Sources.Count;

            RefreshUI();
        }

        void RefreshUI()
        {
            // remove any nulls
            for (int Index = Sources.Count - 1; Index >= 0; Index--)
            {
                if (Sources[Index] == null)
                {
                    Sources.RemoveAt(Index);

                    if (Index == CurrentSourceIndex)
                        CurrentSourceIndex = -1;
                }
            }

            if ((CurrentSource == null) && (Sources.Count == 0))
                CurrentSourceIndex = 0;

            if (CurrentSource == null)
            {
                OnPopulateUI_SelectedObjectName.Invoke("No Sources");
                OnPopulateUI_DebugText.Invoke("");
                return;
            }

            OnPopulateUI_SelectedObjectName.Invoke(CurrentSource.DebugDisplayName);

            // gather the debug data
            var PrimarySource = CurrentSource;
            DebugTextBuilder.Clear();
            foreach (var Source in Sources)
            {
                Source.GatherDebugData(this, Source == PrimarySource);
            }

            OnPopulateUI_DebugText.Invoke(DebugTextBuilder.ToString());
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

        public static void AddSource(IDebuggableObject InObject)
        {
            if (Instance == null)
                return;

            Instance.RegisterSource(InObject);
        }

        public void RegisterSource(IDebuggableObject InObject)
        {
            if (Sources.Contains(InObject))
                return;

            Sources.Add(InObject);

            if (CurrentSource == null)
            {
                CurrentSourceIndex = 0;
                RefreshUI();
            }
        }

        public static void RemoveSource(IDebuggableObject InObject)
        {
            if (Instance == null)
                return;

            Instance.UnregisterSource(InObject);
        }

        public void UnregisterSource(IDebuggableObject InObject)
        {
            if (CurrentSource == InObject)
                CurrentSourceIndex = -1;

            Sources.Remove(InObject);

            if (CurrentSource == null)
            {
                if (Sources.Count > 0)
                    CurrentSourceIndex = 0;

                RefreshUI();
            }
        }
    }
}
