using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.C4;

namespace Titled_Gui.Modules.Visual
{
    internal class C4ESP
    {
        public static bool BoxEnabled = false;
        public static bool TextEnabled = false;
        public static Vector4 BoxColor = new(1, 1, 1, 1);
        public static Vector4 TextColor = new(1, 1, 1, 1);

        public static void DrawESP()
        {
            if (!BoxEnabled && !TextEnabled) return;

            Types.C4? c4 = C4Info.C4;

            if (c4 == null || !c4.Planted || c4.Position == new Vector3(0, 0, 0) || c4.Position2D == new Vector2(-99, -99))
                return;

            if (TextEnabled)
                GameState.renderer.drawList.AddText(c4.Position2D, ImGui.ColorConvertFloat4ToU32(TextColor), "C4");

            if (BoxEnabled)
            {
                GameState.renderer.drawList.AddRect(c4.Position2D, new(
                    c4.Position2D.X + 10 *
                    (float)Math.Clamp(Vector2.Distance(c4.Position2D, GameState.LocalPlayer.Position2D), 1.5, 10),
                    c4.Position2D.Y + 10), ImGui.ColorConvertFloat4ToU32(BoxColor));
            }
        }

        //private static Vector3[] Get3DCorners(Vector3 position, float[] matrix)
        //{
           
        //}
    }
}
