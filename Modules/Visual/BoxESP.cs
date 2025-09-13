using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data;
using Titled_Gui.Classes;
using static Titled_Gui.Notifications.Library;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP
    {
        public static bool TeamCheck = false;
        public static bool enableESP = false;
        public static bool DrawOnSelf = false;
        public static float BoxFillOpacity = 0.2f; // 20%
        public static string[] Shapes = new string[] { "2D Box", "3D Box" };
        public static int CurrentShape = 0;
        public static Vector2 Top = Vector2.One; // used for name display
        public static bool EnableDistanceTracker = true; // toggle for Distance tracker
        public static bool Outline = false;
        public static Vector2 OutlineThickness = new(1f, 1);
        public static float Rounding = 3f;
        public static void DrawBoxESP(Entity entity, Entity localPlayer, Renderer renderer)
        {
            if (!enableESP || DrawOnSelf && entity.PawnAddress != GameState.localPlayer.PawnAddress) return;


            try
            {
                Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) : (GameState.localPlayer.Team == entity.Team ? Colors.TeamColor : Colors.enemyColor);

                Vector4 fillColor = boxColor;
                fillColor.W = BoxFillOpacity;
                // get dimentions
                float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                float halfWidth = entityHeight / 3f;
                float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
                float topY = entity.ViewPosition2D.Y;
                float bottomY = entity.Position2D.Y;

                switch (CurrentShape)
                {
                    case 0: // 2D box
                        Vector2 rectTop = new(centerX - halfWidth, topY);
                        Vector2 rectBottom = new(centerX + halfWidth, bottomY);

                        if (Outline)
                            GameState.renderer.drawList.AddRect(rectTop + OutlineThickness, rectBottom + OutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000);

                        renderer.drawList.AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor), Rounding); // middle
                        renderer.drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor), Rounding); // outside
                        Top = rectTop;
                        break;

                    case 1: // 3D box, kinda weird looking kinda nice idk
                        float depth = halfWidth * 0.5f;

                        //g et p[ositions
                        Vector2 frontTopLeft = new Vector2(centerX - halfWidth, topY);
                        Vector2 frontTopRight = new Vector2(centerX + halfWidth, topY);
                        Vector2 frontBottomLeft = new Vector2(centerX - halfWidth, bottomY);
                        Vector2 frontBottomRight = new Vector2(centerX + halfWidth, bottomY);
                        Vector2 backTopLeft = new Vector2(centerX - halfWidth + depth, topY - depth);
                        Vector2 backTopRight = new Vector2(centerX + halfWidth + depth, topY - depth);
                        Vector2 backBottomLeft = new Vector2(centerX - halfWidth + depth, bottomY - depth);
                        Vector2 backBottomRight = new Vector2(centerX + halfWidth + depth, bottomY - depth);
                        Top = frontTopLeft; // idk which is best tbh
                        // all faces
                        renderer.drawList.AddQuadFilled(frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuadFilled(backTopLeft, backTopRight, backBottomRight, backBottomLeft, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuadFilled(frontTopLeft, frontTopRight, backTopRight, backTopLeft, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuadFilled(frontBottomLeft, frontBottomRight, backBottomRight, backBottomLeft, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuadFilled(frontTopLeft, frontBottomLeft, backBottomLeft, backTopLeft, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuadFilled(frontTopRight, frontBottomRight, backBottomRight, backTopRight, ImGui.ColorConvertFloat4ToU32(fillColor));

                        renderer.drawList.AddQuad(frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft, ImGui.ColorConvertFloat4ToU32(boxColor));

                        renderer.drawList.AddQuad(backTopLeft, backTopRight, backBottomRight, backBottomLeft, ImGui.ColorConvertFloat4ToU32(boxColor));

                        // connections
                        renderer.drawList.AddLine(frontTopLeft, backTopLeft, ImGui.ColorConvertFloat4ToU32(boxColor));
                        renderer.drawList.AddLine(frontTopRight, backTopRight, ImGui.ColorConvertFloat4ToU32(boxColor));
                        renderer.drawList.AddLine(frontBottomLeft, backBottomLeft, ImGui.ColorConvertFloat4ToU32(boxColor));
                        renderer.drawList.AddLine(frontBottomRight, backBottomRight, ImGui.ColorConvertFloat4ToU32(boxColor));
                        break;
                }
            }
            catch (Exception e)
            {
                SendNotification("ERROR", "A Exception Was Thrown: " + e); // no string interpolation wow
            }
        }
    }
}