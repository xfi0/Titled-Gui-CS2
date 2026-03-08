using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class DistanceText
    {
        public static void DrawDistance(Entity? e)
        {
            if (e == null || (BoxESP.TeamCheck && e?.Team == GameState.LocalPlayer.Team) || e?.Health <= 0 ||
                e?.PawnAddress == GameState.LocalPlayer.PawnAddress || e?.Distance == null ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e.Position2D == new Vector2(-99, -99)) return;

            string distText = $"{(int)e.Distance / 100}m";
            Vector2 textPos = new(e.Position2D.X + 2, e.Position2D.Y);
            GameState.renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new(1f, 1f, 1f, 1f)), distText);
        }
    }
}
