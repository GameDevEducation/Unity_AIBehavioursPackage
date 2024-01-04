using HybridGOAP;
using StateMachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace HybridGOAPExample
{
    public class GOAPAction_LookAtPOI : GOAPAction_FSM
    {
        [SerializeField] UnityEvent<GameObject, System.Action<GameObject>> OnPickPOIFn = new();
        [SerializeField] UnityEvent<Transform> OnSetPOIFn = new();

        public override float CalculateCost()
        {
            return 1.0f; // intentionally low cost
        }

        protected override void ConfigureFSM()
        {
            var ChildStates = new List<ISMState>();

            ChildStates.Add(new SMState_SelectPOI(PickPOIFn));
            ChildStates.Add(new SMState_LookAtPOI(SetPOIFn));

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

        GameObject PickPOIFn(GameObject InQuerier)
        {
            var POI = (GameObject)null;
            OnPickPOIFn.Invoke(InQuerier, (GameObject InFoundPOI) =>
            {
                POI = InFoundPOI;
            });

            return POI;
        }

        void SetPOIFn(GameObject InPOI)
        {
            OnSetPOIFn.Invoke(InPOI.transform);
        }
    }
}
