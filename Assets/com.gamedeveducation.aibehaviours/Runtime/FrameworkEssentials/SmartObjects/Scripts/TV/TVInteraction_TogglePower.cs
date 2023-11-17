using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace CommonCore
{
    [RequireComponent(typeof(SmartObject_TV))]
    public class TVInteraction_TogglePower : SimpleInteraction
    {
        protected SmartObject_TV LinkedTV;

        protected void Awake()
        {
            LinkedTV = GetComponent<SmartObject_TV>();
        }

        public override bool Perform(GameObject performer, UnityAction<BaseInteraction> onCompleted)
        {
            LinkedTV.ToggleState();

            return base.Perform(performer, onCompleted);
        }
    }
}
