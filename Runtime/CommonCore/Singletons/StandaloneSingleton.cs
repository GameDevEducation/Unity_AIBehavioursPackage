using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

namespace CommonCore
{
    public abstract class StandaloneSingleton<T> : StandaloneSingleton where T : class, new()
    {
        static T _Instance = null;
        static bool _bInitialising = false;
        static readonly object _InstanceLock = new object();

        public static T Instance
        {
            get
            {
                lock (_InstanceLock)
                {
                    // instance already found?
                    if (_Instance != null)
                        return _Instance;

                    _bInitialising = true;

                    // create a new instance of T
                    _Instance = new T();
                    (_Instance as StandaloneSingleton).Initialise();

                    _bInitialising = false;
                    return _Instance;
                }
            }
        }

        static void ConstructIfNeeded(StandaloneSingleton<T> InInstance)
        {
            lock (_InstanceLock)
            {
                // only construct if the instance is null and is not being initialised
                if (_Instance == null && !_bInitialising)
                {
                    _Instance = InInstance as T;
                }
                else if (_Instance != null && !_bInitialising)
                {
                    Debug.LogError($"Found duplicate {typeof(T)}");
                }
            }
        }

        internal override void Initialise()
        {
#if UNITY_EDITOR
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif // UNITY_EDITOR

            OnInitialise();
        }

        protected virtual void OnInitialise() { }

#if UNITY_EDITOR
        protected virtual void OnPlayInEditorStopped() { }

        void OnPlayModeChanged(PlayModeStateChange InChange)
        {
            if ((InChange == PlayModeStateChange.ExitingPlayMode) && (_Instance != null))
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
                OnPlayInEditorStopped();
                _Instance = null;
            }
        }
#endif // UNITY_EDITOR
    }

    public abstract class StandaloneSingleton
    {
        internal abstract void Initialise();
        public virtual void OnBootstrapped() { }
    }
}
