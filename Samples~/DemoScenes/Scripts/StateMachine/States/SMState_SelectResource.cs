using CommonCore;
using StateMachine;
using UnityEngine;

namespace DemoScenes
{
    public class SMState_SelectResource : SMStateBase
    {
        IResourceQueryInterface ResourceInterface;

        public SMState_SelectResource(IResourceQueryInterface InResourceInterface, string InDisplayName = null) :
            base(InDisplayName)
        {
            ResourceInterface = InResourceInterface;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, CommonCore.Resources.EType.Unknown);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);

            if (ResourceInterface == null)
                return ESMStateStatus.Failed;

            // Attempt to pick a focus
            var FocusType = CommonCore.Resources.EType.Unknown;
            ResourceInterface.RequestResourceFocus(Self, (CommonCore.Resources.EType InFocus) =>
            {
                FocusType = InFocus;
            });

            if (FocusType == CommonCore.Resources.EType.Unknown)
                return ESMStateStatus.Failed;

            // Attempting to pick as source
            var FocusSource = (GameObject)null;
            ResourceInterface.RequestResourceSource(Self, FocusType, (GameObject InSource) =>
            {
                FocusSource = InSource;
            });

            if (FocusSource == null)
                return ESMStateStatus.Failed;

            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, FocusType);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, FocusSource);

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
