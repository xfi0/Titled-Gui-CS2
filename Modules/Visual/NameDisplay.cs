using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class NameDisplay
    {
        public static bool Enabled = false;
        public static float Offset = 100f;
        public static Vector4 NameTextColor = new(1, 1, 1, 1);
        public static void DrawName(Entity e, Renderer renderer)
        {
            if (e == null || e.Position2D == new Vector2(-99, -99) || e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed || e?.Bones2D == null || e?.Bones2D?.Count < 2 || e?.Bones2D?[2] == new Vector2(-99, -99)) return;
            var rect = BoxESP.GetBoxRect(e);
            if (rect != null)
            {
                var (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle) = rect.Value;

                Vector2 textPos = new(topRight.X + 12, topRight.Y);

                string name = (e?.Name ?? "").Split('\0')[0].Replace("?", "").Replace("\0", "");
                renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(NameTextColor), name);
            }
        }
        public static void DrawNamePreview(Vector2 position)
        {
            ImGui.GetWindowDrawList().AddText(position, ImGui.ColorConvertFloat4ToU32(NameTextColor), "John Doe");
        }
    }
}
