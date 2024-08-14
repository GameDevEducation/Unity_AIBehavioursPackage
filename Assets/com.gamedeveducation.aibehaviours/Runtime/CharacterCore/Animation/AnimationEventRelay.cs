using UnityEngine;

namespace CharacterCore
{
    [AddComponentMenu("Character/Animation: Animator Bridge Event Relay")]
    public class AnimationEventRelay : MonoBehaviour
    {
        [SerializeField] AnimatorBridge LinkedBridge;

        public void OnAnimationStart(string InStateName)
        {
            LinkedBridge.OnAnimationStart(InStateName);
        }

        public void OnAnimationFinish(string InStateName)
        {
            LinkedBridge.OnAnimationFinish(InStateName);
        }
    }
}
