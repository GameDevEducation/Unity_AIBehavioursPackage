using BehaviourTree;
using CommonCore;
using HybridGOAP;
using UnityEngine;
using UnityEngine.Events;

namespace DemoScenes
{
    [AddComponentMenu("AI/GOAP/BT Actions/BT Action: Use Interactable")]
    public class GOAP_BTAction_UseInteractable : GOAPAction_BehaviourTree
    {
        [Tooltip("Controls how far from our goal location that we will search for a valid location on the NavMesh.")]
        [SerializeField] float ValidNavMeshSearchRange = 2.0f;

        [Tooltip("Controls how close the AI needs to get to the destination to consider it reached.")]
        [SerializeField] float StoppingDistance = 0.1f;

        [SerializeField] UnityEvent<GameObject, float, System.Action<SmartObject, BaseInteraction>> OnSelectInteraction;

        public override float CalculateCost()
        {
            return 20.0f;
        }

        protected override void ConfigureBehaviourTree()
        {
            var LocalRoot = AddChildToRootNode(new BTFlowNode_Sequence("Use Interactable")) as BTFlowNode_Sequence;

            LocalRoot.AddChild(new BTAction_SelectInteraction(SelectInteractionFn));
            LocalRoot.AddChild(new BTAction_CalculateMoveLocation(ValidNavMeshSearchRange, GetInteractableLocation));
            LocalRoot.AddChild(new BTAction_Move(StoppingDistance));
            LocalRoot.AddChild(new BTAction_UseInteractable());
        }

        protected override ECharacterResources GetRequiredResources()
        {
            return ECharacterResources.Legs | ECharacterResources.Torso;
        }

        protected override void PopulateSupportedGoalTypes()
        {
            SupportedGoalTypes = new System.Type[] { typeof(GOAPGoal_UseInteractable) };
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

        System.Tuple<SmartObject, BaseInteraction> SelectInteractionFn(GameObject InQuerier)
        {
            SmartObject TargetSO = null;
            BaseInteraction TargetInteraction = null;

            OnSelectInteraction.Invoke(Self, -1.0f, (SmartObject InTargetSO, BaseInteraction InTargetInteraction) =>
            {
                TargetSO = InTargetSO;
                TargetInteraction = InTargetInteraction;
            });

            return new System.Tuple<SmartObject, BaseInteraction>(TargetSO, TargetInteraction);
        }

        protected override void OnBehaviourTreeReset()
        {
            base.OnBehaviourTreeReset();

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
