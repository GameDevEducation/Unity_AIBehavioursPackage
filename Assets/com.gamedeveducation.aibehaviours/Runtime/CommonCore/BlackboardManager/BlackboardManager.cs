using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public class Blackboard<BlackboardKeyType>
    {
        Dictionary<BlackboardKeyType, int> IntValues = new();
        Dictionary<BlackboardKeyType, float> FloatValues = new();
        Dictionary<BlackboardKeyType, bool> BoolValues = new();
        Dictionary<BlackboardKeyType, string> StringValues = new();
        Dictionary<BlackboardKeyType, Vector3> Vector3Values = new();
        Dictionary<BlackboardKeyType, GameObject> GameObjectValues = new();
        Dictionary<BlackboardKeyType, MonoBehaviour> MonoBehaviourValues = new();
        Dictionary<BlackboardKeyType, object> GenericValues = new();

        public void SetGeneric<T>(BlackboardKeyType InKey, T InValue)
        {
            GenericValues[InKey] = InValue;
        }

        public T GetGeneric<T>(BlackboardKeyType InKey)
        {
            if (!GenericValues.ContainsKey(InKey))
                throw new System.ArgumentException($"Could not find value for {InKey} in GenericValues");

            return (T)GenericValues[InKey];
        }

        public bool TryGetGeneric<T>(BlackboardKeyType InKey, out T OutValue, T InDefaultValue)
        {
            if (GenericValues.ContainsKey(InKey))
            {
                OutValue = (T)GenericValues[InKey];
                return true;
            }

            OutValue = InDefaultValue;
            return false;
        }

        private T Get<T>(Dictionary<BlackboardKeyType, T> InKeySet, BlackboardKeyType InKey)
        {
            if (!InKeySet.ContainsKey(InKey))
                throw new System.ArgumentException($"Could not find value for {InKey} in {typeof(T).Name}Values");

            return InKeySet[InKey];
        }

        private bool TryGet<T>(Dictionary<BlackboardKeyType, T> InKeySet, BlackboardKeyType InKey, out T OutValue, T InDefaultValue = default)
        {
            if (InKeySet.ContainsKey(InKey))
            {
                OutValue = InKeySet[InKey];
                return true;
            }

            OutValue = default;
            return false;
        }

        public void Set(BlackboardKeyType InKey, int InValue)
        {
            IntValues[InKey] = InValue;
        }

        public int GetInt(BlackboardKeyType InKey)
        {
            return Get(IntValues, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out int OutValue, int InDefaultValue = 0)
        {
            return TryGet(IntValues, InKey, out OutValue, InDefaultValue);
        }

        public void Set(BlackboardKeyType InKey, float InValue)
        {
            FloatValues[InKey] = InValue;
        }

        public float GetFloat(BlackboardKeyType InKey)
        {
            return Get(FloatValues, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out float OutValue, float InDefaultValue = 0)
        {
            return TryGet(FloatValues, InKey, out OutValue, InDefaultValue);
        }

        public void Set(BlackboardKeyType InKey, bool value)
        {
            BoolValues[InKey] = value;
        }

        public bool GetBool(BlackboardKeyType InKey)
        {
            return Get(BoolValues, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out bool OutValue, bool InDefaultValue = false)
        {
            return TryGet(BoolValues, InKey, out OutValue, InDefaultValue);
        }

        public void Set(BlackboardKeyType InKey, string InValue)
        {
            StringValues[InKey] = InValue;
        }

        public string GetString(BlackboardKeyType InKey)
        {
            return Get(StringValues, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out string OutValue, string InDefaultValue = "")
        {
            return TryGet(StringValues, InKey, out OutValue, InDefaultValue);
        }

        public void Set(BlackboardKeyType InKey, Vector3 InValue)
        {
            Vector3Values[InKey] = InValue;
        }

        public Vector3 GetVector3(BlackboardKeyType InKey)
        {
            return Get(Vector3Values, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out Vector3 OutValue, Vector3 InDefaultValue)
        {
            return TryGet(Vector3Values, InKey, out OutValue, InDefaultValue);
        }

        public void Set<T>(BlackboardKeyType InKey, T InValue) where T : MonoBehaviour
        {
            MonoBehaviourValues[InKey] = InValue;
        }

        public T GetMonoBehaviour<T>(BlackboardKeyType InKey) where T : MonoBehaviour
        {
            return Get(MonoBehaviourValues, InKey) as T;
        }

        public bool TryGet<T>(BlackboardKeyType InKey, out T OutValue, T InDefaultValue = null) where T : MonoBehaviour
        {
            MonoBehaviour TempOutValue = null;

            bool bResult = TryGet(MonoBehaviourValues, InKey, out TempOutValue, InDefaultValue);

            OutValue = TempOutValue as T;

            return bResult;
        }

        public void Set(BlackboardKeyType InKey, GameObject InValue)
        {
            GameObjectValues[InKey] = InValue;
        }

        public GameObject GetGameObject(BlackboardKeyType InKey)
        {
            return Get(GameObjectValues, InKey);
        }

        public bool TryGet(BlackboardKeyType InKey, out GameObject OutValue, GameObject InDefaultValue = null)
        {
            return TryGet(GameObjectValues, InKey, out OutValue, InDefaultValue);
        }
    }

    public class BlackboardManager : Singleton<BlackboardManager>
    {
        Dictionary<MonoBehaviour, Blackboard<FastName>> IndividualBlackboards = new();
        Dictionary<int, Blackboard<FastName>> SharedBlackboards = new();

        public Blackboard<FastName> GetIndividualBlackboard(MonoBehaviour InRequestor)
        {
            if (!IndividualBlackboards.ContainsKey(InRequestor))
                IndividualBlackboards[InRequestor] = new();

            return IndividualBlackboards[InRequestor];
        }

        public Blackboard<FastName> GetSharedBlackboard(int InUniqueID)
        {
            if (!SharedBlackboards.ContainsKey(InUniqueID))
                SharedBlackboards[InUniqueID] = new();

            return SharedBlackboards[InUniqueID];
        }
    }
}
