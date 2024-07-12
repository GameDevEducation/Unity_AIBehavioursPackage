using CommonCore;
using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Character: Look At Target")]
    public class LookTargetBase : MonoBehaviour, ILookTarget
    {
        [SerializeField] protected Vector3 Offset = Vector3.zero;
        [SerializeField] protected Transform OverrideTransform = null;

        protected Transform TargetTransform => (OverrideTransform != null) ? OverrideTransform : transform;
        protected Vector3 TargetLocation => TargetTransform.position + Offset;

        protected virtual void Awake()
        {
            ServiceLocator.RegisterService<ILookTarget>(this, gameObject);
        }

        public Vector3 GetLocation()
        {
            return TargetLocation;
        }
    }
}
