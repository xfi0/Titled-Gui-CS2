using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Menu.Types
{
    public class HitText
    {
        public string? Text { get; set; }
        public DateTime ExpireAt { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 BasePosition { get; set; }
        public float State { get; set; } = 0f;
    }
    public enum HitAnimation
    {
        Sin = 0,
        Fade = 1,
    }
}
