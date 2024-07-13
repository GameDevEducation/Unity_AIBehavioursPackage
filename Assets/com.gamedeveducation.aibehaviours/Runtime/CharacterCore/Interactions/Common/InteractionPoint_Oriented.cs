using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interaction Point: Oriented")]
    public class InteractionPoint_Oriented : MonoBehaviour, IInteractionPoint
    {
        public Transform PointTransform => transform;

        public Vector3 PointPosition => transform.position;

        public Quaternion PointRotation => transform.rotation;
    }
}
