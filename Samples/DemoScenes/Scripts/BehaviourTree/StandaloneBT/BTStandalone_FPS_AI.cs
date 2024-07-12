using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/Behaviour Tree: FPS AI")]
    public class BTStandalone_FPS_AI : BTStandaloneBase
    {
        [Header("Chase")]
        [SerializeField] float ChaseMaxDistance = 25.0f;

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

            var ChaseRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Selector("Chase")) as BTFlowNode_Selector;
            ChaseRoot.AddDecorator(new BTDecorator_Function(CanChase, false, "Can Chase?"));
            ChaseRoot.AddService(new BTService_CalculateMoveLocation(ValidNavMeshSearchRange, GetTargetLocation));
            ChaseRoot.AddChild(new BTAction_Move(StoppingDistance, true));

            var WanderRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Sequence("Wander")) as BTFlowNode_Sequence;
            WanderRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetWanderLocation));
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

        Vector3 GetTargetLocation()
        {
            Vector3 TargetLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to get the target object
            GameObject TargetGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Awareness_BestTarget, out TargetGO, null);

            if (TargetGO != null)
            {
                TargetLocation = TargetGO.transform.position;
                LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, TargetGO);
            }
            else
            {
                LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
                LinkedBlackboard.TryGet(CommonCore.Names.Target_Position, out TargetLocation, CommonCore.Constants.InvalidVector3Position);
            }

            return TargetLocation;
        }

        bool CanChase(float InDeltaTime)
        {
            Vector3 TargetLocation = GetTargetLocation();

            if (TargetLocation == CommonCore.Constants.InvalidVector3Position)
                return false;

            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);
            float DistanceToTargetSq = (TargetLocation - CurrentLocation).sqrMagnitude;

            return DistanceToTargetSq < (ChaseMaxDistance * ChaseMaxDistance);
        }
    }
}
