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
        public static float EdgeMultiple = 0.25f; 
        public static void DrawBoxESP(Entity entity, Entity localPlayer, Renderer renderer)
        {
            if (!EnableESP || (DrawOnSelf && entity.PawnAddress != GameState.localPlayer.PawnAddress) || FlashCheck && GameState.localPlayer.IsFlashed || entity?.Bones2D?[2] != new Vector2(-99, -99) || entity.Bones2D.Count > 0) return;

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
                            DrawHelpers.DrawGradientRect(renderer.drawList, rectTop, rectBottom, new(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z, BoxFillOpacity), new Vector4(BoxFillGradientBottom.X, BoxFillGradientBottom.Y, BoxFillGradientBottom.Z, BoxFillOpacity), Rounding);
                        else
                            renderer.drawList.AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor), Rounding);

                            renderer.drawList.AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor), Rounding); // outside
                        break;

                    case 1:// 3D box, kinda weird looking kinda nice idk
                        {
                            float depth = halfWidth;

                            Vector3[] Corners =
                            {
                                new(entity.Position.X - halfWidth, entity.Position.Y + entityHeight, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y + entityHeight, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y + entityHeight, entity.Position.Z + depth),
                                new(entity.Position.X - halfWidth, entity.Position.Y + entityHeight, entity.Position.Z + depth),

                                new(entity.Position.X - halfWidth, entity.Position.Y, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y, entity.Position.Z + depth),
                                new(entity.Position.X - halfWidth, entity.Position.Y, entity.Position.Z + depth),
                            };
                            float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

                            Vector2[] Corners2D = new Vector2[Corners.Length];
                            for (int i = 0; i < Corners.Length; i++)
                            {
                                Corners2D[i] = Calculate.WorldToScreen(ViewMatrix, Corners[i], GameState.renderer.screenSize);

                                if (Corners2D[i].X < 0 || Corners2D[i].Y < 0) return;
                            }

                            renderer.drawList.AddLine(Corners2D[0], Corners2D[1], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[1], Corners2D[2], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[2], Corners2D[3], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[3], Corners2D[0], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[4], Corners2D[5], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[5], Corners2D[6], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[6], Corners2D[7], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[7], Corners2D[4], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[0], Corners2D[4], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[1], Corners2D[5], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[2], Corners2D[6], ImGui.ColorConvertFloat4ToU32(boxColor));
                            renderer.drawList.AddLine(Corners2D[3], Corners2D[7], ImGui.ColorConvertFloat4ToU32(boxColor));
                        }
                        break;

                    case 2: //  edges
                        Vector2 rectTopLeft = new(centerX - halfWidth, topY);
                        Vector2 rectTopRight = new(centerX + halfWidth, topY);
                        Vector2 rectBottomLeft = new(centerX - halfWidth, bottomY);
                        Vector2 rectBottomRight = new(centerX + halfWidth, bottomY);

                        float edgeWidth = (rectTopRight.X - rectTopLeft.X) * EdgeMultiple;
                        float edgeHeight = (rectBottomLeft.Y - rectTopLeft.Y) * EdgeMultiple;

                        if (GlowAmount > 0f)
                        {
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopLeft, new(rectTopLeft.X + edgeWidth, rectTopLeft.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopLeft, new(rectTopLeft.X, rectTopLeft.Y + edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopRight, new(rectTopRight.X - edgeWidth, rectTopRight.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectTopRight, new(rectTopRight.X, rectTopRight.Y + edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomLeft, new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomLeft, new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), boxColor, GlowAmount);

                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomRight, new(rectBottomRight.X - edgeWidth, rectBottomRight.Y), boxColor, GlowAmount);
                            DrawHelpers.DrawGlowLine(renderer.drawList, rectBottomRight, new(rectBottomRight.X, rectBottomRight.Y - edgeHeight), boxColor, GlowAmount);
                        }


                        renderer.drawList.AddLine(rectTopLeft, new(rectTopLeft.X + edgeWidth, rectTopLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectTopLeft, new(rectTopLeft.X, rectTopLeft.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectTopRight, new(rectTopRight.X - edgeWidth, rectTopRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectTopRight, new(rectTopRight.X, rectTopRight.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectBottomLeft, new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectBottomLeft, new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

                        renderer.drawList.AddLine(rectBottomRight, new(rectBottomRight.X - edgeWidth, rectBottomRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor)); 
                        renderer.drawList.AddLine(rectBottomRight, new(rectBottomRight.X, rectBottomRight.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor)); 

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