namespace BehaviourTree
{
    public class BTDecorator_ForceResult : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => true;

        public override string DebugDisplayName { get; protected set; } = "Force Result";

        EBTNodeResult ResultToForce = EBTNodeResult.Succeeded;

        public BTDecorator_ForceResult(EBTNodeResult InResultToForce, bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            ResultToForce = InResultToForce;

            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        public override EBTNodeResult PostProcessTickResult(EBTNodeResult InResult)
        {
            return ResultToForce;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            return true;
        }
    }
}
