using BehaviourTree;
using CharacterCore;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/Behaviour Tree: Sims AI")]
    public class BTStandalone_Sims_AI : BTStandaloneBase
    {
        [Header("Interact")]
        [SerializeField] float MinInteractCooldown = 1.0f;
        [SerializeField] float MaxInteractCooldown = 5.0f;

        [Header("Wander")]
        [SerializeField] float MinWanderRange = 10.0f;
        [SerializeField] float MaxWanderRange = 50.0f;

        [SerializeField] float MinWanderWaitTime = 4.0f;
        [SerializeField] float MaxWanderWaitTime = 8.0f;

        [Header("Idle")]
        [SerializeField] float MinIdleWaitTime = 1.0f;
        [SerializeField] float MaxIdleWaitTime = 10.0f;

        [Header("Navigation")]
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("FPS AI")) as BTFlowNode_Parallel;

            var CoreBehavioursRoot = new BTFlowNode_Selector("Core Logic") as BTFlowNode_Selector;
            LocalRoot.SetPrimary(CoreBehavioursRoot);

            var InteractRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Sequence("Use Interactable")) as BTFlowNode_Sequence;
            InteractRoot.AddDecorator(new BTDecorator_Cooldown(MinInteractCooldown, MaxInteractCooldown));
            InteractRoot.AddDecorator(new BTDecorator_Function(CanInteract, false, "Can Interact?"));

            InteractRoot.AddChild(new BTAction_SelectInteraction());
            InteractRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetInteractableLocation, GetInteractionDirection));
            InteractRoot.AddChild(new BTAction_Move(StoppingDistance));
            InteractRoot.AddChild(new BTAction_UseInteractable());

            var WanderRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Sequence("Wander")) as BTFlowNode_Sequence;
            WanderRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetWanderLocation, null));
            WanderRoot.AddChild(new BTAction_Move(StoppingDistance));
            WanderRoot.AddChild(new BTAction_Wait(MinWanderWaitTime, MaxWanderWaitTime));

            var IdleRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Selector("Idle")) as BTFlowNode_Selector;
            IdleRoot.AddChild(new BTAction_Wait(MinIdleWaitTime, MaxIdleWaitTime));

            var LookAtPOIRoot = new BTFlowNode_Selector("Look at POI") as BTFlowNode_Selector;
            LocalRoot.SetSecondary(LookAtPOIRoot);

            LookAtPOIRoot.AddService(new BTService_SelectPOI(), true);
            LookAtPOIRoot.AddChild(new BTAction_LookAtPOI());
        }

        protected override void ConfigureBlackboard()
        {
        }

        protected override void ConfigureBrain()
        {
        }

        bool CanInteract(float InDeltaTime)
        {
            IInteraction CurrentInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out CurrentInteraction, null);

            if (CurrentInteraction != null)
                return true;

            IInteractable FoundInteractable;
            IInteraction FoundInteraction;

            return InteractionInterface.PickInteraction(PerformerInterface, out FoundInteractable, out FoundInteraction);
        }

        Vector3 GetWanderLocation()
        {
            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);

            // pick random direction and distance
            float Angle = Random.Range(0f, Mathf.PI * 2f);
            float Distance = Random.Range(MinWanderRange, MaxWanderRange);

            // generate a position
            Vector3 WanderTarget = CurrentLocation;
            WanderTarget.x += Distance * Mathf.Sin(Angle);
            WanderTarget.z += Distance * Mathf.Cos(Angle);

            return WanderTarget;
        }

        Vector3 GetInteractableLocation()
        {
            // attempt to get the interact point
            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint != null)
                return TargetPoint.PointPosition;

            return CommonCore.Constants.InvalidVector3Position;
        }

        Vector3 GetInteractionDirection()
        {
            // attempt to get the interact point
            IInteractionPoint TargetPoint;
            LinkedBlackboard.TryGetStorable<IInteractionPoint>(CommonCore.Names.Interaction_Point, out TargetPoint, null);
            if (TargetPoint != null)
                return TargetPoint.PointTransform.forward;

            return CommonCore.Constants.InvalidVector3Position;
        }

        protected override void OnBehaviourTreeReset()
        {
            base.OnBehaviourTreeReset();

            IInteraction CurrentInteraction;
            LinkedBlackboard.TryGetStorable<IInteraction>(CommonCore.Names.Interaction_Type, out CurrentInteraction, null);
            if (CurrentInteraction != null)
                CurrentInteraction.AbandonInteraction(PerformerInterface);

            LinkedBlackboard.Set(CommonCore.Names.Interaction_Interactable, (IInteractable)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (IInteraction)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Point, (IInteractionPoint)null);
        }
    }
}
