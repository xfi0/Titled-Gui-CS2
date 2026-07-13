using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class ArmorBar
    {
        public static bool EnableArmorhBar = false;
        public static bool DrawOnSelf = false;
        public static bool RGB = false;
        public static float ArmorBarWidth = 5f;
        public static float Rounding = 0;
        public static Vector4 ArmorColor = new(0.1f, 0f, 1f, 1f);
        private static Vector4 _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        public static void DrawArmorBar(Entity? e, Renderer renderer, float armor, float maxArmor)
        {
            if (!EnableArmorhBar || e == null || e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 ||
                (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e.Armor < 1 ||
                e.Position2D == new Vector2(-99, -99))
                return;

            var rect = BoxESP.GetBoxRect(e);
            if (rect == null)
                return;

            Vector2 barTopRight = new(rect.TopRight.X - HealthBar.HealthBarWidth + 8, rect.TopRight.Y);
            float height = rect.BottomRight.Y - rect.TopLeft.Y;

            float healthPercentage = Math.Clamp(armor / maxArmor, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * healthPercentage;

            renderer.DrawList.AddRectFilled(rect.TopRight, rect.TopRight + new Vector2(ArmorBarWidth, height),
                ImGui.ColorConvertFloat4ToU32(_backgroundColor), Rounding);

            Vector2 filledTop = rect.TopRight + new Vector2(0, height - filledHeight);
            ArmorColor = RGB ? Colors.Rgb(ArmorColor.W) : new(0.1f, 0f, 1f, 1f);

            renderer.DrawList.AddRectFilled(filledTop, filledTop + new Vector2(ArmorBarWidth, filledHeight),
                ImGui.ColorConvertFloat4ToU32(ArmorColor), Rounding);
        }

        public static void DrawArmorBarPreview(Vector2 position, float entityHeight)
        {
            float barWidth = 5f;
            float armorPercent = Titled_Gui.Classes.Rendering.TextRenderer.AnimateFloat("healthBar");
            float offset = 4;


            Vector2 top = position + new Vector2(entityHeight / 3f + offset, -entityHeight / 2);
            Vector2 bottom = position + new Vector2(entityHeight / 3f + barWidth + offset, entityHeight / 2);

            ImGui.GetWindowDrawList().AddRectFilled(top, bottom, ImGui.ColorConvertFloat4ToU32(_backgroundColor));
            ImGui.GetWindowDrawList().AddRectFilled(top + new Vector2(0, entityHeight * (1 - armorPercent)), bottom, ImGui.ColorConvertFloat4ToU32(ArmorColor));
        }
    }
}
