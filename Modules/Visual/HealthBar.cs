using ImGuiNET;
using Microsoft.VisualBasic.Logging;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using TextRenderer = Titled_Gui.Classes.Rendering.TextRenderer;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;
        public static float HealthBarWidth = 5f;
        public static float Rounding = 2.3f;
        public static Vector4 HealthBarBackGround = new(0.2f, 0.2f, 0.2f, 1f);
        public static Vector4 HealthColor = new(0, 1, 0, 1);
        public static bool RGB = false;

        public static void DrawHealthBar(Entity? e, float health, float maxHealth, Vector2 topLeft, float height)
        {
            if (!EnableHealthBar || e == null || e.Health <= 0 || GameState.LocalPlayer == null || GameState.renderer == null || (!DrawOnSelf && e.PawnAddress == GameState.LocalPlayer.PawnAddress) || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) || e.Position2D == new Vector2(-99, -99)) return;

            float healthPercentage = Math.Clamp(health / maxHealth, 0f, 1f); // percentage of the box that is currently filled
            float filledHeight = height * healthPercentage;

            GameState.renderer.DrawList.AddRectFilled(topLeft, topLeft + new Vector2(HealthBarWidth, height), ImGui.ColorConvertFloat4ToU32(HealthBarBackGround), Rounding);

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);

            if (RGB)
                HealthColor = Colors.Rgb(HealthColor.W);

            else
            {
                if (e.Health > 80)
                    HealthColor = new(0f, 1f, 0f, 1f);

                else if (e.Health > 50)
                {
                    float t = (80 - e.Health) / 30f;
                    HealthColor = Vector4.Lerp(new(0f, 1f, 0f, 1f), new(1f, 1f, 0f, 1f), t);
                }
                else if (e.Health > 20)
                {
                    float t = (50 - e.Health) / 30f;
                    HealthColor = Vector4.Lerp(new(1f, 1f, 0f, 1f), new(1f, 0f, 0f, 1f), t);
                }
                else
                    HealthColor = new(1f, 0f, 0f, 1f);
            }


            GameState.renderer.DrawList.AddRectFilled(filledTop, filledTop + new Vector2(HealthBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(HealthColor));
        }
        public static void DrawHealthBarPreview(Vector2 position, float entityHeight)
        {
            float barWidth = 5f;
            float healthPercent = TextRenderer.AnimateFloat("HealthBar");
            float offset = 4;

            Vector2 top = position + new Vector2(-entityHeight / 3f - barWidth - offset, -entityHeight / 2);
            Vector2 bottom = position + new Vector2(-entityHeight / 3f - offset, entityHeight / 2);

            ImGui.GetWindowDrawList().AddRectFilled(top, bottom, ImGui.ColorConvertFloat4ToU32(HealthBarBackGround));
            ImGui.GetWindowDrawList().AddRectFilled(top + new Vector2(0, entityHeight * (1 - healthPercent)), bottom, ImGui.ColorConvertFloat4ToU32(HealthColor));
        }

    }
}