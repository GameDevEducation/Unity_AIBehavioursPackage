using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    public interface ILookTarget : ILocatableService
    {
        public Vector3 GetLocation();
    }
}
