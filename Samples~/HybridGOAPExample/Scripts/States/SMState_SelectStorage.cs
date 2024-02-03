using CommonCore;
using StateMachine;
using UnityEngine;

namespace HybridGOAPExample
{
    public class SMState_SelectStorage : SMStateBase
    {
        IResourceQueryInterface ResourceInterface;

        public SMState_SelectStorage(IResourceQueryInterface InResourceInterface, string InCustomName = null) :
            base(InCustomName)
        {
            ResourceInterface = InResourceInterface;
        }

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            InBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);

            if (ResourceInterface == null)
                return ESMStateStatus.Failed;

            var Self = GetOwner(InBlackboard);

            // get the focus type
            var FocusType = CommonCore.Resources.EType.Unknown;
            InBlackboard.TryGetGeneric<CommonCore.Resources.EType>(CommonCore.Names.Resource_FocusType, out FocusType, CommonCore.Resources.EType.Unknown);

            if (FocusType == CommonCore.Resources.EType.Unknown)
                return ESMStateStatus.Failed;

            // Select a storage container
            GameObject StorageGO = null;
            ResourceInterface.RequestResourceStorage(Self, FocusType, (GameObject InStorage) =>
            {
                StorageGO = InStorage;
            });

            if (StorageGO == null)
                return ESMStateStatus.Failed;

            InBlackboard.Set(CommonCore.Names.Resource_FocusStorage, StorageGO);

            return ESMStateStatus.Finished;
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return CurrentStatus;
        }
    }
}
