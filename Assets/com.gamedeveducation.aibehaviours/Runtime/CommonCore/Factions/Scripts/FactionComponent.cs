using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [AddComponentMenu("AI/Factions/Faction Component")]
    public class FactionComponent : MonoBehaviour, IFaction
    {
        [field: SerializeField] public FactionDefinition Definition { get; protected set; }

        List<FactionDefinition.FactionStanding> _CurrentStandings = null;
        public List<FactionDefinition.FactionStanding> CurrentStandings
        {
            get
            {
                if (_CurrentStandings == null)
                {
                    _CurrentStandings = new();
                    foreach (var Standing in Definition.DefaultRelationships)
                    {
                        _CurrentStandings.Add(new FactionDefinition.FactionStanding(Standing));
                    }
                }

                return _CurrentStandings;
            }
        }

        void Awake()
        {
            ServiceLocator.RegisterService<IFaction>(this, gameObject);
        }

        public EFactionRelationship GetRelationshipTo(IFaction InOther)
        {
            foreach (var Standing in CurrentStandings)
            {
                if (Standing.OtherFaction == InOther.Definition)
                    return Standing.Relationship;
            }

            return EFactionRelationship.Unknown;
        }
    }
}
