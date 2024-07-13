using BehaviourTree;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/Behaviour Tree: Village AI")]
    public class BTStandalone_Village_AI : BTStandaloneBase
    {
        [Header("Resources")]
        [SerializeField] float GatherSpeed = 5.0f;
        [SerializeField] float StoreSpeed = 10.0f;
        [SerializeField] float MinGatherCooldown = 5.0f;
        [SerializeField] float MaxGatherCooldown = 10.0f;
        [SerializeField][Range(0.0f, 1.0f)] float MinimumGatherDesire = 0.1f;

        [Header("Return Home")]
        [SerializeField] float MissingHomeStartDistance = 50.0f;
        [SerializeField] float MissingHomePeaksDistance = 100.0f;
        [SerializeField] float MissingHomeResetDistance = 10.0f;
        [SerializeField] float MinReturnHomeCooldown = 1.0f;
        [SerializeField] float MaxReturnHomeCooldown = 5.0f;

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

        bool? bIsReturningHome;

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Parallel("RTS AI")) as BTFlowNode_Parallel;

            var CoreBehavioursRoot = new BTFlowNode_Selector("Core Logic") as BTFlowNode_Selector;
            LocalRoot.SetPrimary(CoreBehavioursRoot);

            var ReturnHomeRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Sequence("Return Home")) as BTFlowNode_Sequence;
            ReturnHomeRoot.AddDecorator(new BTDecorator_Cooldown(MinReturnHomeCooldown, MaxReturnHomeCooldown));
            ReturnHomeRoot.AddDecorator(new BTDecorator_Function(ShouldReturnHome, false, "Return Home??"));
            ReturnHomeRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetHomeLocation, null));
            ReturnHomeRoot.AddChild(new BTAction_Move(StoppingDistance));

            var GatherRoot = CoreBehavioursRoot.AddChild(new BTFlowNode_Sequence("Gather Resources")) as BTFlowNode_Sequence;
            GatherRoot.AddDecorator(new BTDecorator_Cooldown(MinGatherCooldown, MaxGatherCooldown));
            GatherRoot.AddDecorator(new BTDecorator_Function(CanGather, false, "Can Gather?"));

            GatherRoot.AddChild(new BTAction_SelectResource(SimpleResourceWrangler.Instance));
            GatherRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetSourceLocation, null));
            GatherRoot.AddChild(new BTAction_Move(StoppingDistance));
            GatherRoot.AddChild(new BTAction_Gather(GatherSpeed));

            GatherRoot.AddChild(new BTAction_SelectStorage(SimpleResourceWrangler.Instance));
            GatherRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetStorageLocation, null));
            GatherRoot.AddChild(new BTAction_Move(StoppingDistance));
            GatherRoot.AddChild(new BTAction_Store(StoreSpeed));

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

        bool ShouldReturnHome(float InDeltaTime)
        {
            Vector3 HomeLocation = GetHomeLocation();

            if (HomeLocation == CommonCore.Constants.InvalidVector3Position)
                return false;

            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);

            float DistanceToHomeSq = (HomeLocation - CurrentLocation).sqrMagnitude;

            if (DistanceToHomeSq < (MissingHomeStartDistance * MissingHomeStartDistance))
            {
                bIsReturningHome = null;
                return false;
            }
            else if (DistanceToHomeSq >= (MissingHomePeaksDistance * MissingHomePeaksDistance))
            {
                bIsReturningHome = true;
                return true;
            }

            if (bIsReturningHome != null)
            {
                if (DistanceToHomeSq < (MissingHomeResetDistance * MissingHomeResetDistance))
                    bIsReturningHome = null;
                else
                    return bIsReturningHome.Value;
            }

            return false;
        }

        bool CanGather(float InDeltaTime)
        {
            if (SimpleResourceWrangler.Instance == null)
                return false;

            float DesireToGather = float.MinValue;
            SimpleResourceWrangler.Instance.GetGatherResourceDesire(Self, (float InDesire) =>
            {
                DesireToGather = InDesire;
            });

            return DesireToGather >= MinimumGatherDesire;
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

        Vector3 GetSourceLocation()
        {
            Vector3 SourceLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to retrieve the source
            GameObject SourceGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Resource_FocusSource, out SourceGO, null);
            if (SourceGO != null)
                SourceLocation = SourceGO.transform.position;

            return SourceLocation;
        }

        Vector3 GetStorageLocation()
        {
            Vector3 StorageLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to retrieve the storage
            GameObject StorageGO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Resource_FocusStorage, out StorageGO, null);
            if (StorageGO != null)
                StorageLocation = StorageGO.transform.position;

            return StorageLocation;
        }

        Vector3 GetHomeLocation()
        {
            Vector3 HomeLocation = CommonCore.Constants.InvalidVector3Position;

            LinkedBlackboard.TryGet(CommonCore.Names.HomeLocation, out HomeLocation, CommonCore.Constants.InvalidVector3Position);

            return HomeLocation;
        }
    }
}
