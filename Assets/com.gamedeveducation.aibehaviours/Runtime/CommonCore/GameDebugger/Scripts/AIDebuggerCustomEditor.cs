using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Events;


// notes: derived from  https://docs.unity3d.com/Manual/UIE-HowTo-CreateEditorWindow.html

namespace CommonCore
{
    public class AIDebuggerCustomEditor : EditorWindow, IGameDebugger
    {
        [SerializeField] string IndentString = "  ";
        [SerializeField] UnityEvent<string> OnPopulateUI_SelectedObjectName = new();
        [SerializeField] UnityEvent<string> OnPopulateUI_DebugText = new();
        
        [SerializeField] private int m_SelectedIndex = -1;
        private VisualElement m_RightPane;

        
        List<IDebuggableObject> Sources = new();  // AI Agents available in scene
        
        // currently selected AI debugging data
        int CurrentSourceIndex = -1;
        IDebuggableObject CurrentSource => ((CurrentSourceIndex < 0) || (CurrentSourceIndex >= Sources.Count)) ? null : Sources[CurrentSourceIndex];

        System.Text.StringBuilder DebugTextBuilder = new();
        int IndentLevel = 0;
        
        
        public static AIDebuggerCustomEditor Instance
        {
            get { return GetWindow< AIDebuggerCustomEditor >(); }
        }
        
        
        [MenuItem("Tools/AI Debugger")]
        public static void ShowMyEditor()
        {
            // This method is called when the user selects the menu item in the Editor
            EditorWindow wnd = GetWindow<AIDebuggerCustomEditor>();
            wnd.titleContent = new GUIContent("AI Debugger");
        }


        void Update()
        {
            RefreshUI();
        }
        
        
        public void CreateGUI()
        {
            // Create a two-pane view with the left pane being fixed width
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            
            // Add the panel to the visual tree by adding it as a child to the root element
            rootVisualElement.Add(splitView);
            
            // A TwoPaneSplitView always needs exactly two child elements
            var leftPane = new ListView();
            splitView.Add(leftPane);
            m_RightPane = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            splitView.Add(m_RightPane);
            
            // Initialize the list view with all the AI-Agent names
            leftPane.makeItem = () => new Label();
            leftPane.bindItem = (item, index) => { (item as Label).text = Sources[index].DebugDisplayName; };
            leftPane.itemsSource = Sources;
            
            // React to the user's selection
            //leftPane.selectionChanged += OnAISourceSelectionChange(leftPane.selectedIndex);
            leftPane.selectedIndicesChanged += OnSelectedIndicesChange;
            
            // Restore the selection index from before the hot reload
            leftPane.selectedIndex = m_SelectedIndex;
            
            // Store the selection index when the selection changes
            leftPane.selectionChanged += (items) => { m_SelectedIndex = leftPane.selectedIndex; };
            
        }
        
        void RefreshUI()
        {
            // Clear all previous content from the pane
            m_RightPane.Clear();
            
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
            
            var debugInformationLabel = new Label();
            debugInformationLabel.text = DebugTextBuilder.ToString();
            // Add the label control to the right-hand pane
            m_RightPane.Add(debugInformationLabel);
        }


        
        private void OnSelectedIndicesChange(IEnumerable<int> selectedItemIndex)
        {
            CurrentSourceIndex = selectedItemIndex.First();
            // regenerate editor panel label with debug text contained within
            RefreshUI();
        }            
            
            
        // private void OnAISourceSelectionChange(IEnumerable<object> selectedItems)
        // {
        //     selectedItems.ind
        //     // regenerate editor panel label with debug text contained within
        //     RefreshUI();
        // }

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

        public static void  AddSource(IDebuggableObject InObject)
        {            
            // todo:  code review needed.  look into how instancing of editor windows works
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
            // todo:  code review needed.  look into how instancing of editor windows works
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
