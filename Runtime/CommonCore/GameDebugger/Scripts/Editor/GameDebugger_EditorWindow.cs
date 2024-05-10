using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CommonCore
{
    public class GameDebugger_EditorWindow : EditorWindow, IGameDebuggerUI
    {
        [SerializeField] int SelectedSourceIndex = -1;

        ListView SourcesList;
        VisualElement SourceInfo;
        IGameDebugger LinkedDebugger;

        [MenuItem("Tools/AI/Game Debugger")]
        public static void ShowGameDebugger()
        {
            EditorWindow Window = GetWindow<GameDebugger_EditorWindow>();
            Window.titleContent = new GUIContent("Game Debugger");
        }

        void CreateGUI()
        {
            // setup the main view and link it
            var MainDebuggerView = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            rootVisualElement.Add(MainDebuggerView);

            // setup our source list
            SourcesList = new ListView();
            MainDebuggerView.Add(SourcesList);
            SourceInfo = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            MainDebuggerView.Add(SourceInfo);

            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                LinkedDebugger = InDebugger as IGameDebugger;
                LinkedDebugger.RegisterUI(this);

                // link to the sources
                SourcesList.makeItem = () => new Label();
                SourcesList.bindItem = (InItem, InIndex) =>
                {
                    (InItem as Label).text = LinkedDebugger.SourceNames[InIndex];
                };
                SourcesList.itemsSource = LinkedDebugger.SourceNames;

                SourcesList.selectedIndicesChanged += OnSelectedIndicesChanged;

                SourcesList.selectedIndex = SelectedSourceIndex;

                RefreshUI();
            });
        }

        void OnSelectedIndicesChanged(IEnumerable<int> InSelectedIndices)
        {
            var Enumerator = InSelectedIndices.GetEnumerator();
            if (Enumerator.MoveNext())
                SelectedSourceIndex = Enumerator.Current;
            else
                SelectedSourceIndex = -1;

            if (LinkedDebugger != null)
                LinkedDebugger.SelectSourceByIndex(SelectedSourceIndex);

            RefreshUI();
        }

        public void OnSourceListModified()
        {
            SourcesList.RefreshItems();

            RefreshUI();
        }

        public void OnDebugOutputUpdated()
        {
            RefreshUI();
        }

        void RefreshUI()
        {
            SourceInfo.Clear();

            if (LinkedDebugger != null)
                SourceInfo.Add(new Label(LinkedDebugger.DebugTextForCurrentSource));
        }
    }
}
