using BehaviourTree;
using CommonCore;
using UnityEngine;

namespace DemoScenes
{
    public class BTAction_SelectResource : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Select Resource";

        IResourceQueryInterface ResourceInterface;

        public BTAction_SelectResource(IResourceQueryInterface InResourceInterface, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            ResourceInterface = InResourceInterface;
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, CommonCore.Resources.EType.Unknown);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);

            if (ResourceInterface == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            // Attempt to pick a focus
            var FocusType = CommonCore.Resources.EType.Unknown;
            ResourceInterface.RequestResourceFocus(Self, (CommonCore.Resources.EType InFocus) =>
            {
                FocusType = InFocus;
            });

            if (FocusType == CommonCore.Resources.EType.Unknown)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            // Attempting to pick as source
            var FocusSource = (GameObject)null;
            ResourceInterface.RequestResourceSource(Self, FocusType, (GameObject InSource) =>
            {
                FocusSource = InSource;
            });

            if (FocusSource == null)
                return SetStatusAndCalculateReturnValue(EBTNodeResult.Failed);

            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, FocusType);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, FocusSource);

            return SetStatusAndCalculateReturnValue(EBTNodeResult.Succeeded);
        }
    }
}
