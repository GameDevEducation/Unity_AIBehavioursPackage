using HybridGOAP;
using StateMachine;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPAction_GatherResource : GOAPAction_FSM
    {
        [SerializeField] float NavigationSearchRange = 5.0f;
        [SerializeField] float StoppingDistance = 0.1f;

        [SerializeField] float GatherSpeed = 5f;
        [SerializeField] float StoreSpeed = 10f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureFSM()
        {
            var State_SelectResource = AddState(new SMState_SelectResource(SimpleResourceWrangler.Instance));
            var State_GetResourceLocation = AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetSourceLocation));
            var State_MoveToResource = AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToSource"));
            var State_GatherResource = AddState(new SMState_Gather(GatherSpeed));

            var State_SelectStorage = AddState(new SMState_SelectStorage(SimpleResourceWrangler.Instance));
            var State_GetStorageLocation = AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetStorageLocation));
            var State_MoveToStorage = AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToStorage"));
            var State_StoreResource = AddState(new SMState_Store(StoreSpeed));

            State_SelectResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetResourceLocation);
            State_GetResourceLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToResource);
            State_MoveToResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GatherResource);
            State_GatherResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_SelectStorage);

            State_SelectStorage.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetStorageLocation);
            State_GetStorageLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToStorage);
            State_MoveToStorage.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_StoreResource);

            AddDefaultTransitions();
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
