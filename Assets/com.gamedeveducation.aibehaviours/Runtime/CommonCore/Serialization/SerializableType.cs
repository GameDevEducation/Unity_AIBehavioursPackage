using System;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [System.Serializable]
    public class SerializableType<T> : ISerializationCallbackReceiver, IEquatable<SerializableType<T>>
    {
        [field: System.NonSerialized] public System.Type Type { get; private set; } = null;
        [SerializeField, HideInInspector] string AssemblyQualifiedName = null;

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Type = string.IsNullOrEmpty(AssemblyQualifiedName) ? null : System.Type.GetType(AssemblyQualifiedName);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Type != null)
                AssemblyQualifiedName = Type.AssemblyQualifiedName;
            else
                AssemblyQualifiedName = null;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as SerializableType<T>);
        }

        public bool Equals(SerializableType<T> other)
        {
            return other is not null &&
                   AssemblyQualifiedName == other.AssemblyQualifiedName;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(AssemblyQualifiedName);
        }

        public static bool operator ==(SerializableType<T> left, SerializableType<T> right)
        {
            return EqualityComparer<SerializableType<T>>.Default.Equals(left, right);
        }

        public static bool operator !=(SerializableType<T> left, SerializableType<T> right)
        {
            return !(left == right);
        }
    }
}
