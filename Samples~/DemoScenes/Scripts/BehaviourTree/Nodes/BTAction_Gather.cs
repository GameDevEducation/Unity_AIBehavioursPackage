using BehaviourTree;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_Gather : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Gather";

        float Speed;
        CommonCore.ResourceSource Source;

        CommonCore.FastName AmountHeldKey;
        CommonCore.FastName CapacityKey;

        public BTAction_Gather(float InGatherSpeed, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            Speed = InGatherSpeed;
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

            // get the source game object
            GameObject SourceGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Resource_FocusSource, out SourceGO, null);

            if (SourceGO == null)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            if (!SourceGO.TryGetComponent<CommonCore.ResourceSource>(out Source))
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            // check that the source is valid
            if ((Source.ResourceType != FocusType) || !Source.CanHarvest)
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            var ResourceName = FocusType.ToString();
            AmountHeldKey = new FastName($"Self.Inventory.{ResourceName}.Held");
            CapacityKey = new FastName($"Self.Inventory.{ResourceName}.Capacity");

            LastStatus = EBTNodeResult.InProgress;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            float AmountHeld = LinkedBlackboard.GetFloat(AmountHeldKey);
            float Capacity = LinkedBlackboard.GetFloat(CapacityKey);

            float AmountRetrieve = Mathf.Min(InDeltaTime * Speed, Capacity - AmountHeld);

            AmountHeld += Source.Consume(AmountRetrieve);
            LinkedBlackboard.Set(AmountHeldKey, AmountHeld);

            if (!Source.CanHarvest || Mathf.Approximately(AmountHeld, Capacity))
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.InProgress);
        }

        protected override void OnExit()
        {
            base.OnExit();

            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);
        }
    }
}
