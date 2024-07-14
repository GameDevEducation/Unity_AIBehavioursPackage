using CommonCore;
using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_Gather : SMStateBase
    {
        float Speed;
        CommonCore.ResourceSource Source;

        CommonCore.FastName AmountHeldKey;
        CommonCore.FastName CapacityKey;

        public SMState_Gather(float InGatherSpeed, string InDisplayName = null) :
            base(InDisplayName)
        {
            Speed = InGatherSpeed;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            // get the focus type
            var FocusType = CommonCore.Resources.EType.Unknown;
            LinkedBlackboard.TryGetGeneric<CommonCore.Resources.EType>(CommonCore.Names.Resource_FocusType, out FocusType, CommonCore.Resources.EType.Unknown);

            if (FocusType == CommonCore.Resources.EType.Unknown)
                return ESMStateStatus.Failed;

            // get the source game object
            GameObject SourceGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Resource_FocusSource, out SourceGO, null);

            if (SourceGO == null)
                return ESMStateStatus.Failed;

            if (!SourceGO.TryGetComponent<CommonCore.ResourceSource>(out Source))
                return ESMStateStatus.Failed;

            // check that the source is valid
            if ((Source.ResourceType != FocusType) || !Source.CanHarvest)
                return ESMStateStatus.Failed;

            var ResourceName = FocusType.ToString();
            AmountHeldKey = new FastName($"Self.Inventory.{ResourceName}.Held");
            CapacityKey = new FastName($"Self.Inventory.{ResourceName}.Capacity");

            return ESMStateStatus.Running;
        }

        protected override void OnExitInternal()
        {
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            float AmountHeld = LinkedBlackboard.GetFloat(AmountHeldKey);
            float Capacity = LinkedBlackboard.GetFloat(CapacityKey);

            float AmountToRetrieve = Mathf.Min(InDeltaTime * Speed, Capacity - AmountHeld);

            AmountHeld += Source.Consume(AmountToRetrieve);
            LinkedBlackboard.Set(AmountHeldKey, AmountHeld);

            if (!Source.CanHarvest || Mathf.Approximately(AmountHeld, Capacity))
                return ESMStateStatus.Finished;

            return ESMStateStatus.Running;
        }
    }
}
