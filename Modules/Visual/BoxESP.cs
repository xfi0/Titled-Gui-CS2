using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;
using static Titled_Gui.Notifications.Library;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP
    {
        public static bool TeamCheck = false;
        public static bool EnableESP = false;
        public static float BoxFillOpacity = 0.2f; // 20%
        public static string[] Shapes = new string[] { "2D Box", "3D Box", "Edges" };
        public static int CurrentShape = 0;
        public static bool EnableDistanceTracker = false;
        public static bool InnerOutline = false;
        public static bool OuterOutline = true;
        public static Vector2 InnerOutlineThickness = new(1f, 1f);
        public static Vector4 InnerOutlineColor = new(0, 0, 0, 1f);
        public static Vector2 OuterOutlineThickness = new(2f, 2f);
        public static Vector4 OutlineEnemyColor = new(1, 0, 0, 1);
        public static Vector4 OutlineTeamColor = new(0, 1, 0, 1);
        public static float Rounding = 3f;
        public static bool FlashCheck = true; // THIS APPLIES TO ALL VISUALS BESIDES LIKE ONES THAT DONT HAVE ANYTHING TO DO WITH THE ENTITIES
        public static float GlowAmount = 1f;
        public static bool BoxFillGradient = true;
        public static Vector4 BoxFillGradientColorTop = new(1f,1f,1f, BoxFillOpacity);
        public static Vector4 BoxFillGradientBottom = new(0f,0f,0f, BoxFillOpacity);
        public static float EdgeMultiplier = 0.25f; 
        public static bool EnableESPPreview = true;

        public static void DrawBoxESP(Entity entity, Entity localPlayer, Renderer renderer)
        {
            if (!EnableESP  || entity == null || (TeamCheck && entity.Team == localPlayer.Team) || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || FlashCheck && GameState.LocalPlayer.IsFlashed || entity?.Bones2D?.Count < 0  || entity?.Bones2D == null) return;

            try
            {
                Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) : (GameState.LocalPlayer.Team == entity?.Team ? Colors.TeamColor : Colors.EnemyColor);

                Vector4 fillColor = boxColor;
                fillColor.W = BoxFillOpacity;
                // get dimensions

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
                        {
                            if (entity.Team == GameState.LocalPlayer.Team)
                                DrawHelpers.DrawGlowRect(renderer.drawList, rectTop, rectBottom, OutlineTeamColor, Rounding, GlowAmount);

                            else
                                DrawHelpers.DrawGlowRect(renderer.drawList, rectTop, rectBottom, OutlineEnemyColor, Rounding, GlowAmount);
                        }

                        if (InnerOutline)
                            GameState.renderer.drawList.AddRect(rectTop - InnerOutlineThickness, rectBottom + InnerOutlineThickness, ImGui.ColorConvertFloat4ToU32(InnerOutlineColor), Rounding);

                        if (BoxFillGradient)
                            DrawHelpers.DrawGradientRect(renderer.drawList, rectTop, rectBottom, new(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z, BoxFillOpacity), new Vector4(BoxFillGradientBottom.X, BoxFillGradientBottom.Y, BoxFillGradientBottom.Z, BoxFillOpacity), Rounding);

                        else
                            renderer.drawList.AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor), Rounding);

                        if (OuterOutline)
                        {
                            if (entity.Team == GameState.LocalPlayer.Team)
                                renderer.drawList.AddRect(rectTop + OuterOutlineThickness, rectBottom + OuterOutlineThickness, ImGui.ColorConvertFloat4ToU32(OutlineTeamColor), Rounding); // outside

                            else
                                renderer.drawList.AddRect(rectTop + OuterOutlineThickness, rectBottom + OuterOutlineThickness, ImGui.ColorConvertFloat4ToU32(OutlineEnemyColor), Rounding); // outside
                        }

                        break;

                    case 1:// 3D box, kinda weird looking kinda nice idk
                        {
                            float depth = halfWidth;

                            Vector3[] Corners =
                            [
                                new(entity.Position.X - halfWidth, entity.Position.Y + entityHeight, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y + entityHeight, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y + entityHeight, entity.Position.Z + depth),
                                new(entity.Position.X - halfWidth, entity.Position.Y + entityHeight, entity.Position.Z + depth),

                                new(entity.Position.X - halfWidth, entity.Position.Y, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y, entity.Position.Z - depth),
                                new(entity.Position.X + halfWidth, entity.Position.Y, entity.Position.Z + depth),
                                new(entity.Position.X - halfWidth, entity.Position.Y, entity.Position.Z + depth),
                            ];
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

                        float edgeWidth = (rectTopRight.X - rectTopLeft.X) * EdgeMultiplier;
                        float edgeHeight = (rectBottomLeft.Y - rectTopLeft.Y) * EdgeMultiplier;
                        Vector2 TopLeftSideIThink = new(centerX + halfWidth, bottomY);
                        //if (Outline)
                        //    renderer.drawList.AddRect(rectTopLeft + OutlineThickness, TopLeftSideIThink + OutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000, Rounding);

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
                SendNotification("ERROR", $"A Exception Was Thrown: {e}");
            }
        }
        public static void RenderESPPreview(Vector2 center)
        {
            if (!EnableESPPreview) return;

            if (EnableESP)
                DrawBoxPreview(center);

            if (EnableDistanceTracker)
                DrawDistancePreview(center);

            if (BoneESP.EnableBoneESP)
                BoneESP.DrawBonePreview(center);

            if (HealthBar.EnableHealthBar)
                HealthBar.DrawHealthBarPreview(center + new Vector2(-70, -100));

            if (ArmorBar.EnableArmorhBar)
                ArmorBar.DrawArmorBarPreview(center + new Vector2(70, -100));

            if (NameDisplay.Enabled)
                NameDisplay.DrawNamePreview(center + new Vector2(70, -100));

            if (Tracers.enableTracers)
                Tracers.DrawTracerPreview(center);
        }

        public static void DrawDistancePreview(Vector2 position)
        {
            ImGui.GetWindowDrawList().AddText(position + new Vector2(0, 60), ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), "15m");
        }

        public static void DrawBoxPreview(Vector2 position)
        {
            if (!EnableESPPreview) return;

            float entityHeight = 200f; 
            float halfWidth = entityHeight / 3f;
            float centerX = position.X;

            float topY = position.Y - entityHeight / 2;
            float bottomY = position.Y + entityHeight / 2;

            Vector4 boxColor = Colors.RGB ? Colors.Rgb(0.5f) : Colors.EnemyColor;
            boxColor.W = BoxFillOpacity;

            Vector4 fillColor = boxColor;
            fillColor.W = BoxFillOpacity;
            switch (CurrentShape)
            {
                case 0: // 2D box
                    Vector2 rectTop = new(centerX - halfWidth, topY);
                    Vector2 rectBottom = new(centerX + halfWidth, bottomY);

                    if (GlowAmount > 0f)
                        DrawHelpers.DrawGlowRect(ImGui.GetWindowDrawList(), rectTop, rectBottom, boxColor, Rounding, GlowAmount);

                    if (InnerOutline)
                        ImGui.GetWindowDrawList().AddRect(rectTop + InnerOutlineThickness, rectBottom + InnerOutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000, Rounding);

                    if (BoxFillGradient)
                        DrawHelpers.DrawGradientRect(ImGui.GetWindowDrawList(), rectTop, rectBottom, new(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z, BoxFillOpacity), new Vector4(BoxFillGradientBottom.X, BoxFillGradientBottom.Y, BoxFillGradientBottom.Z, BoxFillOpacity), Rounding);

                    else
                        ImGui.GetWindowDrawList().AddRectFilled(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor), Rounding);

                    ImGui.GetWindowDrawList().AddRect(rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor), Rounding); // outside
                    break;

                case 1: // 3D box
                    float depth = halfWidth;
                    Vector3[] corners =
                    [
                     new(position.X - halfWidth, position.Y + entityHeight / 2, position.Y - depth),
                     new(position.X + halfWidth, position.Y + entityHeight / 2, position.Y - depth),
                     new(position.X + halfWidth, position.Y + entityHeight / 2, position.Y + depth),
                     new(position.X - halfWidth, position.Y + entityHeight / 2, position.Y + depth),
                     new(position.X - halfWidth, position.Y - entityHeight / 2, position.Y - depth),
                     new(position.X + halfWidth, position.Y - entityHeight / 2, position.Y - depth),
                     new(position.X + halfWidth, position.Y - entityHeight / 2, position.Y + depth),
                     new(position.X - halfWidth, position.Y - entityHeight / 2, position.Y + depth),
                    ];

                    float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                    Vector2[] corners2D = new Vector2[corners.Length];

                    for (int i = 0; i < corners.Length; i++)
                    {
                        corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners[i], GameState.renderer.screenSize);
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        ImGui.GetWindowDrawList().AddLine(corners2D[i], corners2D[(i + 1) % 4], ImGui.ColorConvertFloat4ToU32(boxColor));
                        ImGui.GetWindowDrawList().AddLine(corners2D[i + 4], corners2D[((i + 1) % 4) + 4], ImGui.ColorConvertFloat4ToU32(boxColor));
                        ImGui.GetWindowDrawList().AddLine(corners2D[i], corners2D[i + 4], ImGui.ColorConvertFloat4ToU32(boxColor));
                    }
                    break;

                case 2: // edges
                    Vector2 rectTopLeft = new(centerX - halfWidth, topY);
                    Vector2 rectTopRight = new(centerX + halfWidth, topY);
                    Vector2 rectBottomLeft = new(centerX - halfWidth, bottomY);
                    Vector2 rectBottomRight = new(centerX + halfWidth, bottomY);

                    float edgeWidth = (rectTopRight.X - rectTopLeft.X) * EdgeMultiplier;
                    float edgeHeight = (rectBottomLeft.Y - rectTopLeft.Y) * EdgeMultiplier;

                    ImGui.GetWindowDrawList().AddLine(rectTopLeft, new(rectTopLeft.X + edgeWidth, rectTopLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectTopLeft, new(rectTopLeft.X, rectTopLeft.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectTopRight, new(rectTopRight.X - edgeWidth, rectTopRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectTopRight, new(rectTopRight.X, rectTopRight.Y + edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectBottomLeft, new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectBottomLeft, new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectBottomRight, new(rectBottomRight.X - edgeWidth, rectBottomRight.Y), ImGui.ColorConvertFloat4ToU32(boxColor));
                    ImGui.GetWindowDrawList().AddLine(rectBottomRight, new(rectBottomRight.X, rectBottomRight.Y - edgeHeight), ImGui.ColorConvertFloat4ToU32(boxColor));
                    break;
            }
        }
        public static (Vector2 TopLeft, Vector2 BottomRight, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomMiddle)? GetBoxRect(Entity entity)
        {
            if (entity == null || entity.Position2D == Vector2.Zero || entity.ViewPosition2D == Vector2.Zero)
                return null;

            float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
            float halfWidth = entityHeight / 3f;
            float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
            float topY = entity.Bones2D != null && entity.Bones2D.Count > 2 ? entity.Bones2D[2].Y : entity.ViewPosition2D.Y;
            float bottomY = entity.Position2D.Y;

            Vector2 topLeft = new(centerX - halfWidth, topY);
            Vector2 topRight = new(centerX + halfWidth, topY);
            Vector2 bottomLeft = new(centerX - halfWidth, bottomY);
            Vector2 bottomRight = new(centerX + halfWidth, bottomY);
            Vector2 bottomMiddle = new(centerX, bottomY);

            return (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle);
        }
    }
}