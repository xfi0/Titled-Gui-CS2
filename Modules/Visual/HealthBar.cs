using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ImGuiNET;
using Titled_Gui;
using Titled_Gui.Data;
using Titled_Gui.Classes;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar 
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;
        public static bool TeamCheck = false;
        public static float HealthBarWidth = 5f;
        public static float Rounding = 2.3f;
        public static void DrawHealthBar(Renderer renderer, float Health, float maxHealth, Vector2 topLeft, float height, Entity e)
        {
            if (!EnableHealthBar || HealthBar.DrawOnSelf && e.Team == GameState.localPlayer.Team || e == null) return;

            float HealthPercentage = Math.Clamp(Health / maxHealth, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * HealthPercentage;

            renderer.drawList.AddRectFilled(topLeft, topLeft + new Vector2(HealthBarWidth, height), ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1f)), Rounding);

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);
            Vector4 HealthColor = Colors.RGB ? Colors.Rgb(HealthPercentage) : new Vector4(0f, 1f, 0f, 1f);

            renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(HealthBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(HealthColor), Rounding);
        }
    }
}