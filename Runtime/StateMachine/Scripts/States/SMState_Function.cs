using CommonCore;

namespace StateMachine
{
    public class SMState_Function : SMStateBase
    {
        System.Func<Blackboard<FastName>, ESMStateStatus> OnEnterInternalFn;
        System.Func<Blackboard<FastName>, float, ESMStateStatus> OnTickInternalFn;
        System.Action<Blackboard<FastName>> OnExitInternalFn;

        public SMState_Function(System.Func<Blackboard<FastName>, ESMStateStatus> InOnEnterInternalFn,
                                System.Func<Blackboard<FastName>, float, ESMStateStatus> InOnTickInternalFn,
                                System.Action<Blackboard<FastName>> InOnExitInternalFn,
                                string InDisplayName = null) :
            base(InDisplayName)
        {
            OnEnterInternalFn = InOnEnterInternalFn;
            OnTickInternalFn = InOnTickInternalFn;
            OnExitInternalFn = InOnExitInternalFn;
        }

        protected override ESMStateStatus OnEnterInternal()
        {
            return OnEnterInternalFn(LinkedBlackboard);
        }

        protected override void OnExitInternal()
        {
            OnExitInternalFn(LinkedBlackboard);
        }

        protected override ESMStateStatus OnTickInternal(float InDeltaTime)
        {
            return OnTickInternalFn(LinkedBlackboard, InDeltaTime);
        }
    }
}
