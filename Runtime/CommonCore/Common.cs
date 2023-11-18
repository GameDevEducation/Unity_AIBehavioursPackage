using System.Collections;
using System.Collections.Generic;
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
        public static readonly FastName MoveToLocation = new("Self.Navigation.MoveToLocation");
    }
}
