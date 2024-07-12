using StateMachine;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAP
{
    [AddComponentMenu("AI/GOAP/FSM Actions/FSM Action: Look At Point of Interest")]
    public class GOAP_SMAction_LookAtPOI : GOAPAction_FSM
    {
        public override float CalculateCost()
        {
            return 1.0f; // intentionally low cost
        }

        protected override void ConfigureFSM()
        {
            var ChildStates = new List<ISMState>();

            ChildStates.Add(new SMState_SelectPOI(LookAtHandler));
            ChildStates.Add(new SMState_LookAtPOI(LookAtHandler));

            AddState(new SMState_Parallel(ChildStates));

            AddDefaultTransitions();
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Head;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_LookAtPOI) };
        }
    }
}
