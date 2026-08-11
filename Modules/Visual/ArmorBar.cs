using ImGuiNET;
using NAudio.Gui;
using System.Numerics;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;

namespace Titled_Gui.Modules.Visual
{
    internal class ArmorBar
    {
        public static bool EnableArmorBar = false;
        public static bool DrawOnSelf = false; // why does this exist, i would remove but its funny.
        public static bool RGB = false;
        public static float ArmorBarWidth = 5f;
        public static float Rounding = 0;
        public static Colors ArmorColor = new(new(0.1f, 0f, 1f, 1f), new(0.1f, 0f, 1f, 1f));
        private static Vector4 _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        private static Vector4 _outlineColor = new(0f, 0f, 0f, 1f);
        private static int _outlineThickness = 1;

        public static void DrawArmorBar(Entity? e, Renderer renderer, float armor, float maxArmor)
        {
            if (!EnableArmorBar || e == null || GameState.LocalPlayer == null || e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 ||
                (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e.Armor <= 0 ||
                e.Position2D == new Vector2(-99, -99))
                return;

            var rect = BoxESP.GetBoxRect(e);
            if (rect == null)
                return;

            float height = rect.BottomRight.Y - rect.TopLeft.Y;

            float armorPercentage = Math.Clamp(armor / maxArmor, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * armorPercentage;

            renderer.DrawList.AddRectFilled(rect.TopRight, rect.TopRight + new Vector2(ArmorBarWidth, height), ImGui.ColorConvertFloat4ToU32(_backgroundColor), Rounding);

            renderer.DrawList.AddRectFilled(new Vector2(rect.TopRight.X - _outlineThickness, rect.TopRight.Y - _outlineThickness), new Vector2(rect.TopRight.X + ArmorBarWidth + _outlineThickness, rect.TopRight.Y + height + _outlineThickness), ImGui.ColorConvertFloat4ToU32(_outlineColor), Rounding);
            Vector2 filledTop = rect.TopRight + new Vector2(0, height - filledHeight);
            Vector4 armorColor = GetArmorColor(e.IsTeammate);

            renderer.DrawList.AddRectFilled(filledTop, filledTop + new Vector2(ArmorBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(armorColor), Rounding);
        }

        private static Vector4 GetArmorColor(bool teammate)
        {
            if (teammate)
                return ArmorColor.TeamRGB ? Colors.Rgb(ArmorColor.TeamColor.W) : ArmorColor.TeamColor;
            else
                return ArmorColor.EnemyRGB ? Colors.Rgb(ArmorColor.EnemyColor.W) : ArmorColor.EnemyColor;
        }

        public static void DrawArmorBarPreview(Vector2 position, float entityHeight)
        {
            float barWidth = 5f;
            float armorPercent = Titled_Gui.Classes.Rendering.TextRenderer.AnimateFloat("healthBar");
            float offset = 4;


            Vector2 top = position + new Vector2(entityHeight / 3f + offset, -entityHeight / 2);
            Vector2 bottom = position + new Vector2(entityHeight / 3f + barWidth + offset, entityHeight / 2);
            Vector4 color = GetArmorColor(false);

            ImGui.GetWindowDrawList().AddRectFilled(top, bottom, ImGui.ColorConvertFloat4ToU32(_backgroundColor));
            ImGui.GetWindowDrawList().AddRectFilled(top + new Vector2(0, entityHeight * (1 - armorPercent)), bottom, ImGui.ColorConvertFloat4ToU32(color));
        }
    }
}
