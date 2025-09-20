using System.Numerics;
using ImGuiNET;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar 
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;
        public static bool TeamCheck = false;
        public static float HealthBarWidth = 5f;
        public static float Rounding = 2.3f;
        public static Vector4 HealthBarBackGround = new(0.2f, 0.2f, 0.2f, 1f);
        public static Vector4 HealthColor = new();

        public static void DrawHealthBar(Renderer renderer, float Health, float maxHealth, Vector2 topLeft, float height, Entity e)
        {
            if (!EnableHealthBar || (!DrawOnSelf && e.PawnAddress == GameState.localPlayer.PawnAddress) || e == null || (BoxESP.FlashCheck && GameState.localPlayer.IsFlashed)) return;

            float HealthPercentage = Math.Clamp(Health / maxHealth, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * HealthPercentage;

            renderer.drawList.AddRectFilled(topLeft, topLeft + new Vector2(HealthBarWidth, height), ImGui.ColorConvertFloat4ToU32(HealthBarBackGround), Rounding);

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);
            HealthColor = Colors.RGB ? Colors.Rgb(HealthPercentage) : new Vector4(0f, 1f, 0f, 1f);

            renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(HealthBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(HealthColor), Rounding);
        }
    }
}