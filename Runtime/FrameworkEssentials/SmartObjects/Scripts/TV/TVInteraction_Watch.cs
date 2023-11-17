using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    [RequireComponent(typeof(SmartObject_TV))]
    public class TVInteraction_Watch : SimpleInteraction
    {
        protected SmartObject_TV LinkedTV;

        protected void Awake()
        {
            LinkedTV = GetComponent<SmartObject_TV>();
        }

        public override bool CanPerform()
        {
            return base.CanPerform() && LinkedTV.IsOn;
        }
    }
}
