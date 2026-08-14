using ImGuiNET;
using Microsoft.VisualBasic.Logging;
using System.Net.WebSockets;
using System.Numerics;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;
using TextRenderer = Titled_Gui.Classes.Rendering.TextRenderer;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar : IModule
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;
        public static float HealthBarWidth = 5f;
        public static float Rounding = 0;
        public static Vector4 HealthBarBackGround = new(0.2f, 0.2f, 0.2f, 1f);
        public static Vector4 HealthColorStart = new(0, 1, 0, 1);
        public static Vector4 HealthColorEnd = new(1, 0, 0, 1);
        public static Colors HealthColor = new(primaryColor: HealthColorStart, secondaryColor: HealthColorEnd);
        private static Vector4 _outlineColor = new(0f, 0f, 0f, 1f);
        private static int _outlineThickness = 1;
        private static int _paddingX = 3;

        public static void DrawHealthBar(Entity? e, float health, float maxHealth, BoxRect rect)
        {
            if (!EnableHealthBar || e == null || e.Health <= 0 || GameState.LocalPlayer == null || GameState.renderer == null || (!DrawOnSelf && e.PawnAddress == GameState.LocalPlayer.PawnAddress) || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) || e.Position2D == new Vector2(-99, -99)) return;

            float height = rect.BottomRight.Y - rect.TopLeft.Y;
            float filledHeight = height * Math.Clamp(health / maxHealth, 0f, 1f);

            Vector2 top = new(rect.TopLeft.X - _paddingX - HealthBarWidth, rect.TopLeft.Y);
            Vector2 bottom = new(rect.TopLeft.X - _paddingX, rect.TopLeft.Y + height);

            DrawHealthBarInternal(GameState.renderer.DrawList, top, bottom, filledHeight, GetHealthColor());
        }

        private static (Vector4, Vector4) GetHealthColor()
        {
            var primary = HealthColor.PrimaryRGB ? Colors.Rgb(HealthColor.PrimaryColor.W) : HealthColor.PrimaryColor;
            var secondary = HealthColor.SecondaryRGB ? Colors.Rgb(HealthColor.SecondaryColor.W) : HealthColor.SecondaryColor;

            return (primary, secondary);
        }

        private static void DrawHealthBarInternal(ImDrawListPtr drawList, Vector2 top, Vector2 bottom, float filledHeight, (Vector4, Vector4) color)
        {
            drawList.AddRectFilled(new(top.X - _outlineThickness, top.Y - _outlineThickness), new(bottom.X + _outlineThickness, bottom.Y + _outlineThickness), ImGui.ColorConvertFloat4ToU32(_outlineColor), Rounding);

            drawList.AddRectFilled(top, bottom, ImGui.ColorConvertFloat4ToU32(HealthBarBackGround), Rounding);

            Vector2 filledTop = new(top.X, bottom.Y - filledHeight);
            ShapeRenderer.DrawGradientRect(drawList, filledTop, bottom, color.Item1, HealthColorEnd);
        }

        public static void DrawHealthBarPreview(Vector2 position, float entityHeight)
        {
            if (GameState.renderer == null)
                return;

            float barWidth = 5f;
            float healthPercent = TextRenderer.AnimateFloat("HealthBar", 0.8f);
            float offset = 4;

            Vector2 top = position + new Vector2(-entityHeight / 3f - barWidth - offset, -entityHeight / 2);
            Vector2 bottom = position + new Vector2(-entityHeight / 3f - offset, entityHeight / 2);
            var color = GetHealthColor();

            DrawHealthBarInternal(ImGui.GetWindowDrawList(), top, bottom, entityHeight * healthPercent, color);
        }
    }
}