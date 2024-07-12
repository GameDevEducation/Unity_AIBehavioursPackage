using StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/State Machine: RTS AI")]
    public class SMStandalone_RTS_AI : SMStandaloneBase
    {
        [Header("Resources")]
        [SerializeField] float GatherSpeed = 5f;
        [SerializeField] float StoreSpeed = 10f;

        [Header("Wander (Short)")]
        [SerializeField] float MinShortWanderRange = 2.0f;
        [SerializeField] float MaxShortWanderRange = 5.0f;
        [SerializeField] float MinShortWanderWaitTime = 2.0f;
        [SerializeField] float MaxShortWanderWaitTime = 4.0f;

        [Header("Wander (Long)")]
        [SerializeField] float MinLongWanderRange = 10.0f;
        [SerializeField] float MaxLongWanderRange = 50.0f;
        [SerializeField] float MinLongWanderWaitTime = 4.0f;
        [SerializeField] float MaxLongWanderWaitTime = 8.0f;

        [Header("Idle")]
        [SerializeField] float MinIdleWaitTime = 1.0f;
        [SerializeField] float MaxIdleWaitTime = 10.0f;

        [Header("Navigation")]
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        protected override void ConfigureFSM()
        {
            var SMCoreLogic = new SMState_SMContainer("Core Logic");

            // Gather State
            var SMGather = SMCoreLogic.AddState(new SMState_SMContainer("Gather")) as SMState_SMContainer;
            {
                var State_SelectResource = SMGather.AddState(new SMState_SelectResource(SimpleResourceWrangler.Instance));
                var State_GetResourceLocation = SMGather.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetSourceLocation));
                var State_MoveToResource = SMGather.AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToSource"));
                var State_GatherResource = SMGather.AddState(new SMState_Gather(GatherSpeed));

                var State_SelectStorage = SMGather.AddState(new SMState_SelectStorage(SimpleResourceWrangler.Instance));
                var State_GetStorageLocation = SMGather.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetStorageLocation));
                var State_MoveToStorage = SMGather.AddState(new SMState_MoveTo(Navigation, StoppingDistance, false, 0f, "SMMoveToStorage"));
                var State_StoreResource = SMGather.AddState(new SMState_Store(StoreSpeed));

                State_SelectResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetResourceLocation);
                State_GetResourceLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToResource);
                State_MoveToResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GatherResource);
                State_GatherResource.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_SelectStorage);

                State_SelectStorage.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetStorageLocation);
                State_GetStorageLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToStorage);
                State_MoveToStorage.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_StoreResource);

                SMGather.FinishConstruction();
            }

            // Wander (Short) State
            var SMWander_Short = SMCoreLogic.AddState(new SMState_SMContainer("Wander (Short)")) as SMState_SMContainer;
            {
                var State_PickLocation = SMWander_Short.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetShortWanderLocation));
                var State_MoveToLocation = SMWander_Short.AddState(new SMState_MoveTo(Navigation, StoppingDistance));
                var State_Wait = SMWander_Short.AddState(new SMState_WaitForTime(MinShortWanderWaitTime, MaxShortWanderWaitTime));

                State_PickLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToLocation);
                State_MoveToLocation.AddTransition(new SMTransition_FinishedMove(Navigation), State_Wait);

                SMWander_Short.FinishConstruction();
            }

            // Wander (Long) State
            var SMWander_Long = SMCoreLogic.AddState(new SMState_SMContainer("Wander (Long)")) as SMState_SMContainer;
            {
                var State_PickLocation = SMWander_Long.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetLongWanderLocation));
                var State_MoveToLocation = SMWander_Long.AddState(new SMState_MoveTo(Navigation, StoppingDistance));
                var State_Wait = SMWander_Long.AddState(new SMState_WaitForTime(MinLongWanderWaitTime, MaxLongWanderWaitTime));

                State_PickLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToLocation);
                State_MoveToLocation.AddTransition(new SMTransition_FinishedMove(Navigation), State_Wait);

                SMWander_Long.FinishConstruction();
            }

            // Idle State
            var SMIdle = SMCoreLogic.AddState(new SMState_WaitForTime(MinIdleWaitTime, MaxIdleWaitTime, "Idle"));

            // Setup core logic transitions
            {
                SMGather.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMWander_Short);
                SMGather.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMWander_Long);

                SMWander_Long.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMIdle);
                SMWander_Long.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMWander_Short);

                SMWander_Short.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMIdle);
                SMWander_Short.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMIdle);

                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMGather);
                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMGather);

                SMCoreLogic.FinishConstruction();
            }

            // Look At State
            var SMLookAt = new SMState_SMContainer("Look At");
            {
                var ChildStates = new List<ISMState>();

                ChildStates.Add(new SMState_SelectPOI(LookAtHandler));
                ChildStates.Add(new SMState_LookAtPOI(LookAtHandler));

                SMLookAt.AddState(new SMState_Parallel(ChildStates));

                SMLookAt.FinishConstruction();
            }

            var ParallelStates = new List<ISMState>();
            ParallelStates.Add(SMCoreLogic);
            ParallelStates.Add(SMLookAt);
            AddState(new SMState_Parallel(ParallelStates, true, false));
            AddDefaultTransitions();
        }

        protected override void ConfigureBlackboard()
        {
        }

        protected override void ConfigureBrain()
        {
        }

        Vector3 GetShortWanderLocation() { return GetWanderLocation(false); }
        Vector3 GetLongWanderLocation() { return GetWanderLocation(true); }

        Vector3 GetWanderLocation(bool bIsLong)
        {
            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);

            // pick random direction and distance
            float Angle = Random.Range(0f, Mathf.PI * 2f);
            float Distance = Random.Range(bIsLong ? MinLongWanderRange : MinShortWanderRange,
                                          bIsLong ? MaxLongWanderRange : MaxShortWanderRange);

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
    }
}
