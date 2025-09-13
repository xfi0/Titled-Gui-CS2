using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui.Classes;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
using static Titled_Gui.Classes.Colors;
using static Titled_Gui.Renderer;

namespace Titled_Gui.Modules.Visual
{
    public class Tracers
    {
        public static bool enableTracers = false;
        public static bool DrawOnSelf = false;
        public static float LineThickness = 1f;
        public static string[] StartPositions = new string[] { "Middle", "Bottom", "Top" };
        public static string[] EndPositions = new string[] { "Bottom", "Top" };
        public static int CurrentStartPos = 0;
        public static int CurrentEndPos = 0;
        public static Vector2 StartPos = new();
        public static void DrawTracers(Entity entity, Renderer renderer)
        {
            if (!enableTracers || Tracers.DrawOnSelf && entity == localPlayer) return;

            if (entity.Position2D != new Vector2(-99, -99))
            {
                switch (CurrentStartPos)
                {
                    case 0:
                        StartPos = new(renderer.screenSize.X / 2, renderer.screenSize.Y / 2);
                        break;
                    case 1:
                        StartPos = new(renderer.screenSize.X / 2, renderer.screenSize.Y);
                        break;
                    case 2:
                        StartPos = new(renderer.screenSize.X / 2, -renderer.screenSize.Y);
                        break;
                }
                if (!RGB)
                {
                    Vector4 lineColor = localPlayer.Team == entity.Team ? TeamColor : enemyColor; //get color idk if rgb works here
                    renderer.drawList.AddLine(StartPos, entity.Position2D, ImGui.ColorConvertFloat4ToU32(lineColor), LineThickness); // add line for non rgb just liek Team color
                }
                else if (RGB)
                {
                    Vector4 lineColor = Colors.Rgb(0.5f); //rgb works here nvm
                    renderer.drawList.AddLine(StartPos, entity.Position2D, ImGui.GetColorU32(lineColor), LineThickness); // add line for rgb
                }
            }
        }
    }
}