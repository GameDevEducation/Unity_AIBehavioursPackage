using System;
using System.Collections.Generic;

namespace CommonCore
{
    public sealed class FastName : IEquatable<FastName>, IComparable<FastName>
    {
        public static FastName None = new();

        UInt32 NameID;

        private FastName()
        {
            NameID = 0;
        }

        public FastName(string InName)
        {
            NameID = NameManager.CreateOrRetrieveID(InName);
        }

        public override string ToString()
        {
            if (this == None)
                return "## None ##";

            return NameManager.RetrieveNameFromID(NameID);
        }

        public override bool Equals(object InOther)
        {
            return Equals(InOther as FastName);
        }

        public bool Equals(FastName InOther)
        {
            return InOther is not null &&
                   NameID == InOther.NameID;
        }

        public override int GetHashCode()
        {
            return NameID.GetHashCode();
        }

        public int CompareTo(FastName InOther)
        {
            return NameID.CompareTo(InOther.NameID);
        }

        public static bool operator ==(FastName InLHS, FastName InRHS)
        {
            return EqualityComparer<FastName>.Default.Equals(InLHS, InRHS);
        }

        public static bool operator !=(FastName InLeft, FastName InRight)
        {
            return !(InLeft == InRight);
        }

        public static implicit operator bool(FastName InOther)
        {
            return InOther != None;
        }
    }

    public class NameManager : MonoBehaviourSingleton<NameManager>
    {
        UInt32 NextNameID = 1;
        Dictionary<UInt32, string> NameIDs = new();
        static object _NameIDsLock = new object();

        internal static uint CreateOrRetrieveID(string InName)
        {
            if (Instance == null)
                return 0;

            return Instance.CreateOrRetrieveIDInternal(InName);
        }

        internal static string RetrieveNameFromID(uint NameID)
        {
            if (Instance == null)
                return "## NO NameManager ##";

            return Instance.RetrieveNameFromIDInternal(NameID);
        }

        internal UInt32 CreateOrRetrieveIDInternal(string InName)
        {
            lock (_NameIDsLock)
            {
                // does this name already exist?
                UInt32 FoundNameID = 0;
                foreach (var KVP in NameIDs)
                {
                    if (KVP.Value == InName)
                    {
                        FoundNameID = KVP.Key;
                        break;
                    }
                }

                // name ID not found - create
                if (FoundNameID == 0)
                {
                    FoundNameID = NextNameID;
                    ++NextNameID;

                    NameIDs.Add(FoundNameID, InName);
                }

                return FoundNameID;
            }
        }

        internal string RetrieveNameFromIDInternal(UInt32 NameID)
        {
            lock (_NameIDsLock)
            {
                string FoundName = null;

                if (NameIDs.TryGetValue(NameID, out FoundName))
                    return FoundName;

                return "## Missing ID ##";
            }
        }
    }
}
