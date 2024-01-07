using HybridGOAP;
using UnityEngine;
using UnityEngine.Events;

namespace HybridGOAPExample
{
    public class GOAPGoal_UseInteractable : GOAPGoalBase
    {
        [SerializeField] UnityEvent<GameObject, System.Action<float>> OnGetUseInteractableDesire;

        public override void PrepareForPlanning()
        {
            float InteractDesire = float.MinValue;

            OnGetUseInteractableDesire.Invoke(Self, (float InDesire) =>
            {
                InteractDesire = InDesire;
            });

            if (InteractDesire <= 0)
                Priority = GoalPriority.DoNotRun;
            else
                Priority = Mathf.FloorToInt(Mathf.Lerp(GoalPriority.Low, GoalPriority.High, InteractDesire));
        }
    }
}
