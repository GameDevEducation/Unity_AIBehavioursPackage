using HybridGOAP;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
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
            var State_SelectResource = LinkedStateMachine.AddState(new SMState_SelectResource(SimpleResourceWrangler.Instance));
            var State_GetResourceLocation = LinkedStateMachine.AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetSourceLocation));
            var State_MoveToResource = LinkedStateMachine.AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToSource"));
            var State_GatherResource = LinkedStateMachine.AddState(new SMState_Gather(GatherSpeed));

            var State_SelectStorage = LinkedStateMachine.AddState(new SMState_SelectStorage(SimpleResourceWrangler.Instance));
            var State_GetStorageLocation = LinkedStateMachine.AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetStorageLocation));
            var State_MoveToStorage = LinkedStateMachine.AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToStorage"));
            var State_StoreResource = LinkedStateMachine.AddState(new SMState_Store(StoreSpeed));

            State_SelectResource.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_GetResourceLocation);
            State_GetResourceLocation.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_MoveToResource);
            State_MoveToResource.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_GatherResource);
            State_GatherResource.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_SelectStorage);

            State_SelectStorage.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_GetStorageLocation);
            State_GetStorageLocation.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_MoveToStorage);
            State_MoveToStorage.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_StoreResource);

            LinkedStateMachine.AddDefaultTransitions(InternalState_Finished, InternalState_Failed);
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
