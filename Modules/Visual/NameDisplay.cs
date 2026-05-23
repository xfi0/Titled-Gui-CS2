using ImGuiNET;
using System.Numerics;
using System.Xml.Linq;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class NameDisplay
    {
        public static bool Enabled = false;
        public static float Offset = 100f;
        public static Vector4 NameTextColor = new(1, 1, 1, 1);

        public static void DrawName(Entity? e, Renderer renderer)
        {
            if (!Enabled || e == null || e.Position2D == new Vector2(-99, -99) ||
                e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 ||
                BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed || e?.Bones == null || e?.Bones?.Count < 2 ||
                e?.Bones?[(int)BoneESP.BoneIds.Head].Position2D == new Vector2(-99, -99))
                return;

            var rect = BoxESP.GetBoxRect(e);

            if (rect == null)
                return;

            var (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle) = rect.Value;
            float offsetY = 20f;
            string name = (e?.Name ?? "").Split('\0')[0].Replace("?", "").Replace("\0", "");

            Vector2 textSize = ImGui.CalcTextSize(name);
            Vector2 textPos = new(bottomMiddle.X - textSize.X, topRight.Y - offsetY);

            renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(NameTextColor), name);

        }

        public static void DrawNamePreview(Vector2 position)
        {
            ImGui.GetWindowDrawList().AddText(position, ImGui.ColorConvertFloat4ToU32(NameTextColor), "John Doe");
        }
    }
}
