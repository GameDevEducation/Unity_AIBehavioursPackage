using HybridGOAP;
using StateMachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HybridGOAPExample
{
    public class GOAPAction_Chase : GOAPAction_FSM
    {
        [SerializeField] float NavigationSearchRange = 5.0f;
        [SerializeField] float StoppingDistance = 1f;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureFSM()
        {
            var ChildStates = new List<ISMState>();
            ChildStates.Add(new SMState_CalculateMoveLocation(Navigation, NavigationSearchRange, GetTargetLocation, true));
            ChildStates.Add(new SMState_MoveTo(Navigation, StoppingDistance, true));

            LinkedStateMachine.AddState(new SMState_Parallel(ChildStates));

            LinkedStateMachine.AddDefaultTransitions(InternalState_Finished, InternalState_Failed);
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_Chase) };
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
    }
}
