using CommonCore;
using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_SelectStorage : SMStateBase
    {
        IResourceQueryInterface ResourceInterface;

        public SMState_SelectStorage(IResourceQueryInterface InResourceInterface, string InDisplayName = null) :
            base(InDisplayName)
        {
            ResourceInterface = InResourceInterface;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);

            if (ResourceInterface == null)
                return ESMStateStatus.Failed;

            // get the focus type
            var FocusType = CommonCore.Resources.EType.Unknown;
            LinkedBlackboard.TryGetGeneric<CommonCore.Resources.EType>(CommonCore.Names.Resource_FocusType, out FocusType, CommonCore.Resources.EType.Unknown);

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

            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, StorageGO);

            return ESMStateStatus.Finished;
        }

        protected override void OnExitInternal()
        {
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            return CurrentStatus;
        }
    }
}
