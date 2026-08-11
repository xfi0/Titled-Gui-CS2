using ImGuiNET;
using NAudio.Gui;
using System.Numerics;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;

namespace Titled_Gui.Modules.Visual
{
    internal class ArmorBar
    {
        public static bool EnableArmorBar = false;
        public static bool DrawOnSelf = false; // why does this exist, i would remove but its funny.
        public static float ArmorBarWidth = 5f;
        public static float Rounding = 0;
        public static Colors ArmorColor = new(new(0.1f, 0f, 1f, 1f), new(0.1f, 0f, 1f, 1f));
        private static Vector4 _backgroundColor = new(0.2f, 0.2f, 0.2f, 1f);
        private static Vector4 _outlineColor = new(0f, 0f, 0f, 1f);
        private static int _outlineThickness = 1;
        private static int _paddingX = 3;

        public static void DrawArmorBar(Entity? e, Renderer renderer, float armor, float maxArmor, BoxRect rect)
        {
            if (!EnableArmorBar || e == null || GameState.LocalPlayer == null || e.PawnAddress == GameState.LocalPlayer.PawnAddress || e.Health <= 0 ||
                (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team) ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || e.Armor <= 0 ||
                e.Position2D == new Vector2(-99, -99))
                return;

            float height = rect.BottomRight.Y - rect.TopLeft.Y;
            float filledHeight = height * Math.Clamp(armor / maxArmor, 0f, 1f);

            Vector2 top = new(rect.TopRight.X + _paddingX, rect.TopRight.Y);
            Vector2 bottom = new(rect.TopRight.X + _paddingX + ArmorBarWidth, rect.TopRight.Y + height);

            DrawArmorBarInternal(renderer.DrawList, top, bottom, filledHeight, GetArmorColor(e.IsTeammate));
        }

        private static Vector4 GetArmorColor(bool teammate)
        {
            if (teammate)
                return ArmorColor.TeamRGB ? Colors.Rgb(ArmorColor.TeamColor.W) : ArmorColor.TeamColor;
            else
                return ArmorColor.EnemyRGB ? Colors.Rgb(ArmorColor.EnemyColor.W) : ArmorColor.EnemyColor;
        }

        private static void DrawArmorBarInternal(ImDrawListPtr drawList, Vector2 top, Vector2 bottom, float filledHeight, Vector4 armorColor)
        {
            drawList.AddRectFilled(new(top.X - _outlineThickness, top.Y - _outlineThickness), new(bottom.X + _outlineThickness, bottom.Y + _outlineThickness), ImGui.ColorConvertFloat4ToU32(_outlineColor), Rounding);
            drawList.AddRectFilled(top, bottom, ImGui.ColorConvertFloat4ToU32(_backgroundColor), Rounding);

            Vector2 filledTop = new(top.X, bottom.Y - filledHeight);
            drawList.AddRectFilled(filledTop, new(bottom.X, bottom.Y), ImGui.ColorConvertFloat4ToU32(armorColor), Rounding);
        }

        public static void DrawArmorBarPreview(Vector2 position, float entityHeight)
        {
            float armorPercent = Titled_Gui.Classes.Rendering.TextRenderer.AnimateFloat("HealthBar", 0.8f); // health bar because they should be in sync.
            float offset = 4;

            Vector2 top = position + new Vector2(entityHeight / 3f + offset, -entityHeight / 2);
            Vector2 bottom = position + new Vector2(entityHeight / 3f + ArmorBarWidth + offset, entityHeight / 2);
            float filledHeight = (bottom.Y - top.Y) * armorPercent;

            DrawArmorBarInternal(ImGui.GetWindowDrawList(), top, bottom, filledHeight, GetArmorColor(false));
        }
    }
}
