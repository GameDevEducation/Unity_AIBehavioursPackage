using UnityEngine;

namespace CommonCore
{
    public static class Constants
    {
        public static readonly Vector2 InvalidVector2Position = new(float.MaxValue, float.MaxValue);
        public static readonly Vector3 InvalidVector3Position = new(float.MaxValue, float.MaxValue, float.MaxValue);
    }

    public static class Names
    {
        public static readonly FastName Self = new("Self");

        public static readonly FastName CurrentLocation = new("Self.Transform.Position");
        public static readonly FastName HomeLocation = new("Self.Home.Position");
        public static readonly FastName MoveToLocation = new("Self.Navigation.MoveToLocation");

        public static readonly FastName Target_GameObject = new("Self.Target.GameObject");
        public static readonly FastName Target_Position = new("Self.Target.Position");

        public static readonly FastName Awareness_PreviousBestTarget = new("Self.Awareness.PreviousBestTarget.GameObject");
        public static readonly FastName Awareness_BestTarget = new("Self.Awareness.BestTarget.GameObject");

        public static readonly FastName LookAt_GameObject = new("Self.LookAt.GameObject");

        public static readonly FastName Interaction_SmartObject = new("Self.Interaction.SmartObject");
        public static readonly FastName Interaction_Type = new("Self.Interaction.Type");

        public static readonly FastName Resource_FocusType = new("Self.Resources.Focus.Type");
        public static readonly FastName Resource_FocusSource = new("Self.Resources.Focus.Source");
        public static readonly FastName Resource_FocusStorage = new("Self.Resources.Focus.Storage");
    }
}
