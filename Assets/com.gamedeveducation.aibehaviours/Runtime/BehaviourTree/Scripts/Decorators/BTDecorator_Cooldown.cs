using UnityEngine;

namespace BehaviourTree
{
    public class BTDecorator_Cooldown : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => true;

        public override string DebugDisplayName { get; protected set; } = "Cooldown";

        float MinCooldownTime;
        float? MaxCooldownTime;

        float? CooldownRemaining;

        public BTDecorator_Cooldown(float InMinCooldown, float? InMaxCooldown = null, bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            MinCooldownTime = InMinCooldown;
            MaxCooldownTime = InMaxCooldown;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            // not on cooldown?
            if (CooldownRemaining == null)
                return true;

            // update the cooldown and test if expired
            CooldownRemaining = InDeltaTime;
            if (CooldownRemaining <= 0)
            {
                CooldownRemaining = null;
                return true;
            }

            return false;
        }

        public override EBTNodeResult PostProcessTickResult(EBTNodeResult InResult)
        {
            // if there is no cooldown active and the node completed then go on cooldown
            if ((CooldownRemaining == null) &&
                ((InResult == EBTNodeResult.Succeeded) || (InResult == EBTNodeResult.Failed)))
            {
                if (MaxCooldownTime != null)
                    CooldownRemaining = Random.Range(MinCooldownTime, MaxCooldownTime.Value);
                else
                    CooldownRemaining = MinCooldownTime;
            }

            return base.PostProcessTickResult(InResult);
        }
    }
}
