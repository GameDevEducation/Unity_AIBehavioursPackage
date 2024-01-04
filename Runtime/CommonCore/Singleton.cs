using UnityEngine;

// This is heavily based on the implementation here:
// https://gamedev.stackexchange.com/a/151547 - Cosmic Giant

namespace CommonCore
{
    public abstract class Singleton<T> : Singleton where T : MonoBehaviour
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
                    // do nothing if currently quitting
                    if (bIsQuitting)
                        return null;

                    // instance already found?
                    if (_Instance != null)
                        return _Instance;

                    _bInitialising = true;

                    // search for any in-scene instance of T
                    var AllInstances = FindObjectsByType<T>(FindObjectsSortMode.None);

                    // found exactly one?
                    if (AllInstances.Length == 1)
                    {
                        _Instance = AllInstances[0];
                    } // found none?
                    else if (AllInstances.Length == 0)
                    {
                        _Instance = new GameObject($"Singleton<{typeof(T)}>").AddComponent<T>();
                    } // multiple found?
                    else
                    {
                        _Instance = AllInstances[0];

                        // destroy the duplicates
                        for (int Index = 1; Index < AllInstances.Length; ++Index)
                        {
                            Debug.LogError($"Destroying duplicate {typeof(T)} on {AllInstances[0].gameObject.name}");
                            Destroy(AllInstances[Index].gameObject);
                        }
                    }

                    _bInitialising = false;
                    return _Instance;
                }
            }
        }

        static void ConstructIfNeeded(Singleton<T> InInstance)
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
                    Debug.LogError($"Destroying duplicate {typeof(T)} on {InInstance.gameObject.name}");
                    Destroy(InInstance.gameObject);
                }
            }
        }

        private void Awake()
        {
            ConstructIfNeeded(this);

            OnAwake();
        }

        protected virtual void OnAwake()
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public abstract class Singleton : MonoBehaviour
    {
        protected static bool bIsQuitting { get; private set; } = false;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void OnBeforeSceneLoad()
        {
            bIsQuitting = false;
        }

        private void OnApplicationQuit()
        {
            bIsQuitting = true;
        }
    }
}
