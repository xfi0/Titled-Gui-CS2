using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Entity.Types
{
    public class BoxRect(Vector2 topLeft, Vector2 bottomRight, Vector2 topRight, Vector2 bottomLeft, Vector2 bottomMiddle)
    {
        public Vector2 TopLeft { get; set; } = topLeft;

        public Vector2 BottomRight { get; set; } = bottomRight;

        public Vector2 TopRight { get; set; } = topRight;

        public Vector2 BottomLeft { get; set; } = bottomLeft;

        public Vector2 BottomMiddle { get; set; } = bottomMiddle;
    }
}
