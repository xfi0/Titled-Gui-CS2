using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP
    {
        public static bool Tracers = true;
        public static bool TeamCheck = false;
        public static bool enableESP = false;
        public static bool DrawOnSelf = false;

        public static void DrawBoxESP(Entity entity, Renderer renderer)
        {
            //  get color
            Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) : (GameState.localPlayer.team == entity.team ? Colors.teamColor : Colors.enemyColor);
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

            // get the dimenstions and pos
            Vector2 rectTTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);

            //draw a hollow box around the entity, maybee add a like semi transparent box in the middle?
            renderer.drawList.AddRect(rectTTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }
    }
}