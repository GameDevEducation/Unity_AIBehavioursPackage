using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    public interface IInteractionPoint : IBlackboardStorable
    {
        Transform PointTransform { get; }
        Vector3 PointPosition { get; }
        Quaternion PointRotation { get; }
    }
}
