using ImGuiNET;
using Newtonsoft.Json.Bson;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class ArmorBar
    {
        public static bool EnableArmorhBar = false;
        public static bool DrawOnSelf = false;
        public static bool TeamCheck = false;
        public static float ArmorBarWidth = 5f;
        public static float Rounding = 2.3f;
        public static Vector4 ArmorColor = new(0.1f, 0f, 1f, 1f);
        public static void DrawArmorBar(Renderer renderer, float Armor, float MaxArmor, Vector2 topRight, float height, Entity e)
        {
            if (!EnableArmorhBar || (!TeamCheck && e.Team == GameState.LocalPlayer.Team) || e == null || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed || e.Armor < 1) return;

            float HealthPercentage = Math.Clamp(Armor / MaxArmor, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * HealthPercentage;

            renderer.drawList.AddRectFilled(topRight, topRight + new Vector2(ArmorBarWidth, height), ImGui.ColorConvertFloat4ToU32(new(0.2f, 0.2f, 0.2f, 1f)), Rounding);

            Vector2 filledTop = topRight + new Vector2(0, height - filledHeight);
            ArmorColor = Colors.RGB ? Colors.Rgb(HealthPercentage) : new(0.1f, 0f, 1f, 1f);

            renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(ArmorBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(ArmorColor), Rounding);
        }
        public static void DrawArmorBarPreview(Vector2 position)
        {
            float BarHeight = 200f;
            float BarWidth = 6f;
            float HealthPercentage = 0.5f; // TODO maybe make moving

            Vector2 bottom = position + new Vector2(0, BarHeight);
            ImGui.GetWindowDrawList().AddRectFilled(position, bottom + new Vector2(BarWidth, 0), ImGui.ColorConvertFloat4ToU32(ArmorColor));
            ImGui.GetWindowDrawList().AddRectFilled(position, position + new Vector2(BarWidth, BarHeight * HealthPercentage), ImGui.ColorConvertFloat4ToU32(new(0.2f, 0.2f, 0.2f, 1f)));
        }
    }
}
