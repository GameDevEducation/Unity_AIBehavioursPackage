using BehaviourTree;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_SelectStorage : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select Storage";

        IResourceQueryInterface ResourceInterface;

        public BTAction_SelectStorage(IResourceQueryInterface InResourceInterface, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            ResourceInterface = InResourceInterface;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);

            if (ResourceInterface == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            // get the focus type
            var FocusType = CommonCore.Resources.EType.Unknown;
            LinkedBlackboard.TryGetGeneric<CommonCore.Resources.EType>(CommonCore.Names.Resource_FocusType, out FocusType, CommonCore.Resources.EType.Unknown);

            if (FocusType == CommonCore.Resources.EType.Unknown)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            // Select a storage container
            GameObject StorageGO = null;
            ResourceInterface.RequestResourceStorage(Self, FocusType, (GameObject InStorage) =>
            {
                StorageGO = InStorage;
            });

            if (StorageGO == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, StorageGO);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);
        }
    }
}
