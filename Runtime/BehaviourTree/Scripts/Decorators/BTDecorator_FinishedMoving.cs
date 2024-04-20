namespace BehaviourTree
{
    public class BTDecorator_FinishedMoving : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => false;

        public override string DebugDisplayName { get; protected set; } = "Move Finished?";

        public BTDecorator_FinishedMoving(bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            return OwningTree.NavigationInterface.HasReachedDestination(Self);
        }
    }
}
