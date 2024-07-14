using CharacterCore;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [AddComponentMenu("Character/Interactions/Interactable: TV")]
    public class Interactable_TV : SimpleInteractable
    {
        [SerializeField] List<GameObject> InteractionGameObjectsThatNeedPower;

        List<IInteraction> InteractionsRequiringPower;

        public bool IsOn { get; private set; } = false;

        public void TogglePower()
        {
            IsOn = !IsOn;
        }

        protected override void Initialise()
        {
            base.Initialise();

            if ((InteractionGameObjectsThatNeedPower != null) &&
                (InteractionGameObjectsThatNeedPower.Count > 0))
            {
                InteractionsRequiringPower = new();

                foreach (var InteractionGO in InteractionGameObjectsThatNeedPower)
                {
                    IInteraction Interaction = InteractionGO.GetComponent<IInteraction>();

                    if (Interaction != null)
                        InteractionsRequiringPower.Add(Interaction);
                }
            }
        }

        public override bool IsUsable(IInteraction InInteraction = null)
        {
            if (!base.IsUsable(InInteraction))
                return false;

            if (!IsOn && (InteractionsRequiringPower.Count > 0))
            {
                return !InteractionsRequiringPower.Contains(InInteraction);
            }

            return true;
        }
    }
}
