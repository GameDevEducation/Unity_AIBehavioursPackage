using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public enum EFactionRelationship
    {
        Friendly,
        Neutral,
        Hostile,

        Unknown
    }

    [CreateAssetMenu(menuName = "AI/Factions/FactionDefinition", fileName = "Faction")]
    public class FactionDefinition : ScriptableObject
    {
        [System.Serializable]
        public class FactionStanding
        {
            [field: SerializeField] public FactionDefinition OtherFaction { get; protected set; }
            [field: SerializeField] public EFactionRelationship Relationship { get; protected set; }

            public FactionStanding(FactionStanding InOther)
            {
                OtherFaction = InOther.OtherFaction; 
                Relationship = InOther.Relationship;
            }
        }

        [field: SerializeField] public string DisplayName { get; protected set; }
        [field: SerializeField] public List<FactionStanding> DefaultRelationships { get; protected set; }
    }
}
