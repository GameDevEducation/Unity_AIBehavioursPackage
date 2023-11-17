using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EBlackboardKey
{
    Character_FocusObject,

    Household_ObjectsInUse,

    Memories_ShortTerm,
    Memories_LongTerm
}

public class Blackboard
{
    Dictionary<EBlackboardKey, int>         IntValues           = new Dictionary<EBlackboardKey, int>();
    Dictionary<EBlackboardKey, float>       FloatValues         = new Dictionary<EBlackboardKey, float>();
    Dictionary<EBlackboardKey, bool>        BoolValues          = new Dictionary<EBlackboardKey, bool>();
    Dictionary<EBlackboardKey, string>      StringValues        = new Dictionary<EBlackboardKey, string>();
    Dictionary<EBlackboardKey, Vector3>     Vector3Values       = new Dictionary<EBlackboardKey, Vector3>();
    Dictionary<EBlackboardKey, GameObject>  GameObjectValues    = new Dictionary<EBlackboardKey, GameObject>();
    Dictionary<EBlackboardKey, object>      GenericValues       = new Dictionary<EBlackboardKey, object>();

    public void SetGeneric<T>(EBlackboardKey key, T value)
    {
        GenericValues[key] = value;
    }

    public T GetGeneric<T>(EBlackboardKey key)
    {
        if (!GenericValues.ContainsKey(key))
            throw new System.ArgumentException($"Could not find value for {key} in GenericValues");

        return (T)GenericValues[key];
    }

    public bool TryGetGeneric<T>(EBlackboardKey key, out T value, T defaultValue)
    {
        if (GenericValues.ContainsKey(key))
        {
            value = (T)GenericValues[key];
            return true;
        }

        value = defaultValue;
        return false;
    }

    private T Get<T>(Dictionary<EBlackboardKey, T> keySet, EBlackboardKey key)
    {
        if (!keySet.ContainsKey(key))
            throw new System.ArgumentException($"Could not find value for {key} in {typeof(T).Name}Values");

        return keySet[key];
    }

    private bool TryGet<T>(Dictionary<EBlackboardKey, T> keySet, EBlackboardKey key, out T value, T defaultValue = default)
    {
        if (keySet.ContainsKey(key))
        {
            value = keySet[key];
            return true;
        }

        value = default;
        return false;
    }

    public void Set(EBlackboardKey key, int value)
    {
        IntValues[key] = value;
    }

    public int GetInt(EBlackboardKey key)
    {
        return Get(IntValues, key);
    }

    public bool TryGet(EBlackboardKey key, out int value, int defaultValue = 0)
    {
        return TryGet(IntValues, key, out value, defaultValue);
    }

    public void Set(EBlackboardKey key, float value)
    {
        FloatValues[key] = value;
    }

    public float GetFloat(EBlackboardKey key)
    {
        return Get(FloatValues, key);
    }

    public bool TryGet(EBlackboardKey key, out float value, float defaultValue = 0)
    {
        return TryGet(FloatValues, key, out value, defaultValue);
    }

    public void Set(EBlackboardKey key, bool value)
    {
        BoolValues[key] = value;
    }

    public bool GetBool(EBlackboardKey key)
    {
        return Get(BoolValues, key);
    }

    public bool TryGet(EBlackboardKey key, out bool value, bool defaultValue = false)
    {
        return TryGet(BoolValues, key, out value, defaultValue);
    }

    public void Set(EBlackboardKey key, string value)
    {
        StringValues[key] = value;
    }

    public string GetString(EBlackboardKey key)
    {
        return Get(StringValues, key);
    }

    public bool TryGet(EBlackboardKey key, out string value, string defaultValue = "")
    {
        return TryGet(StringValues, key, out value, defaultValue);
    }

    public void Set(EBlackboardKey key, Vector3 value)
    {
        Vector3Values[key] = value;
    }

    public Vector3 GetVector3(EBlackboardKey key)
    {
        return Get(Vector3Values, key);
    }

    public bool TryGet(EBlackboardKey key, out Vector3 value, Vector3 defaultValue)
    {
        return TryGet(Vector3Values, key, out value, defaultValue);
    }

    public void Set(EBlackboardKey key, GameObject value)
    {
        GameObjectValues[key] = value;
    }

    public GameObject GetGameObject(EBlackboardKey key)
    {
        return Get(GameObjectValues, key);
    }

    public bool TryGet(EBlackboardKey key, out GameObject value, GameObject defaultValue = null)
    {
        return TryGet(GameObjectValues, key, out value, defaultValue);
    }
}

public class BlackboardManager : MonoBehaviour
{
    public static BlackboardManager Instance { get; private set; } = null;

    Dictionary<MonoBehaviour, Blackboard> IndividualBlackboards = new Dictionary<MonoBehaviour, Blackboard>();
    Dictionary<int, Blackboard> SharedBlackboards = new Dictionary<int, Blackboard>();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Trying to create second BlackboardManager on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Blackboard GetIndividualBlackboard(MonoBehaviour requestor)
    {
        if (!IndividualBlackboards.ContainsKey(requestor))
            IndividualBlackboards[requestor] = new Blackboard();

        return IndividualBlackboards[requestor];
    }

    public Blackboard GetSharedBlackboard(int uniqueID)
    {
        if (!SharedBlackboards.ContainsKey(uniqueID))
            SharedBlackboards[uniqueID] = new Blackboard();

        return SharedBlackboards[uniqueID];
    }
}
