using HybridGOAP;
using StateMachine;
using UnityEngine;
using UnityEngine.Serialization;

namespace HybridGOAPExample
{
    public class GOAPAction_Wander : GOAPAction_FSM
    {
        [SerializeField] float MinWanderRange = 10.0f;
        [SerializeField] float MaxWanderRange = 50.0f;

        [SerializeField] float MinWaitTime = 4.0f;
        [SerializeField] float MaxWaitTime = 8.0f;

        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField][FormerlySerializedAs("NavigationSearchRange")] float ValidNavMeshSearchRange = 5.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 1f; // intentionally low cost
        }

        protected override void ConfigureFSM()
        {
            var State_PickLocation = AddState(new SMState_CalculateMoveLocation(Navigation, ValidNavMeshSearchRange, GetWanderLocation));
            var State_MoveToLocation = AddState(new SMState_MoveTo(Navigation, StoppingDistance));
            var State_Wait = AddState(new SMState_WaitForTime(MinWaitTime, MaxWaitTime));

            State_PickLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToLocation);
            State_MoveToLocation.AddTransition(new SMTransition_FinishedMove(Navigation), State_Wait);

            AddDefaultTransitions();
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
