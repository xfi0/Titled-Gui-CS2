using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Menu.Types
{
    public class GrenadeLinup
    {
        public string Name = string.Empty;
        public string MapName = string.Empty;
        public GrenadeLaunchType LaunchType = GrenadeLaunchType.Still;
        public Vector3 Position = Vector3.Zero;
        public Vector3 Angle = Vector3.Zero;
        public Vector3 CircleDirection = Vector3.Zero;
    }
    public enum GrenadeLaunchType
    {
        Still = 0,
        Running,
        Jump,
        RunJump,
    }
}
