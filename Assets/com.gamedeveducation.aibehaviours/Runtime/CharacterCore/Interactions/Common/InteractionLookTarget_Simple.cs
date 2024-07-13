using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Interactions/Interaction Look Target: Simple")]
    public class InteractionLookTarget_Simple : MonoBehaviour, IInteractionLookTarget
    {
        public Vector3 GetLocation()
        {
            return transform.position;
        }
    }
}
