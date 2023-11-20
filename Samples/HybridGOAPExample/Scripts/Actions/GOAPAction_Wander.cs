using HybridGOAP;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPAction_Wander : GOAPAction_FSM
    {
        [SerializeField] float MinWanderRange = 10.0f;
        [SerializeField] float MaxWanderRange = 50.0f;

        [SerializeField] float MinWaitTime = 4.0f;
        [SerializeField] float MaxWaitTime = 8.0f;

        [SerializeField] float NavigationSearchRange = 5.0f;
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 1f; // intentionally low cost
        }

        protected override void ConfigureFSM()
        {
            var State_PickLocation = LinkedStateMachine.AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetWanderLocation));
            var State_MoveToLocation = LinkedStateMachine.AddState(new SMState_MoveTo(Navigation, StoppingDistance));
            var State_Wait = LinkedStateMachine.AddState(new SMState_WaitForTime(MinWaitTime, MaxWaitTime));

            State_PickLocation.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State_MoveToLocation);
            State_MoveToLocation.AddTransition(new SMTransition_FinishedMove(Navigation), State_Wait);

            LinkedStateMachine.AddDefaultTransitions(InternalState_Finished, InternalState_Failed);
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Wander) };
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
