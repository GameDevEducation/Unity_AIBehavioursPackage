using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    public class BTService_SelectPOI : BTServiceBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select POI";

        System.Func<GameObject, GameObject> PickPOIFn;

        public BTService_SelectPOI(System.Func<GameObject, GameObject> InPickPOIFn, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            PickPOIFn = InPickPOIFn;
        }

        public override bool Tick(float InDeltaTime)
        {
            if (PickPOIFn == null)
            {
                LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
                return false;
            }

            LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, PickPOIFn(Self));

            return true;
        }
    }
}
