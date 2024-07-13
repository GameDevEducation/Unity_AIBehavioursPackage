using CommonCore;
using System.Collections.Generic;

namespace CharacterCore
{
    public interface IInteraction : IDebuggable, IBlackboardStorable
    {
        bool IsMutuallyExclusive { get; }
        int MaxSimultaneousPerformers { get; }
        int CurrentPerformerCount { get; }

        IInteractable Owner { get; }

        List<IInteractionLookTarget> LookTargets { get; }
        List<IInteractionPoint> InteractionPoints { get; }

        void Bind(IInteractable InOwner);

        void Tick(float InDeltaTime);

        bool IsUsable();

        bool LockInteraction(IInteractionPerformer InPerformer, out IInteractionPoint OutFoundPoint);
        bool BeginInteraction(IInteractionPerformer InPerformer,
                              System.Action<IInteraction> InOnBeganCallbackFn = null,
                              System.Func<IInteraction, float, float, bool> InOnTickCallbackFn = null,
                              System.Action<IInteraction> InOnCompletedCallbackFn = null);
        bool UnlockInteraction(IInteractionPerformer InPerformer);
        bool AbandonInteraction(IInteractionPerformer InPerformer);
    }
}
