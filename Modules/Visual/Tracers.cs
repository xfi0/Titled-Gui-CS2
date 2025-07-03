using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
using static Titled_Gui.ModuleHelpers.Colors;
using static Titled_Gui.Renderer;

namespace Titled_Gui.Modules.Visual
{
    public class Tracers
    {
        public static bool enableTracers = true;
        public static bool DrawOnSelf = false;

        public static void DrawTracers(Entity entity, Renderer renderer)
        {
            if (enableTracers && !RGB)
            {
               
                Vector4 lineColor = localPlayer.team == entity.team ? teamColor : enemyColor; //get color idk if rgb works here
                renderer.drawList.AddLine(new Vector2(renderer.screenSize.X / 2, renderer.screenSize.Y / 2), entity.position2D, ImGui.ColorConvertFloat4ToU32(lineColor), 1.0f); // add line for non rgb just liek team color
            }
            else if (enableTracers && RGB)
            {
                Vector4 lineColor = Colors.Rgb(0.5f); //rgb works here nvm
                renderer.drawList.AddLine(new Vector2(renderer.screenSize.X / 2, renderer.screenSize.Y / 2), entity.position2D, ImGui.GetColorU32(lineColor), 1.0f); // add line for rgb
            }
        }
    }
}