using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP
    {
        public static bool Tracers = true;
        public static bool TeamCheck = false;
        public static bool enableESP = false;
        public static bool DrawOnSelf = false;
        public static float BoxFillOpacity = 0.2f; // 20%
        public static void DrawBoxESP(Entity entity, Entity localPlayer, Renderer renderer)
        {
            if (!enableESP || (TeamCheck && GameState.localPlayer.team == entity.team))
                return;
            try
            {
                if (entity.PawnAddress == localPlayer.PawnAddress && !DrawOnSelf)
                    return;
                Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) :
                        (GameState.localPlayer.team == entity.team ? Colors.teamColor : Colors.enemyColor);

                float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;

                // pos and dementions
                Vector2 rectTTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
                Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y);
                //ModuleHelpers.GetGunName.GetGunNameFunction(entity);
                renderer.drawList.AddRect(rectTTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));

                // add semi transparent fill
                Vector4 fillColor = boxColor;
                fillColor.W = BoxFillOpacity; 
                renderer.drawList.AddRectFilled(rectTTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor));
            }
            catch
            {
            }
        }
    }
}