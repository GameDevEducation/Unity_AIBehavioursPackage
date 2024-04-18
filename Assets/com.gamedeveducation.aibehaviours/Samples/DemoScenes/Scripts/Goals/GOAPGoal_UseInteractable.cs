using HybridGOAP;
using UnityEngine;
using UnityEngine.Events;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/Goals/Goal: Use Interactable")]
    public class GOAPGoal_UseInteractable : GOAPGoalBase
    {
        [SerializeField] UnityEvent<GameObject, float, System.Action<float>> OnGetUseInteractableDesire;

        public override void PrepareForPlanning()
        {
            float InteractDesire = float.MinValue;

            OnGetUseInteractableDesire.Invoke(Self, -1.0f, (float InDesire) =>
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
