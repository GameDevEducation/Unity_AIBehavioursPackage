using CommonCore;
using System.Collections.Generic;

namespace CharacterCore
{
    public enum EParameterUndoMode
    {
        UseRequestDefault,
        AlwaysUndo,
        AlwaysLeave
    }

    [System.Serializable]
    public abstract class AnimationParameter
    {
        public string Name;
        public EParameterUndoMode UndoOnCancellation = EParameterUndoMode.UseRequestDefault;
        public EParameterUndoMode UndoOnInterruption = EParameterUndoMode.UseRequestDefault;
    }

    [System.Serializable]
    public class BooleanParameter : AnimationParameter
    {
        public bool Value;
    }

    [System.Serializable]
    public class IntegerParameter : AnimationParameter
    {
        public int Value;
    }

    [System.Serializable]
    public class FloatParameter : AnimationParameter
    {
        public float Value;
    }

    [System.Serializable]
    public class TriggerParameter : AnimationParameter
    {
    }

    [System.Serializable]
    public class AnimationRequest
    {
        public string StateName;
        public List<BooleanParameter> BooleanParameters;
        public List<IntegerParameter> IntegerParameters;
        public List<FloatParameter> FloatParameters;
        public List<TriggerParameter> TriggerParameters;
        public bool UndoOnCompletion = true;
        public bool UndoOnInterruption = true;

        public bool IsValid()
        {
            if (!string.IsNullOrEmpty(StateName))
                return true;

            foreach (var Parameter in BooleanParameters)
            {
                if (!string.IsNullOrEmpty(Parameter.Name))
                    return true;
            }
            foreach (var Parameter in IntegerParameters)
            {
                if (!string.IsNullOrEmpty(Parameter.Name))
                    return true;
            }
            foreach (var Parameter in FloatParameters)
            {
                if (!string.IsNullOrEmpty(Parameter.Name))
                    return true;
            }
            foreach (var Parameter in TriggerParameters)
            {
                if (!string.IsNullOrEmpty(Parameter.Name))
                    return true;
            }

            return false;
        }
    }

    public enum EAnimationCompletionReason
    {
        Finished,
        Interrupted
    }

    public interface IAnimationInterface : ILocatableService
    {
        void PushBool(string InName, bool bInValue);
        void PushInteger(string InName, int InValue);
        void PushFloat(string InName, float InValue);

        void SendTrigger(string InName);
        void ResetTrigger(string InName);

        void SetState(string InStateName);

        System.Int32 IssueRequest(AnimationRequest InRequestData, System.Action<System.Int32, EAnimationCompletionReason> InOnFinishedCallbackFn = null);
        void CancelRequest(System.Int32 InRequestID);
    }
}
