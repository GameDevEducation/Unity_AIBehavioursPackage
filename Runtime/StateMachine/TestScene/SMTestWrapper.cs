using CommonCore;
using UnityEngine;

namespace StateMachine
{
    public class SMTestWrapper : MonoBehaviour, IDebuggableObject
    {
        public string DebugDisplayName => gameObject.name;

        SMInstance LinkedStateMachine = new();

        public void GetDebuggableObjectContent(IGameDebugger InDebugger, bool bInIsSelected)
        {
            LinkedStateMachine.GatherDebugData(InDebugger, bInIsSelected);
        }

        // Start is called before the first frame update
        void Start()
        {
            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                (InDebugger as IGameDebugger).RegisterSource(this);
            });

            var State1 = LinkedStateMachine.AddState(new SMState_WaitForTime(2f, 5f, "Wait 1"));
            var State2 = LinkedStateMachine.AddState(new SMState_WaitForTime(2f, 5f, "Wait 2"));
            var State3 = LinkedStateMachine.AddState(new SMState_WaitForTime(2f, 5f, "Wait 3"));

            State1.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State2);
            State2.AddTransition(new SMTransition_StateStatus(ESMStateStatus.Finished), State3);

            LinkedStateMachine.AddDefaultTransitions(State1, State1);
        }

        void OnDestroy()
        {
            ServiceLocator.AsyncLocateService<IGameDebugger>((ILocatableService InDebugger) =>
            {
                (InDebugger as IGameDebugger).UnregisterSource(this);
            });
        }

        // Update is called once per frame
        void Update()
        {
            LinkedStateMachine.Tick(Time.deltaTime);
        }
    }
}
