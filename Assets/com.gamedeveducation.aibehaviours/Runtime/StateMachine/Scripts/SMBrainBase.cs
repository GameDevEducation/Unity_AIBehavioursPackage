using CharacterCore;
using CommonCore;
using UnityEngine;

namespace StateMachine
{
    public abstract class SMBrainBase : MonoBehaviour, ISMBrain
    {
        class InternalState_StateMachineFinished : SMStateBase
        {
            System.Action CallbackFn;

            internal InternalState_StateMachineFinished(System.Action InCallbackFn) :
                base(null)
            {
                CallbackFn = InCallbackFn;
            }

            protected override ESMStateStatus OnEnterInternal()
            {
                CallbackFn();

                return ESMStateStatus.Finished;
            }

            protected override void OnExitInternal()
            {
            }

            protected override ESMStateStatus OnTickInternal(float InDeltaTime)
            {
                return ESMStateStatus.Finished;
            }
        }

        [SerializeField] float ResourceCapacity = 50.0f;

        public Blackboard<FastName> LinkedBlackboard { get; protected set; }
        public INavigationInterface Navigation { get; protected set; }
        public ILookHandler LookAtHandler { get; protected set; }

        protected ISMInstance LinkedStateMachine = new SMInstance();

        protected ISMState InternalState_Failed { get; private set; }
        protected ISMState InternalState_Finished { get; private set; }

        public string DebugDisplayName => gameObject.name;

        public GameObject Self => gameObject;

        void Start()
        {
            InternalState_Failed = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Failed);
            InternalState_Finished = new InternalState_StateMachineFinished(InternalOnStateMachineCompleted_Finished);

            ServiceLocator.AsyncLocateService<INavigationInterface>((ILocatableService InService) =>
            {
                Navigation = InService as INavigationInterface;
            }, gameObject, EServiceSearchMode.LocalOnly);
            ServiceLocator.AsyncLocateService<ILookHandler>((ILocatableService InService) =>
            {
                LookAtHandler = (ILookHandler)InService;
            }, gameObject, EServiceSearchMode.LocalOnly);

            LinkedBlackboard = BlackboardManager.GetIndividualBlackboard(this);

            LinkedBlackboard.Set(CommonCore.Names.Self, gameObject);

            LinkedBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);
            LinkedBlackboard.Set(CommonCore.Names.HomeLocation, transform.position);

            LinkedBlackboard.Set(CommonCore.Names.MoveToLocation, CommonCore.Constants.InvalidVector3Position);

            LinkedBlackboard.Set(CommonCore.Names.Target_GameObject, (GameObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Target_Position, CommonCore.Constants.InvalidVector3Position);

            LinkedBlackboard.Set(CommonCore.Names.Awareness_PreviousBestTarget, (GameObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Awareness_BestTarget, (GameObject)null);

            LinkedBlackboard.Set(CommonCore.Names.LookAt_GameObject, (GameObject)null);
            LinkedBlackboard.Set(CommonCore.Names.LookAt_Position, CommonCore.Constants.InvalidVector3Position);

            LinkedBlackboard.Set(CommonCore.Names.Interaction_SmartObject, (SmartObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Interaction_Type, (BaseInteraction)null);

            LinkedBlackboard.SetGeneric(CommonCore.Names.Resource_FocusType, CommonCore.Resources.EType.Unknown);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusSource, (GameObject)null);
            LinkedBlackboard.Set(CommonCore.Names.Resource_FocusStorage, (GameObject)null);

            // populate the inventory
            var ResourceNames = System.Enum.GetNames(typeof(CommonCore.Resources.EType));
            foreach (var ResourceName in ResourceNames)
            {
                if (ResourceName == CommonCore.Resources.EType.Unknown.ToString())
                    continue;

                LinkedBlackboard.Set(new FastName($"Self.Inventory.{ResourceName}.Held"), 0f);
                LinkedBlackboard.Set(new FastName($"Self.Inventory.{ResourceName}.Capacity"), ResourceCapacity);
            }

            ConfigureBlackboard();

            LinkedStateMachine.BindToBlackboard(LinkedBlackboard);

            ConfigureFSM();

            LinkedStateMachine.Reset();

            ConfigureBrain();

            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                (InDebugger as IGameDebugger).RegisterSource(this);
            });

            ServiceLocator.RegisterService(LinkedBlackboard, gameObject);
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
            LinkedBlackboard.Set(CommonCore.Names.CurrentLocation, transform.position);

            TickBrain(Time.deltaTime);
        }

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            if (!bInIsSelected)
                return;

            LinkedStateMachine.GatherDebugData(InDebugger, bInIsSelected);
            LinkedBlackboard.GatherDebugData(InDebugger, bInIsSelected);
        }

        protected void TickBrain(float InDeltaTime)
        {
            OnPreTickBrain(InDeltaTime);

            LinkedStateMachine.Tick(InDeltaTime);

            OnPostTickBrain(InDeltaTime);
        }

        protected void ResetStateMachine()
        {
            LinkedStateMachine.Reset();
            OnStateMachineReset();
        }

        void InternalOnStateMachineCompleted_Failed()
        {
            ResetStateMachine();

            OnStateMachineCompleted_Failed();
        }

        void InternalOnStateMachineCompleted_Finished()
        {
            ResetStateMachine();

            OnStateMachineCompleted_Finished();
        }

        protected virtual void OnStateMachineCompleted_Failed() { }
        protected virtual void OnStateMachineCompleted_Finished() { }
        protected virtual void OnStateMachineReset() { }

        protected ISMState AddState(ISMState InState)
        {
            return LinkedStateMachine.AddState(InState);
        }

        protected void AddDefaultTransitions(ISMState InFinishedState = null, ISMState InFailedState = null)
        {
            LinkedStateMachine.AddDefaultTransitions(InFinishedState == null ? InternalState_Finished : InFinishedState,
                                                     InFailedState == null ? InternalState_Failed : InFailedState);
        }

        protected abstract void ConfigureBlackboard();
        protected abstract void ConfigureFSM();
        protected abstract void ConfigureBrain();

        protected virtual void OnBehaviourTreeCompleted_Finished() { }
        protected virtual void OnBehaviourTreeCompleted_Failed() { }
        protected virtual void OnBehaviourTreeReset() { }

        protected virtual void OnPreTickBrain(float InDeltaTime) { }
        protected virtual void OnPostTickBrain(float InDeltaTime) { }
    }
}
