using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using static Titled_Gui.Classes.Colors;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class Tracers
    {
        public static bool enableTracers = false;
        public static bool TeamCheck = false;
        public static float LineThickness = 1f;
        public static string[] StartPositions = ["Middle", "Bottom", "Top"];
        public static string[] EndPositions = ["Bottom", "Top"];
        public static int CurrentStartPos = 0;
        public static int CurrentEndPos = 0;
        private static Vector2 StartPos = new();
        private static Vector2 EndPos = new();
        private static float HeadOffset = 50f;
        public static float RGBSpeed = 0.5f;
        public static void DrawTracers(Entity entity, Renderer renderer)
        {
            if (!enableTracers || entity.PawnAddress == LocalPlayer.PawnAddress || entity == null || (TeamCheck && entity.Team == LocalPlayer.Team) || BoxESP.FlashCheck && LocalPlayer.IsFlashed || entity?.Bones2D?.Count <= 0 || entity?.Position2D == new Vector2(-99, -99) || entity?.Bones2D == null) return;

            switch (CurrentStartPos)
            {
                case 0:
                    StartPos = new(renderer.screenSize.X / 2, renderer.screenSize.Y / 2);
                    break;
                case 1:
                    StartPos = new(renderer.screenSize.X / 2, renderer.screenSize.Y);
                    break;
                case 2:
                    StartPos = new(renderer.screenSize.X / 2, -renderer.screenSize.Y);
                    break;
            }
            switch (CurrentEndPos)
            {
                case 0: EndPos = entity.Position2D; break;
                case 1: EndPos = new(entity.Bones2D[2].X, entity.Bones2D[2].Y + HeadOffset); break;
            }

            Vector4 lineColor = RGB ? Colors.Rgb(RGBSpeed) : (LocalPlayer.Team == entity.Team ? TeamColor : EnemyColor);
            renderer.drawList.AddLine(StartPos, EndPos, ImGui.ColorConvertFloat4ToU32(lineColor), LineThickness); // add line for non rgb just liek Team color
        }
        public static void DrawTracerPreview(Vector2 position)
        {
            Vector2 bottom = new(position.X, position.Y + 200);
            ImGui.GetWindowDrawList().AddLine(bottom, position, ImGui.ColorConvertFloat4ToU32(Colors.EnemyColor));
        }
    }
}