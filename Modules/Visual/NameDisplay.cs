using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class NameDisplay
    {
        public static bool Enabled = false;
        public static float Offset = 100f;

        public static void DrawName(Entity e, Renderer renderer)
        {
            if (e != null && e.Position2D != new Vector2(-99, -99) || (e?.PawnAddress != GameState.localPlayer.PawnAddress) || e.Bones2D == null || e.Health == 0 || (BoxESP.FlashCheck && !GameState.localPlayer.IsFlashed))
            {
                //Offset / (e.Distance * 0.1f), 50f, 60f
                float BestDistance = 225f; // looks good imo
                float OffsetX = Math.Clamp(Offset * (BestDistance / Math.Max(e!.Distance, 1f)), 50f, 60f);

                Vector2 textPos = new(e.Bones2D[2].X + OffsetX, e.Bones2D[2].Y);
                string name = (e?.Name ?? "").Split('\0')[0].Replace("?", "").Replace("\0", "");

                renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), name);

            }
        }
    }
}
