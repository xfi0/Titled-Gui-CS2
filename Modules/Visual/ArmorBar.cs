using ImGuiNET;
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
        public static void DrawArmorBar(Renderer renderer, float Armor, float MaxArmor, Vector2 topLeft, float height, Entity e)
        {
            if (!EnableArmorhBar || (!TeamCheck && e.Team == GameState.localPlayer.Team) || e == null || BoxESP.FlashCheck && GameState.localPlayer.IsFlashed || e.Armor < 1) return;

            float HealthPercentage = Math.Clamp(Armor / MaxArmor, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * HealthPercentage;

            renderer.drawList.AddRectFilled(topLeft, topLeft + new Vector2(ArmorBarWidth, height), ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1f)), Rounding);

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);
            Vector4 HealthColor = Colors.RGB ? Colors.Rgb(HealthPercentage) : new Vector4(0.1f, 0f, 1f, 1f);

            renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(ArmorBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(HealthColor), Rounding);
        }
    }
}
