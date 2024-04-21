using CommonCore;
using UnityEngine;

namespace BehaviourTree
{
    public abstract class BTBrainBase : MonoBehaviour, IBTBrain
    {
        [SerializeField] float ResourceCapacity = 50.0f;

        public Blackboard<FastName> CurrentBlackboard { get; protected set; }
        public INavigationInterface Navigation { get; protected set; }

        public IBehaviourTree LinkedBehaviourTree { get; protected set; } = new BTInstance();

        public string DebugDisplayName => gameObject.name;

        public GameObject Self => gameObject;

        void Start()
        {
            ServiceLocator.AsyncLocateService<INavigationInterface>((ILocatableService InService) =>
            {
                Navigation = InService as INavigationInterface;
            }, gameObject, EServiceSearchMode.LocalOnly);

            CurrentBlackboard = BlackboardManager.GetIndividualBlackboard(this);

            CurrentBlackboard.Set(CommonCore.Names.Self, gameObject);

            CurrentBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);
            CurrentBlackboard.Set(CommonCore.Names.HomeLocation, transform.position);

            CurrentBlackboard.Set(CommonCore.Names.MoveToLocation, CommonCore.Constants.InvalidVector3Position);

            CurrentBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Target_Position, CommonCore.Constants.InvalidVector3Position);

            CurrentBlackboard.Set(CommonCore.Names.Awareness_PreviousBestTarget, (GameObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Awareness_BestTarget, (GameObject)null);

            CurrentBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);

            CurrentBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            CurrentBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, CommonCore.Resources.EType.Unknown);
            CurrentBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);
            CurrentBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);

            // populate the inventory
            var ResourceNames = System.Enum.GetNames(typeof(CommonCore.Resources.EType));
            foreach (var ResourceName in ResourceNames)
            {
                if (ResourceName == CommonCore.Resources.EType.Unknown.ToString())
                    continue;

                CurrentBlackboard.Set(new FastName($"Self.Inventory.{ResourceName}.Held"), 0f);
                CurrentBlackboard.Set(new FastName($"Self.Inventory.{ResourceName}.Capacity"), ResourceCapacity);
            }

            ConfigureBlackboard();

            LinkedBehaviourTree.Initialise(Navigation, CurrentBlackboard);

            ConfigureBehaviourTree();

            ResetBehaviourTree();

            ConfigureBrain();

            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                (InDebugger as IGameDebugger).RegisterSource(this);
            });

            ServiceLocator.RegisterService(CurrentBlackboard, gameObject);
        }

        void OnDestroy()
        {
            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                (InDebugger as IGameDebugger).UnregisterSource(this);
            });
        }

        void Update()
        {
            CurrentBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);

            TickBrain(Time.deltaTime);
        }

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            CurrentBlackboard.GatherDebugData(InDebugger, bInIsSelected);
            LinkedBehaviourTree.GatherDebugData(InDebugger, bInIsSelected);
        }

        protected void TickBrain(float InDeltaTime)
        {
            OnPreTickBrain(InDeltaTime);

            var Result = LinkedBehaviourTree.Tick(InDeltaTime);

            if (Result == EBTNodeResult.Succeeded)
            {
                ResetBehaviourTree();
                OnBehaviourTreeCompleted_Finished();
            }
            else if (Result == EBTNodeResult.Failed)
            {
                ResetBehaviourTree();
                OnBehaviourTreeCompleted_Failed();
            }

            OnPostTickBrain(InDeltaTime);
        }

        protected void ResetBehaviourTree()
        {
            LinkedBehaviourTree.Reset();
            OnBehaviourTreeReset();
        }

        protected IBTNode AddChildToRootNode(IBTNode InNode)
        {
            return LinkedBehaviourTree.AddChildToRootNode(InNode);
        }

        protected abstract void ConfigureBlackboard();
        protected abstract void ConfigureBehaviourTree();
        protected abstract void ConfigureBrain();

        protected virtual void OnBehaviourTreeCompleted_Finished() { }
        protected virtual void OnBehaviourTreeCompleted_Failed() { }
        protected virtual void OnBehaviourTreeReset() { }

        protected virtual void OnPreTickBrain(float InDeltaTime) { }
        protected virtual void OnPostTickBrain(float InDeltaTime) { }
    }
}
