using HybridGOAP;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPAction_ReturnHome : GOAPAction_FSM
    {
        [SerializeField] float NavigationSearchRange = 5.0f;
        [SerializeField] float StoppingDistance = 0.1f;

        public override float CalculateCost()
        {
            return 20f;
        }

        protected override void ConfigureFSM()
        {
            var State_GetHomeLocation   = AddState(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetHomeLocation));
            var State_MoveToLocation    = AddState(new SMState_MoveTo(Navigation, StoppingDistance));

            State_GetHomeLocation.AddTransition(SMTransition_StateStatus.IfHasFinished(), State_MoveToLocation);

            AddDefaultTransitions();
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_ReturnHome) };
        }

        Vector3 GetHomeLocation()
        {
            Vector3 HomeLocation = CommonCore.Constants.InvalidVector3Position;

            LinkedBlackboard.TryGet(CommonCore.Names.HomeLocation, out HomeLocation, CommonCore.Constants.InvalidVector3Position);

            return HomeLocation;
        }
    }
}
