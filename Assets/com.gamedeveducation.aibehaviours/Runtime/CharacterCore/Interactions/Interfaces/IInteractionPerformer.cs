using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    public interface IInteractionPerformer : ILocatableService
    {
        string DisplayName { get; }
        Vector3 PerformerLocation { get; }
        IAnimationInterface AnimationInterface { get; }
    }
}
