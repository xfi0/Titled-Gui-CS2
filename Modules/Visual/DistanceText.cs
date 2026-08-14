using ImGuiNET;
using System.Numerics;
using System.Xml.Linq;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Modules.Legit.HitStuff;

namespace Titled_Gui.Modules.Visual
{
    internal class DistanceText : IModule
    {
        public static bool Enabled = false;
        public static void DrawDistance(Entity? e)
        {
            if (!Enabled || e == null || GameState.LocalPlayer == null || GameState.renderer == null ||
                (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) || e.Health <= 0 || e.PawnAddress == GameState.LocalPlayer.PawnAddress
                || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e.Position2D == new Vector2(-99, -99))
                return;

            string distText = $"{(int)e.Distance / 100}m";
            Vector2 textSize = ImGui.CalcTextSize(distText);

            Vector2 textPos = new(e.Position2D.X + 2 - (textSize.X / 2), e.Position2D.Y);
            GameState.renderer.DrawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new(1f, 1f, 1f, 1f)), distText);
        }
        public static void DrawDistancePreview(Vector2 position, float entityHeight)
        {
            string distText = "15m";

            Vector2 textSize = ImGui.CalcTextSize(distText);

            ImGui.GetWindowDrawList().AddText(position + new Vector2(0 - (textSize.X / 2), entityHeight / 2),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), "15m");
        }

    }
}
