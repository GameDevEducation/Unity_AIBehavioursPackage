using CommonCore;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCore
{
    [System.Serializable]
    public class AnimatorBridgeAnimationSetEntry
    {
        public string StateName;
    }

    [System.Serializable]
    public class AnimatorBridgeAnimationSet
    {
        public string ExternalName;
        public List<AnimatorBridgeAnimationSetEntry> Animations;
    }

    [AddComponentMenu("Character/Animation: Animator Bridge")]
    public class AnimatorBridge : MonoBehaviour, IAnimationInterface
    {
        [SerializeField] Animator LinkedAnimator;
        [SerializeField] List<AnimatorBridgeAnimationSet> AnimationSets;
        [SerializeField] string IdleState = "Idle";

        Dictionary<string, List<AnimatorBridgeAnimationSetEntry>> AnimationMap = new();
        System.Action<System.Int32, EAnimationCompletionReason> OnFinishedCallbackFn = null;
        AnimationRequest CurrentRequest = null;
        string SelectedAnimationState = null;
        int NextRequestID = 0;

        void Awake()
        {
            ServiceLocator.RegisterService<IAnimationInterface>(this, gameObject);

            // build the animation map
            foreach (var Set in AnimationSets)
            {
                AnimationMap[Set.ExternalName] = Set.Animations;
            }

            // Credit for this logic to Dejan Ignjatovic: https://gamedev.stackexchange.com/a/187958

            // setup begin and end event notifications
            for (int ClipIndex = 0; ClipIndex < LinkedAnimator.runtimeAnimatorController.animationClips.Length; ++ClipIndex)
            {
                AnimationClip Clip = LinkedAnimator.runtimeAnimatorController.animationClips[ClipIndex];

                AnimationEvent StartEvent = new AnimationEvent();
                StartEvent.time = 0;
                StartEvent.functionName = "OnAnimationStart";
                StartEvent.stringParameter = Clip.name;
                Clip.AddEvent(StartEvent);

                AnimationEvent FinishEvent = new AnimationEvent();
                FinishEvent.time = Clip.length - (1.0f / Clip.frameRate);
                FinishEvent.functionName = "OnAnimationFinish";
                FinishEvent.stringParameter = Clip.name;
                Clip.AddEvent(FinishEvent);
            }
        }

        #region IAnimationInterface
        public void PushBool(string InName, bool bInValue)
        {
            LinkedAnimator.SetBool(InName, bInValue);
        }

        public void PushInteger(string InName, int InValue)
        {
            LinkedAnimator.SetInteger(InName, InValue);
        }

        public void PushFloat(string InName, float InValue)
        {
            LinkedAnimator.SetFloat(InName, InValue);
        }

        public void SendTrigger(string InName)
        {
            LinkedAnimator.SetTrigger(InName);
        }

        public void ResetTrigger(string InName)
        {
            LinkedAnimator.ResetTrigger(InName);
        }

        public void SetState(string InStateName)
        {
            LinkedAnimator.Play(InStateName);
        }

        public System.Int32 IssueRequest(AnimationRequest InRequestData, System.Action<System.Int32, EAnimationCompletionReason> InOnFinishedCallbackFn = null)
        {
            if (CurrentRequest != null)
                CancelRequest(NextRequestID - 1);

            int IssuedID = NextRequestID;
            ++NextRequestID;

            OnFinishedCallbackFn = InOnFinishedCallbackFn;
            CurrentRequest = InRequestData;

            // pick the animation if one is provided
            SelectedAnimationState = null;
            if (!string.IsNullOrEmpty(InRequestData.StateName))
            {
                List<AnimatorBridgeAnimationSetEntry> CandidateAnimations;
                if (AnimationMap.TryGetValue(InRequestData.StateName, out CandidateAnimations))
                {
                    if (CandidateAnimations.Count > 0)
                    {
                        SelectedAnimationState = CandidateAnimations[Random.Range(0, CandidateAnimations.Count)].StateName;
                    }
                }
            }

            // Set the state if found
            if (!string.IsNullOrEmpty(SelectedAnimationState))
                SetState(SelectedAnimationState);

            // push parameters and triggers
            foreach (var Parameter in InRequestData.BooleanParameters)
                PushBool(Parameter.Name, Parameter.Value);
            foreach (var Parameter in InRequestData.IntegerParameters)
                PushInteger(Parameter.Name, Parameter.Value);
            foreach (var Parameter in InRequestData.FloatParameters)
                PushFloat(Parameter.Name, Parameter.Value);
            foreach (var Parameter in InRequestData.TriggerParameters)
                SendTrigger(Parameter.Name);

            return IssuedID;
        }

        public void CancelRequest(System.Int32 InRequestID)
        {
            if (CurrentRequest == null)
                return;

            Cleanup(InRequestID, EAnimationCompletionReason.Interrupted);
        }

        #endregion

        void Cleanup(System.Int32 InRequestID, EAnimationCompletionReason InReason)
        {
            if (OnFinishedCallbackFn != null)
            {
                OnFinishedCallbackFn(InRequestID, InReason);
                OnFinishedCallbackFn = null;
            }

            if (CurrentRequest != null)
            {
                bool bUndoByDefault = InReason == EAnimationCompletionReason.Finished ? CurrentRequest.UndoOnCompletion : CurrentRequest.UndoOnInterruption;

                if (bUndoByDefault)
                    SetState(IdleState);

                foreach (var Parameter in CurrentRequest.BooleanParameters)
                {
                    EParameterUndoMode UndoMode = InReason == EAnimationCompletionReason.Finished ? Parameter.UndoOnCancellation : Parameter.UndoOnInterruption;

                    if (UndoMode == EParameterUndoMode.AlwaysLeave)
                        continue;

                    if ((UndoMode == EParameterUndoMode.AlwaysUndo) || !bUndoByDefault)
                        PushBool(Parameter.Name, !Parameter.Value);
                }
                foreach (var Parameter in CurrentRequest.TriggerParameters)
                {
                    EParameterUndoMode UndoMode = InReason == EAnimationCompletionReason.Finished ? Parameter.UndoOnCancellation : Parameter.UndoOnInterruption;

                    if (UndoMode == EParameterUndoMode.AlwaysLeave)
                        continue;

                    if ((UndoMode == EParameterUndoMode.AlwaysUndo) || !bUndoByDefault)
                        ResetTrigger(Parameter.Name);
                }
            }
            else
                SetState(IdleState);

            CurrentRequest = null;
            SelectedAnimationState = null;
        }

        public void OnAnimationStart(string InStateName)
        {
        }

        public void OnAnimationFinish(string InStateName)
        {
            if ((OnFinishedCallbackFn != null) && (CurrentRequest != null) && (InStateName == SelectedAnimationState))
            {
                Cleanup(NextRequestID - 1, EAnimationCompletionReason.Finished);
            }
        }
    }
}
