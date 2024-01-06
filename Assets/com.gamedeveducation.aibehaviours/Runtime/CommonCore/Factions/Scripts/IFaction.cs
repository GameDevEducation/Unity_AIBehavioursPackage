using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    public interface IFaction : ILocatableService
    {
        FactionDefinition Definition { get; }

        EFactionRelationship GetRelationshipTo(IFaction InOther);
    }
}
