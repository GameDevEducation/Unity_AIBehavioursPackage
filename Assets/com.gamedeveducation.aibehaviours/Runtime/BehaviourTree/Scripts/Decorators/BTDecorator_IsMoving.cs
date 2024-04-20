namespace BehaviourTree
{
    public class BTDecorator_IsMoving : BTDecoratorBase
    {
        public override bool CanPostProcessTickResult => false;

        public override string DebugDisplayName { get; protected set; } = "Is Moving?";

        public BTDecorator_IsMoving(bool bInIsInverted = false, string InDisplayName = null) :
            base(bInIsInverted)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;
        }

        protected override bool OnEvaluate(float InDeltaTime)
        {
            return OwningTree.NavigationInterface.IsPathfindingOrMoving(Self);
        }
    }
}
