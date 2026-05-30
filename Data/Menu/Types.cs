using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Menu
{
    internal class Types
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
    }
}
