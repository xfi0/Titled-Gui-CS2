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
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;

        public static void DrawHealthBar(Renderer renderer, float health, float maxHealth, Vector2 topLeft, float height, float width = 5f)
        {
            float healthPercentage = Math.Clamp(health / maxHealth, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * healthPercentage;

            renderer.drawList.AddRectFilled(topLeft, topLeft + new Vector2(width, height), ImGui.ColorConvertFloat4ToU32(new Vector4(0.2f, 0.2f, 0.2f, 1f)));

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);
            Vector4 healthColor = Colors.RGB ? Colors.Rgb(healthPercentage) : new Vector4(0f, 1f, 0f, 1f);

            renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(width, filledHeight), ImGui.ColorConvertFloat4ToU32(healthColor));
        }
    }
}