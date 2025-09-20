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
        public static bool EnableESP = false;
        public static bool DrawOnSelf = false;
        public static float BoxFillOpacity = 0.2f; // 20%
        public static string[] Shapes = new string[] { "2D Box", "3D Box", "Edges" };
        public static int CurrentShape = 0;
        public static bool EnableDistanceTracker = false;
        public static bool Outline = false;
        public static Vector2 OutlineThickness = new(1f, 1f);
        public static float Rounding = 3f;
        public static bool FlashCheck = true; // THIS APPLIES TO ALL VISUALS BESIDES LIKE ONES THAT DONT HAVE ANYTHING TO DO WITH THE ENTITIES
        public static float GlowAmount = 1f;
        public static bool BoxFillGradient = true;
        public static Vector4 BoxFillGradientColorTop = new(1f,1f,1f, BoxFillOpacity);
        public static Vector4 BoxFillGradientBottom = new(0f,0f,0f, BoxFillOpacity);
        public static void DrawBoxESP(Entity entity, Entity localPlayer, Renderer renderer)
        {
            if (!EnableESP || (DrawOnSelf && entity.PawnAddress != GameState.localPlayer.PawnAddress) || FlashCheck && GameState.localPlayer.IsFlashed) return;

            try
            {
                Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) : (GameState.localPlayer.Team == entity.Team ? Colors.TeamColor : Colors.EnemyColor);

                Vector4 fillColor = boxColor;
                fillColor.W = BoxFillOpacity;
                // get dimentions
                float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                float halfWidth = entityHeight / 3f;
                float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
                float topY = entity.Bones2D[2].Y;
                float bottomY = entity.Position2D.Y;

                switch (CurrentShape)
                {
                    case 0: // 2D box
                        Vector2 rectTop = new(centerX - halfWidth, topY);
                        Vector2 rectBottom = new(centerX + halfWidth, bottomY);
                        if (GlowAmount > 0f)
                            DrawHelpers.DrawGlowRect(renderer.drawList, rectTop, rectBottom, boxColor, Rounding, GlowAmount);

                        if (Outline)
                            GameState.renderer.drawList.AddRect(rectTop + OutlineThickness, rectBottom + OutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000);

                        if (BoxFillGradient)
                            DrawHelpers.DrawGradientRect(renderer.drawList, rectTop, rectBottom, new Vector4(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z, BoxFillOpacity), new Vector4(BoxFillGradientBottom.X, BoxFillGradientBottom.Y, BoxFillGradientBottom.Z, BoxFillOpacity), Rounding);
                        else
                            renderer.drawList.AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor), Rounding);

                            renderer.drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor), Rounding); // outside
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
                        // all faces
                        if (GlowAmount > 0f)
                        {
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontTopLeft, frontTopRight, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontTopRight, frontBottomRight, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomRight, frontBottomLeft, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomLeft, frontTopLeft, boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, backTopLeft, backTopRight, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, backTopRight, backBottomRight, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, backBottomRight, backBottomLeft, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, backBottomLeft, backTopLeft, boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, frontTopLeft, backTopLeft, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontTopRight, backTopRight, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomLeft, backBottomLeft, boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomRight, backBottomRight, boxColor, GlowAmount);
                        }
                        if (Outline)
                            GameState.renderer.drawList.AddRect(frontTopLeft + OutlineThickness, frontBottomRight + OutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000);

                       DrawHelpers.DrawGlowQuadFilled(renderer.drawList, frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft, boxColor, GlowAmount);
                       DrawHelpers.DrawGlowQuadFilled(renderer.drawList, backTopLeft, backTopRight, backBottomRight, backBottomLeft, boxColor, GlowAmount);
                       DrawHelpers.DrawGlowQuadFilled(renderer.drawList, frontTopLeft, frontTopRight, backTopRight, backTopLeft, boxColor, GlowAmount);
                       DrawHelpers.DrawGlowQuadFilled(renderer.drawList, frontBottomLeft, frontBottomRight, backBottomRight, backBottomLeft, boxColor, GlowAmount);
                       DrawHelpers.DrawGlowQuadFilled(renderer.drawList, frontTopLeft, frontBottomLeft, backBottomLeft, backTopLeft, boxColor, GlowAmount);

                        DrawHelpers.DrawGlowQuadFilled(renderer.drawList,frontTopRight, frontBottomRight, backBottomRight, backTopRight, boxColor, GlowAmount);

                        DrawHelpers.DrawGlowQuad(renderer.drawList,frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft, boxColor, GlowAmount);

                        DrawHelpers.DrawGlowQuad(renderer.drawList, backTopLeft, backTopRight, backBottomRight, backBottomLeft, boxColor, GlowAmount);

                        // connections
                        DrawHelpers.DrawGlowLine(renderer.drawList, frontTopLeft, backTopLeft, boxColor, GlowAmount);
                        DrawHelpers.DrawGlowLine(renderer.drawList, frontTopRight, backTopRight, boxColor, GlowAmount);
                        DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomLeft, backBottomLeft, boxColor, GlowAmount);
                        DrawHelpers.DrawGlowLine(renderer.drawList, frontBottomRight, backBottomRight, boxColor, GlowAmount);
                        break;

                    case 2: //  edges
                        Vector2 rectTopLeft = new(centerX - halfWidth, topY);
                        Vector2 rectTopRight = new(centerX + halfWidth, topY);
                        Vector2 rectBottomLeft = new(centerX - halfWidth, bottomY);
                        Vector2 rectBottomRight = new(centerX + halfWidth, bottomY);

                        float edgeFrac = 0.25f; // lenght

                        float edgeWidth = (rectTopRight.X - rectTopLeft.X) * edgeFrac;
                        float edgeHeight = (rectBottomLeft.Y - rectTopLeft.Y) * edgeFrac;
                        if (GlowAmount > 0f)
                        {
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopLeft, new Vector2(rectTopLeft.X + edgeWidth, rectTopLeft.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopLeft, new Vector2(rectTopLeft.X, rectTopLeft.Y + edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopRight, new Vector2(rectTopRight.X - edgeWidth, rectTopRight.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopRight, new Vector2(rectTopRight.X, rectTopRight.Y + edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomLeft, new Vector2(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomLeft, new Vector2(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomRight, new Vector2(rectBottomRight.X - edgeWidth, rectBottomRight.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomRight, new Vector2(rectBottomRight.X, rectBottomRight.Y - edgeHeight), boxColor, GlowAmount);
                        }


                        renderer.drawList.AddLine(rectTopLeft, new Vector2(rectTopLeft.X + edgeWidth, rectTopLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectTopLeft, new Vector2(rectTopLeft.X, rectTopLeft.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectTopRight, new Vector2(rectTopRight.X - edgeWidth, rectTopRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectTopRight, new Vector2(rectTopRight.X, rectTopRight.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectBottomLeft, new Vector2(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectBottomLeft, new Vector2(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectBottomRight, new Vector2(rectBottomRight.X - edgeWidth, rectBottomRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectBottomRight, new Vector2(rectBottomRight.X, rectBottomRight.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        break;

                }
            }
            catch (Exception e)
            {
                SendNotification("ERROR", $"A Exception Was Thrown: {e}"); // no string interpolation wow
            }
        }
    }
}