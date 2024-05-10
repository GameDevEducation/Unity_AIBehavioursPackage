using BehaviourTree;
using HybridGOAP;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Gather Resource")]
    public class GOAP_BTAction_GatherResource : GOAPAction_BehaviourTree
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        [SerializeField] float GatherSpeed = 5f;
        [SerializeField] float StoreSpeed = 10f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Sequence("Gather Resources")) as BTFlowNode_Sequence;

            LocalRoot.AddChild(new BTAction_SelectResource(SimpleResourceWrangler.Instance));
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetSourceLocation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));
            LocalRoot.AddChild(new BTAction_Gather(GatherSpeed));

            LocalRoot.AddChild(new BTAction_SelectStorage(SimpleResourceWrangler.Instance));
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetStorageLocation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));
            LocalRoot.AddChild(new BTAction_Store(StoreSpeed));
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_GatherResource) };
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
    }
}
