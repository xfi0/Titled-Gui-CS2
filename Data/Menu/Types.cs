using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Menu
{
    public class Types
    {
        public class GernadeLineupType
        {
            public string Name = string.Empty;
            public string MapName = string.Empty;
            public GernadeLaunchType LaunchType = GernadeLaunchType.Still;
            public Vector3 Position = Vector3.Zero;
            public Vector3 Angle = Vector3.Zero;
            public Vector3 CircleDirection = Vector3.Zero;
        }
        public enum GernadeLaunchType
        {
            Still = 0,
            Running,
            Jump,
            RunJump,
        }

        public class BoxRect(Vector2 topLeft, Vector2 bottomRight, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomMiddle)
        {
            public Vector2 TopLeft { get; set; } = topLeft;

            public Vector2 BottomRight { get; set; } = bottomRight;

            public Vector2 TopRight { get; set; } = topRight;

            public Vector2 BottomLeft { get; set; } = bottomLeft;

            public Vector2 BottomMiddle { get; set; } = bottomMiddle;
        }
    }
}
