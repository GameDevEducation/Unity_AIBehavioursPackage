using CharacterCore;

namespace BehaviourTree
{
    public class BTAction_SendAnimationRequest : BTActionNodeBase
    {
        public override string DebugDisplayName { get; protected set; } = "Send Animation Request";

        bool bWaitForCompletion = false;
        AnimationRequest RequestData;
        System.Int32 CurrentRequestID = -1;
        bool bIsComplete = false;

        public BTAction_SendAnimationRequest(AnimationRequest InRequestData, bool bInWaitForCompletion = true, string InDisplayName = null)
        {
            if (!string.IsNullOrEmpty(InDisplayName))
                DebugDisplayName = InDisplayName;

            RequestData = InRequestData;
            bWaitForCompletion = bInWaitForCompletion;
        }

        protected override void OnEnter()
        {
            base.OnEnter();

            if ((OwningTree.AnimationInterface == null) || !RequestData.IsValid())
            {
                LastStatus = EBTNodeResult.Failed;
                return;
            }

            if (bWaitForCompletion)
            {
                bIsComplete = false;
                CurrentRequestID = OwningTree.AnimationInterface.IssueRequest(RequestData, OnAnimationCompleted);
                LastStatus = EBTNodeResult.InProgress;
            }
            else
            {
                bIsComplete = true;
                CurrentRequestID = -1;
                OwningTree.AnimationInterface.IssueRequest(RequestData);
                LastStatus = EBTNodeResult.Succeeded;
            }
        }

        protected void OnAnimationCompleted(System.Int32 InRequestID, EAnimationCompletionReason InReason)
        {
            if (InRequestID == CurrentRequestID)
            {
                bIsComplete = true;
                CurrentRequestID = -1;
            }
        }

        protected override bool OnTick_NodeLogic(float InDeltaTime)
        {
            if (LastStatus != EBTNodeResult.InProgress)
                return SetStatusAndCalculateReturnValue(LastStatus);

            return SetStatusAndCalculateReturnValue(bIsComplete ? EBTNodeResult.Succeeded : EBTNodeResult.InProgress);
        }

        protected override void OnExit()
        {
            base.OnExit();

            if ((CurrentRequestID >= 0) && (OwningTree.AnimationInterface != null))
            {
                OwningTree.AnimationInterface.CancelRequest(CurrentRequestID);
                CurrentRequestID = -1;
            }
        }
    }
}
