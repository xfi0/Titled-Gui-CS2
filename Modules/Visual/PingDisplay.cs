using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class PingDisplay
    {
        public static bool Enabled = false;
        public static Vector4 PingTextColor = new(1, 1, 1, 1);

        public static void DrawPing(Entity? e, Renderer renderer)
        {
            if (!Enabled || e == null || e.Position2D == new Vector2(-99, -99) ||
                e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e?.Bones == null || e.Bones.Count < 2 ||
                e.Bones[(int)BoneESP.BoneIds.Head].Position2D == new Vector2(-99, -99))
                return;

            var rect = BoxESP.GetBoxRect(e ?? GameState.LocalPlayer);
            if (rect == null)
                return;

            Vector2 textPos = new(rect.TopLeft.X - 12, rect.TopLeft.Y);

            string name = (e?.Ping.ToString() ?? "Unknown").Split('\0')[0].Replace("?", "").Replace("\0", "");
            renderer.DrawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(PingTextColor), name);

        }

        public static void DrawPingPreview(Vector2 position)
        {
            ImGui.GetWindowDrawList().AddText(position, ImGui.ColorConvertFloat4ToU32(PingTextColor), "69");
        }
    }
}
