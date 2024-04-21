using UnityEngine;

namespace BehaviourTree
{
    public class BTAction_Wait : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Wait";

        float MinTimeToWait;
        float? MaxTimeToWait;
        float TimeRemaining;

        public BTAction_Wait(float InMinTimeToWait, float? InMaxTimeToWait = null, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            MinTimeToWait = InMinTimeToWait;
            MaxTimeToWait = InMaxTimeToWait;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            if (MaxTimeToWait == null)
                TimeRemaining = MinTimeToWait;
            else
                TimeRemaining = Random.Range(MinTimeToWait, MaxTimeToWait.Value);
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (TimeRemaining > 0)
                TimeRemaining -= InDeltaTime;

            return SetStatusAndCalculateReturnValue(TimeRemaining > 0 ? EBTNodeResult.InProgress : EBTNodeResult.Succeeded);
        }
    }
}
