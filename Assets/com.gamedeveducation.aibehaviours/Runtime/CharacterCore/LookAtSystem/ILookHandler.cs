using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    public interface ILookHandler : ILocatableService
    {
        bool SetLookTarget(Transform InTransform);
        bool SetLookTarget(Vector3 InPosition);

        void ClearLookTarget();

        void DetermineBestLookTarget(Blackboard<FastName> InBlackboard, out GameObject OutLookTargetGO, out Vector3 OutLookTargetPosition);
    }
}
