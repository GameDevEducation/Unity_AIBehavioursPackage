using HybridGOAP;
using UnityEngine;

namespace DemoScenes
{
    public class GOAPBrainWrapper : GOAPBrainBase
    {
        protected override void ConfigureBlackboard()
        {
        }

        protected override void ConfigureBrain()
        {
        }

        protected override void OnPreTickBrain(float InDeltaTime)
        {
            base.OnPreTickBrain(InDeltaTime);

            GameObject CurrentAwarenessTarget = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out CurrentAwarenessTarget, null);

            GameObject PreviousAwarenessTarget = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_PreviousBestTarget, out PreviousAwarenessTarget, null);

            // if we're changing targets?
            if ((PreviousAwarenessTarget != null) && (CurrentAwarenessTarget != PreviousAwarenessTarget))
            {
                // clear the look at target if it matches the old awareness target
                GameObject CurrentLookAtTarget = null;
                LinkedBlackboard.TryGet(CommonCore.Names.LookAt_GameObject, out CurrentLookAtTarget, null);

                if (CurrentLookAtTarget == PreviousAwarenessTarget)
                    LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

                // clear the focus/move target if it matches the old awareness target
                GameObject CurrentTarget = null;
                LinkedBlackboard.TryGet(CommonCore.Names.Target_GameObject, out CurrentTarget, null);

                if (CurrentTarget == PreviousAwarenessTarget)
                    LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            }
        }
    }
}
