using BehaviourTree;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_Store : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Store";

        float Speed;
        CommonCore.ResourceContainer Storage;

        CommonCore.FastName AmountHeldKey;

        public BTAction_Store(float InStoreSpeed, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            Speed = InStoreSpeed;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            // get the focus type
            var FocusType = CommonCore.Resources.EType.Unknown;
            LinkedBlackboard.TryGetGeneric<CommonCore.Resources.EType>(CommonCore.Names.Resource_FocusType, out FocusType, CommonCore.Resources.EType.Unknown);

            if (FocusType == CommonCore.Resources.EType.Unknown)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            // get the container game object
            GameObject StorageGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Resource_FocusStorage, out StorageGO, null);

            if (StorageGO == null)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            if (!StorageGO.TryGetComponent<CommonCore.ResourceContainer>(out Storage))
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            // check that the container is valid
            if ((Storage.ResourceType != FocusType) || !Storage.CanStore)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            var ResourceName = FocusType.ToString();
            AmountHeldKey = new FastName($"Self.Inventory.{ResourceName}.Held");

            LastStatus = EBTNodeResult.InProgress;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            float AmountHeld = LinkedBlackboard.GetFloat(AmountHeldKey);

            float AmountToStore = Mathf.Max(InDeltaTime * Speed, 0);

            Storage.StoreResource(AmountToStore);

            AmountHeld -= AmountToStore;
            LinkedBlackboard.Set(AmountHeldKey, AmountHeld);

            if (!Storage.CanStore || (AmountHeld <= 0))
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.InProgress);
        }

        protected override void OnExit()
        {
            base.OnExit();

            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, CommonCore.Resources.EType.Unknown);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);
        }
    }
}
