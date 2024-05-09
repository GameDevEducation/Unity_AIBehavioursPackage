using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    public class GameDebugger_UIOverlay : MonoBehaviour, IGameDebuggerUI
    {
        [SerializeField] UnityEvent<string> OnPopulateUI_SelectedObjectName = new();
        [SerializeField] UnityEvent<string> OnPopulateUI_DebugText = new();
        IGameDebugger LinkedDebugger;

        void Start()
        {
            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                LinkedDebugger = InDebugger as IGameDebugger;
                LinkedDebugger.RegisterUI(this);

                RefreshUI();
            });
        }

        void OnDestroy()
        {
            if (LinkedDebugger == null)
                return;

            LinkedDebugger.UnregisterUI(this);
            LinkedDebugger = null;
        }

        void Update()
        {
            if (LinkedDebugger == null)
                return;

            RefreshUI();
        }

        public void UI_OnSelectPrevious()
        {
            if (LinkedDebugger == null)
                return;

            LinkedDebugger.SelectPreviousSource();

            RefreshUI();
        }

        public void UI_OnSelectNext()
        {
            if (LinkedDebugger == null)
                return;

            LinkedDebugger.SelectNextSource();

            RefreshUI();
        }

        void RefreshUI()
        {
            if (LinkedDebugger.CurrentSource == null)
            {
                OnPopulateUI_SelectedObjectName.Invoke("No Sources");
                OnPopulateUI_DebugText.Invoke("");
                return;
            }

            OnPopulateUI_SelectedObjectName.Invoke(LinkedDebugger.CurrentSource.DebugDisplayName);

            OnPopulateUI_DebugText.Invoke(LinkedDebugger.DebugTextForCurrentSource);
        }

        public void OnSourceListModified()
        {
            RefreshUI();
        }

        public void OnDebugOutputUpdated()
        {
        }
    }
}