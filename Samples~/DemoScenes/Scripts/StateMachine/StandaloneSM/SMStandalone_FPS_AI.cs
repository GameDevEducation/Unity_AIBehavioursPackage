using CommonCore;
using StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/State Machine: FPS AI")]
    public class SMStandalone_FPS_AI : SMStandaloneBase
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

        protected override void ConfigureFSM()
        {
            var SMCoreLogic = new SMState_SMContainer("Core Logic");

            // Chase State
            var SMChase = SMCoreLogic.AddState(new SMState_SMContainer("Chase")) as SMState_SMContainer;
            {
                var ChildStates = new List<ISMState>();
                ChildStates.Add(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetTargetLocation, true));
                ChildStates.Add(new SMState_MoveTo(Navigation, StoppingDistance, true));

                SMChase.AddState(new SMState_Parallel(ChildStates));

                SMChase.FinishConstruction();
            }

            // Wander State
            var SMWander = SMCoreLogic.AddState(new SMState_SMContainer("Wander")) as SMState_SMContainer;
            {
                var State_PickLocation = SMWander.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetWanderLocation));
                var State_MoveToLocation = SMWander.AddState(new SMState_MoveTo(Navigation, StoppingDistance));
                var State_Wait = SMWander.AddState(new SMState_WaitForTime(MinWanderWaitTime, MaxWanderWaitTime));

                State_PickLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToLocation);
                State_MoveToLocation.AddTransition(new SMTransition_FinishedMove(Navigation), State_Wait);

                SMWander.FinishConstruction();
            }

            // Idle State
            var SMIdle = SMCoreLogic.AddState(new SMState_WaitForTime(MinIdleWaitTime, MaxIdleWaitTime, "Idle"));

            // Setup core logic transitions
            {
                SMChase.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMWander);
                SMChase.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMWander);
                SMChase.AddTransition(new SMTransition_Function(ShouldLeaveChase), SMWander);

                SMWander.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMIdle);
                SMWander.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMIdle);
                SMWander.AddTransition(new SMTransition_Function(ShouldEnterChase), SMChase);

                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMWander);
                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMWander);
                SMIdle.AddTransition(new SMTransition_Function(ShouldEnterChase), SMChase);

                SMCoreLogic.FinishConstruction();
            }

            // Look At State
            var SMLookAt = new SMState_SMContainer("Look At");
            {
                var ChildStates = new List<ISMState>();

                ChildStates.Add(new SMState_SelectPOI(PickSuitablePOI));
                ChildStates.Add(new SMState_LookAtPOI(SetPOIFn));

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

        ESMTransitionResult ShouldEnterChase(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return CanChase() ? ESMTransitionResult.Valid : ESMTransitionResult.Invalid;
        }

        ESMTransitionResult ShouldLeaveChase(ISMState InCurrentState, Blackboard<FastName> InBlackboard)
        {
            return !CanChase() ? ESMTransitionResult.Valid : ESMTransitionResult.Invalid;
        }

        bool CanChase()
        {
            Vector3 TargetLocation = GetTargetLocation();

            if (TargetLocation == CommonCore.Constants.InvalidVector3Position)
                return false;

            Vector3 CurrentLocation = LinkedBlackboard.GetVector3(CommonCore.Names.CurrentLocation);
            float DistanceToTargetSq = (TargetLocation - CurrentLocation).sqrMagnitude;

            return DistanceToTargetSq < (ChaseMaxDistance * ChaseMaxDistance);
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
    }
}
