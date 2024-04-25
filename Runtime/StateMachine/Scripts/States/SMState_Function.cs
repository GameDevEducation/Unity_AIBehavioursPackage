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

        protected override ESMStateStatus OnEnterInternal(Blackboard<FastName> InBlackboard)
        {
            return OnEnterInternalFn(InBlackboard);
        }

        protected override void OnExitInternal(Blackboard<FastName> InBlackboard)
        {
            OnExitInternalFn(InBlackboard);
        }

        protected override ESMStateStatus OnTickInternal(Blackboard<FastName> InBlackboard, float InDeltaTime)
        {
            return OnTickInternalFn(InBlackboard, InDeltaTime);
        }
    }
}
