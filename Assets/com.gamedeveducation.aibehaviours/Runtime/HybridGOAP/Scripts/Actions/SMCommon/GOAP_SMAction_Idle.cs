using StateMachine;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/FSM Actions/FSM Action: Idle")]
    public class GOAP_SMAction_Idle : GOAPAction_FSM
    {
        [SerializeField] float MinWaitTime = 1.0f;
        [SerializeField] float MaxWaitTime = 10.0f;

        public override float CalculateCost()
        {
            return 0.0f;
        }

        protected override void ConfigureFSM()
        {
            AddState(new SMState_WaitForTime(MinWaitTime, MaxWaitTime));

            AddDefaultTransitions();
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Idle) };
        }
    }
}
