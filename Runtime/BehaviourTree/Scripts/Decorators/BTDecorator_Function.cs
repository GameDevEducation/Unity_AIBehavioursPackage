namespace BehaviourTree
{
    public class BTDecorator_Function : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => false;

        public override string DebugDisplayName { get; protected set; } = "Run Function";

        System.Func<float, bool> OnEvaluateFn;

        public BTDecorator_Function(System.Func<float, bool> InOnEvaluateFn, bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            OnEvaluateFn = InOnEvaluateFn;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            if (OnEvaluateFn == null)
                return false;

            return OnEvaluateFn.Invoke(InDeltaTime);
        }
    }
}
