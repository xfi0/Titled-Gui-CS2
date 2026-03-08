using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Menu
{
    internal class Types
    {
        public class GernadeHelperType
        {
            public string Name = string.Empty;
            public string MapName = string.Empty;
            public GernadeLaunchType LaunchType = GernadeLaunchType.Still;
            public Vector3 Position = Vector3.Zero;
            public Vector2 Angle = Vector2.Zero;
        }
        public enum GernadeLaunchType
        {
            Still = 0,
            Jump,
            RunJump,
        }
    }
}
