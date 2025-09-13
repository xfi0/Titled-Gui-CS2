using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Visual
{
    internal class NameDisplay
    {
        public static bool Enabled = false;
        public static void DrawName(Entity e, Renderer renderer)
        {
            if (e != null && e.Position2D != new Vector2(-99, -99))
            {
                Vector2 textPos = new(e.Position2D.X, e.Position2D.Y); // i cant find a good position
                renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), e?.Name);
            }
        }
    }
}
