using CommonCore;
using StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace DemoScenes
{
    [AddComponentMenu("AI/Standalone/State Machine: Sims AI")]
    public class SMStandalone_Sims_AI : SMStandaloneBase
    {
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

            // Interact State
            var SMInteract = SMCoreLogic.AddState(new SMState_SMContainer("Interact")) as SMState_SMContainer;
            {
                var State_SelectInteractable = SMInteract.AddState(new SMState_SelectInteraction(SelectInteractionFn));
                var State_GetInteractableLocation = SMInteract.AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetInteractableLocation));
                var State_MoveToInteractable = SMInteract.AddState(new SMState_MoveTo(Navigation, StoppingDistance));
                var State_UseInteractable = SMInteract.AddState(new SMState_UseInteractable());

                State_SelectInteractable.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_GetInteractableLocation);
                State_GetInteractableLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToInteractable);
                State_MoveToInteractable.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_UseInteractable);

                SMInteract.FinishConstruction();
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
                SMInteract.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMWander);
                SMInteract.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMWander);

                SMWander.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMIdle);
                SMWander.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMIdle);

                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), SMInteract);
                SMIdle.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Failed), SMInteract);

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

        System.Tuple<SmartObject, BaseInteraction> SelectInteractionFn(GameObject InQuerier)
        {
            SmartObject TargetSO = null;
            BaseInteraction TargetInteraction = null;

            SelectRandomInteraction(Self, -1.0f, (SmartObject InTargetSO, BaseInteraction InTargetInteraction) =>
            {
                TargetSO = InTargetSO;
                TargetInteraction = InTargetInteraction;
            });

            return new System.Tuple<SmartObject, BaseInteraction>(TargetSO, TargetInteraction);
        }

        Vector3 GetInteractableLocation()
        {
            Vector3 InteractableLocation = CommonCore.Constants.InvalidVector3Position;

            // attempt to get the smart object
            SmartObject InteractableSO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out InteractableSO, null);
            if (InteractableSO != null)
            {
                InteractableLocation = InteractableSO.InteractionPoint;
            }

            return InteractableLocation;
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

        protected override void OnStateMachineReset()
        {
            base.OnStateMachineReset();

            SmartObject TargetSO = null;
            LinkedBlackboard.TryGet(CommonCore.Names.Interaction_SmartObject, out TargetSO, null);
            if (TargetSO != null)
            {
                BaseInteraction TargetInteraction = null;
                LinkedBlackboard.TryGet(CommonCore.Names.Interaction_Type, out TargetInteraction, null);
                if (TargetInteraction != null)
                {
                    TargetInteraction.UnlockInteraction(Self);
                }
            }

            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);
        }
    }
}
