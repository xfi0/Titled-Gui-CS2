using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static ValveResourceFormat.ResourceTypes.EntityLump;
using Entity = Titled_Gui.Data.Entity.Entity;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP
    {
        public static bool TeamCheck = false;
        public static bool EnableESP = false;
        public static float BoxFillOpacity = 0.2f; // 20%
        public static bool FillBox = true;
        public static string[] Shapes =
            ["2D Box", "3D Box", "Edges", "Pyramid", "Star", "Hexagon", "Rhombus", "Pentagram", "Pentagon"];

        public static int CurrentShape = 0;
        public static bool EnableDistanceTracker = false;
        public static bool InnerOutline = false;
        public static bool OuterOutline = true;
        public static Vector2 InnerOutlineThickness = new(1f, 1f);
        public static Vector4 InnerOutlineColor = new(0, 0, 0, 1f);
        public static Vector2 OuterOutlineThickness = new(1.4f, 1.4f);
        public static Vector4 OutlineEnemyColor = new(1, 0, 0, 1);
        public static Vector4 OutlineTeamColor = new(0, 1, 0, 1);
        public static float Rounding = 0f;

        public static bool
            FlashCheck =
                true; // THIS APPLIES TO ALL VISUALS BESIDES LIKE ONES THAT DONT HAVE ANYTHING TO DO WITH THE ENTITIES

        public static float GlowAmount = 0f;
        public static bool BoxFillGradient = false;
        public static Vector4 BoxFillGradientColorTop = new(1f, 1f, 1f, BoxFillOpacity);
        public static Vector4 BoxFillGradientBottom = new(0f, 0f, 0f, BoxFillOpacity);
        public static float EdgeMultiplier = 0.25f;
        public static bool EnableESPPreview = true;
        public static Vector4 OccludedEnemy = new(1, 1, 0, 1);
        public static Vector4 OccludedTeam = new(0, 0, 1, 1);
        public static Vector4 EnemyFill = new(1, 0, 0, BoxFillOpacity);
        public static Vector4 TeamFill = new(0, 1, 0, BoxFillOpacity);

        public static void DrawBoxESP(Entity? entity, Renderer renderer)
        {
            if (!EnableESP || entity == null || (TeamCheck && entity.Team == GameState.LocalPlayer.Team) ||
                entity.PawnAddress == GameState.LocalPlayer.PawnAddress ||
                (FlashCheck && GameState.LocalPlayer.IsFlashed) || entity?.Bones2D?.Count < 0 ||
                entity?.Bones2D == null || entity.Position2D == new Vector2(-99, -99)) return;

            try
            {
                bool isTeam = entity.Team == GameState.LocalPlayer.Team;
                Vector4 boxColor = GetBoxColor(entity);
                Vector4 outlineColor = isTeam ? OutlineTeamColor : OutlineEnemyColor;
                float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                Vector4 fillColor = entity.Visible ? (isTeam ? TeamFill : EnemyFill) : (isTeam ? OccludedTeam : OccludedEnemy);
                fillColor.W = BoxFillOpacity;


                var preConvertedColor = ImGui.ColorConvertFloat4ToU32(boxColor);
                var preConvertedFillColor = ImGui.ColorConvertFloat4ToU32(fillColor);
                var preConvertedOutlineColor =
                    ImGui.ColorConvertFloat4ToU32(isTeam ? OutlineTeamColor : OutlineEnemyColor);
                //var min2D = Calculate.WorldToScreen(viewMatrix, entity.vecMin);
                //var max2D = Calculate.WorldToScreen(viewMatrix, entity.vecMax);

                float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                float halfWidth = entityHeight / 3f;
                float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
                Vector3 hitboxTop = entity.Position + new Vector3(0, 0, entity.VecMax.Z);
                Vector3 hitboxBottom = entity.Position + new Vector3(0, 0, entity.VecMin.Z);
                Vector2 top2D = Calculate.WorldToScreen(viewMatrix, hitboxTop);
                Vector2 bottom2D = Calculate.WorldToScreen(viewMatrix, hitboxBottom);

                if (top2D == new Vector2(-99, -99)) return;
                if (bottom2D == new Vector2(-99, -99)) return;

                float topY = top2D.Y;
                float bottomY = bottom2D.Y;
                float centerY = (topY + bottomY) / 2f;
                Vector2 rectTop = new(centerX - halfWidth, topY);
                Vector2 rectBottom = new(centerX + halfWidth, bottomY);
                float thickness = 2f;

                switch (CurrentShape)
                {
                    case 0: // 2D box
                    {
                        Draw2DBox(GameState.renderer.drawList, rectTop, rectBottom, preConvertedFillColor,
                            preConvertedOutlineColor, outlineColor);
                    }
                        break;

                    case 1: // 3D box
                    {
                        if (Draw3DBox(entity, viewMatrix, preConvertedOutlineColor, preConvertedFillColor, thickness)) return;
                    }
                        break;


                    case 2: //  edges
                    {
                        DrawEdgeBox(GameState.renderer.drawList, centerX, halfWidth, topY, bottomY, outlineColor,
                            preConvertedOutlineColor, preConvertedFillColor);
                    }
                        break;

                    case 3: // Pyramid
                    {
                        if (DrawPyramid(entity, viewMatrix, preConvertedOutlineColor, preConvertedFillColor, thickness)) return;
                        break;
                    }
                    case 4: // star of david
                    {
                        DrawStarOfDavid(renderer.drawList, bottomY, topY, centerX, centerY, outlineColor,
                            preConvertedOutlineColor, thickness);
                        break;
                    }
                    case 5: // hexagon
                    {
                        DrawPolygon(GameState.renderer.drawList, centerX, centerY, bottomY, topY, 6, outlineColor, preConvertedOutlineColor, 2);
                        break;
                    }
                    case 6: // rhombus
                    {
                        DrawPolygon(GameState.renderer.drawList, centerX, centerY, bottomY, topY, 4, outlineColor, preConvertedOutlineColor, 2);
                        break;
                    }
                    case 7: // pentagram
                    {
                        DrawPentagram(GameState.renderer.drawList, bottomY, topY, centerX, centerY, outlineColor, preConvertedOutlineColor);
                        break;
                    }
                    case 8: // pentagon
                    {
                        DrawPolygon(GameState.renderer.drawList, centerX, centerY, bottomY, topY, 5, outlineColor, preConvertedOutlineColor, 2);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception was thrown: {e}");
            }
        }

        private static void DrawPentagram(ImDrawListPtr imDrawListPtr, float bottomY, float topY, float centerX, float centerY,
            Vector4 outlineColor, uint preConvertedOutlineColor)
        {
            float radius = (bottomY - topY) / 2f;

            Vector2[] points = new Vector2[6];
            for (int i = 0; i < 6; i++)
            {
                points[i] = new Vector2(
                    centerX + radius * MathF.Cos(MathF.PI * i * 6 / 5 - MathF.PI / 2),
                    centerY + radius * MathF.Sin(MathF.PI * i * 6 / 5 - MathF.PI / 2)
                );
            }


            for (int i = 0; i < 6; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % 6];

                if (GlowAmount > 0f)
                    DrawHelpers.DrawGlowLine(imDrawListPtr, current, next, outlineColor,
                        GlowAmount);


                imDrawListPtr.AddLine(current, next, preConvertedOutlineColor);
            }
        }


        private static void DrawPolygon(ImDrawListPtr imDrawListPtr,float centerX, float centerY, float bottomY, float topY, int sides,
            Vector4 outlineColor, uint preConvertedOutlineColor, float thickness)
        {
            float radius = (bottomY - topY) / 2f;

            Vector2[] points = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                points[i] = new Vector2(
                    centerX + radius * MathF.Cos(MathF.PI * 2 * i / sides - MathF.PI * 2),
                    centerY + radius * MathF.Sin(MathF.PI * 2 * i / sides - MathF.PI * 2)
                );
            }


            for (int i = 0; i < sides; i++)
            {
                Vector2 current = points[i];
                Vector2 next = points[(i + 1) % sides];

                if (GlowAmount > 0f)
                {
                    DrawHelpers.DrawGlowLine(imDrawListPtr, current, next, outlineColor,
                        GlowAmount);
                }

                imDrawListPtr.AddLine(current, next, preConvertedOutlineColor);
            }
        }

        private static void DrawStarOfDavid(ImDrawListPtr imDrawListPtr, float bottomY, float topY, float centerX, float centerY,
            Vector4 boxColor, uint preConvertedOutlineColor, float thickness)
        {
            float radius = (bottomY - topY) / 2f;
            // 0.866 / sin(60°) is offset from center to each vertex
            Vector2 triangle1Top = new(centerX, centerY - radius);
            Vector2 triangle1BottomLeft = new(centerX - radius * 0.866f, centerY + radius * 0.5f);
            Vector2 triangle1BottomRight = new(centerX + radius * 0.866f, centerY + radius * 0.5f);

            Vector2 triangle2Bottom = new(centerX, centerY + radius);
            Vector2 triangle2TopLeft = new(centerX - radius * 0.866f, centerY - radius * 0.5f);
            Vector2 triangle2TopRight = new(centerX + radius * 0.866f, centerY - radius * 0.5f);

            if (GlowAmount > 0f)
            {
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle1Top, triangle1BottomLeft, boxColor,
                    GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle1BottomLeft, triangle1BottomRight,
                    boxColor, GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle1BottomRight, triangle1Top, boxColor,
                    GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle2Bottom, triangle2TopLeft, boxColor,
                    GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle2TopLeft, triangle2TopRight, boxColor,
                    GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, triangle2TopRight, triangle2Bottom, boxColor,
                    GlowAmount);
            }

            // triangle 1

            imDrawListPtr.AddLine(triangle1Top, triangle1BottomLeft, preConvertedOutlineColor,
                thickness);
            imDrawListPtr.AddLine(triangle1BottomLeft, triangle1BottomRight, preConvertedOutlineColor,
                thickness);
            imDrawListPtr.AddLine(triangle1BottomRight, triangle1Top, preConvertedOutlineColor,
                thickness);

            // triangle 2
            imDrawListPtr.AddLine(triangle2Bottom, triangle2TopLeft, preConvertedOutlineColor,
                thickness);
            imDrawListPtr.AddLine(triangle2TopLeft, triangle2TopRight, preConvertedOutlineColor,
                thickness);
            imDrawListPtr.AddLine(triangle2TopRight, triangle2Bottom, preConvertedOutlineColor,
                thickness);
        }

        private static bool DrawPyramid(Entity entity, float[] viewMatrix, uint preConvertedColor,
            uint preConvertedFillColor, float thickness)
        {
            float yaw = entity.AngEyeAngles.Y * (MathF.PI / 180f);
            float cos = MathF.Cos(yaw);
            float sin = MathF.Sin(yaw);

            Vector3[] corners3D =
            [
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMin.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMin.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMax.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMax.Y, entity.VecMin.Z, cos, sin),
                entity.Position + new Vector3(0, 0, entity.VecMax.Z)
            ];

            var corners2D = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return true;
            }

            GameState.renderer.drawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[1], corners2D[3], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[2], corners2D[0], preConvertedColor, thickness);

            GameState.renderer.drawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[1], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[2], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.drawList.AddLine(corners2D[3], corners2D[4], preConvertedColor, thickness);

            if (FillBox)
            {
                GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2],
                    preConvertedFillColor);
                GameState.renderer.drawList.AddTriangleFilled(corners2D[0], corners2D[1], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.drawList.AddTriangleFilled(corners2D[1], corners2D[3], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.drawList.AddTriangleFilled(corners2D[3], corners2D[2], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.drawList.AddTriangleFilled(corners2D[2], corners2D[0], corners2D[4],
                    preConvertedFillColor);
            }

            return false;
        }

        private static void DrawEdgeBox(ImDrawListPtr imDrawListPtr, float centerX, float halfWidth, float topY,
            float bottomY,
            Vector4 boxColor, uint preConvertedColor, uint preConvertedFillColor)
        {
            Vector2 rectTopLeft = new(centerX - halfWidth, topY);
            Vector2 rectTopRight = new(centerX + halfWidth, topY);
            Vector2 rectBottomLeft = new(centerX - halfWidth, bottomY);
            Vector2 rectBottomRight = new(centerX + halfWidth, bottomY);

            float edgeWidth = (rectTopRight.X - rectTopLeft.X) * EdgeMultiplier;
            float edgeHeight = (rectBottomLeft.Y - rectTopLeft.Y) * EdgeMultiplier;
            Vector2 topLeftSide = new(centerX + halfWidth, bottomY);
            //if (Outline)
            //    renderer.drawList.AddRect(rectTopLeft + OutlineThickness, topLeftSide + OutlineThickness, ImGui.ColorConvertFloat4ToU32(boxColor) & 0xFF000000, Rounding);

            if (GlowAmount > 0f)
            {
                DrawHelpers.DrawGlowLine(imDrawListPtr, rectTopLeft,
                    new(rectTopLeft.X + edgeWidth, rectTopLeft.Y), boxColor, GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, rectTopLeft,
                    new(rectTopLeft.X, rectTopLeft.Y + edgeHeight), boxColor, GlowAmount);

                DrawHelpers.DrawGlowLine(imDrawListPtr, rectTopRight,
                    new(rectTopRight.X - edgeWidth, rectTopRight.Y), boxColor, GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, rectTopRight,
                    new(rectTopRight.X, rectTopRight.Y + edgeHeight), boxColor, GlowAmount);

                DrawHelpers.DrawGlowLine(imDrawListPtr, rectBottomLeft,
                    new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), boxColor, GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, rectBottomLeft,
                    new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), boxColor, GlowAmount);

                DrawHelpers.DrawGlowLine(imDrawListPtr, rectBottomRight,
                    new(rectBottomRight.X - edgeWidth, rectBottomRight.Y), boxColor, GlowAmount);
                DrawHelpers.DrawGlowLine(imDrawListPtr, rectBottomRight,
                    new(rectBottomRight.X, rectBottomRight.Y - edgeHeight), boxColor, GlowAmount);
            }


            imDrawListPtr.AddLine(rectTopLeft, new(rectTopLeft.X + edgeWidth, rectTopLeft.Y),
                ImGui.ColorConvertFloat4ToU32(boxColor));
            imDrawListPtr.AddLine(rectTopLeft, new(rectTopLeft.X, rectTopLeft.Y + edgeHeight),
                ImGui.ColorConvertFloat4ToU32(boxColor));

            imDrawListPtr.AddLine(rectTopRight, new(rectTopRight.X - edgeWidth, rectTopRight.Y),
                ImGui.ColorConvertFloat4ToU32(boxColor));
            imDrawListPtr.AddLine(rectTopRight, new(rectTopRight.X, rectTopRight.Y + edgeHeight),
                preConvertedColor);

            imDrawListPtr.AddLine(rectBottomLeft, new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y),
                preConvertedColor);
            imDrawListPtr.AddLine(rectBottomLeft, new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight),
                preConvertedColor);

            imDrawListPtr.AddLine(rectBottomRight,
                new(rectBottomRight.X - edgeWidth, rectBottomRight.Y),
                preConvertedColor);
            imDrawListPtr.AddLine(rectBottomRight,
                new(rectBottomRight.X, rectBottomRight.Y - edgeHeight),
                preConvertedColor);

            if (FillBox)
                imDrawListPtr.AddRectFilled(rectTopLeft, rectBottomRight, preConvertedFillColor);
        }

        private static bool Draw3DBox(Entity entity, float[] viewMatrix, uint preConvertedOutlineColor, uint preConvertedFillColor, float rounding)
        {
            float yaw = entity.AngEyeAngles.Y * (MathF.PI / 180f);
            float cos = MathF.Cos(yaw);
            float sin = MathF.Sin(yaw);

            Vector3[] corners3D =
            [
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMin.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMin.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMax.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMax.Y, entity.VecMin.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMin.Y, entity.VecMax.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMin.Y, entity.VecMax.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMin.X, entity.VecMax.Y, entity.VecMax.Z, cos, sin),
                RotateCorner(entity.Position, entity.VecMax.X, entity.VecMax.Y, entity.VecMax.Z, cos, sin),
            ];

            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return true;
            }

            WorldESP.Draw3DBoxESP(corners2D, preConvertedOutlineColor, FillBox, rounding, preConvertedFillColor); // applicable here
            return false;
        }

        private static void Draw2DBox(ImDrawListPtr imDrawListPtr, Vector2 rectTop, Vector2 rectBottom,
            uint preConvertedFillColor, uint preConvertedOutlineColor, Vector4 outlineColor)
        {

            if (GlowAmount > 0f)
                DrawHelpers.DrawGlowRect(imDrawListPtr, rectTop, rectBottom, outlineColor, Rounding, GlowAmount);


            if (InnerOutline)
                imDrawListPtr.AddRect(rectTop - InnerOutlineThickness,
                    rectBottom + InnerOutlineThickness, ImGui.ColorConvertFloat4ToU32(InnerOutlineColor),
                    Rounding);

            if (BoxFillGradient)
                DrawHelpers.DrawGradientRect(imDrawListPtr, rectTop, rectBottom,
                    new(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z,
                        BoxFillOpacity),
                    new Vector4(BoxFillGradientBottom.X, BoxFillGradientBottom.Y, BoxFillGradientBottom.Z,
                        BoxFillOpacity), Rounding);
            else
            {
                if (FillBox)
                    imDrawListPtr.AddRectFilled(rectTop, rectBottom, preConvertedFillColor, Rounding);
            }

            if (OuterOutline)
            {
                imDrawListPtr.AddRect(rectTop + OuterOutlineThickness,
                    rectBottom + OuterOutlineThickness, preConvertedOutlineColor,
                    Rounding); // outside
            }
        }

        private static Vector4 GetBoxColor(Entity entity)
        {
            Vector4 boxColor = new(1, 1, 1, 1);

            if (BoneESP.visibilityCheck && !entity.Visible)
            {
                boxColor = entity.Team == GameState.LocalPlayer.Team ? OccludedTeam : OccludedEnemy;
            }
            else
            {
                boxColor = entity.Team == GameState.LocalPlayer.Team ? OutlineTeamColor : OutlineEnemyColor;
            }

            if (Colors.RGB)
            {
                boxColor = Colors.Rgb();
            }

            boxColor.W = BoxFillOpacity;
            if (BoxFillGradient)
            {
                boxColor = new(BoxFillGradientColorTop.X, BoxFillGradientColorTop.Y, BoxFillGradientColorTop.Z,
                    BoxFillOpacity);
            }

            return boxColor;
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

            if (Tracers.EnableTracers)
                Tracers.DrawTracerPreview(center);
        }

        public static void DrawDistancePreview(Vector2 position)
        {
            ImGui.GetWindowDrawList().AddText(position + new Vector2(0, 60),
                ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, 1)), "15m");
        }

        public static void DrawBoxPreview(Vector2 position)
        {
            if (!EnableESPPreview) return;

            float entityHeight = 200f;
            float halfWidth = entityHeight / 3f;
            float centerX = position.X;

            float topY = position.Y - entityHeight / 2;
            float bottomY = position.Y + entityHeight / 2;
            float centerY = (topY + bottomY) / 2f;

            Vector2 rectTop = new(centerX - halfWidth, topY);
            Vector2 rectBottom = new(centerX + halfWidth, bottomY);

            Vector4 boxColor = Colors.RGB ? Colors.Rgb() : Colors.EnemyColor;
            boxColor.W = BoxFillOpacity;

            Vector4 fillColor = boxColor;
            uint preConvertedOutlineColor = ImGui.ColorConvertFloat4ToU32(OutlineEnemyColor);

            var windowDrawList = ImGui.GetWindowDrawList();
            switch (CurrentShape)
            {
                case 0: // 2D box
                    Draw2DBox(windowDrawList, rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor),
                        ImGui.ColorConvertFloat4ToU32(OutlineEnemyColor), OutlineEnemyColor);
                    break;

                case 1: // 3D box preview
                {

                    float offset = halfWidth * 0.4f;

                    Vector2 frontTopLeft = new(centerX - halfWidth, topY);
                    Vector2 frontTopRight = new(centerX + halfWidth, topY);
                    Vector2 frontBottomLeft = new(centerX - halfWidth, bottomY);
                    Vector2 frontBottomRight = new(centerX + halfWidth, bottomY);

                    Vector2 backTopLeft = new(centerX - halfWidth + offset, topY - offset);
                    Vector2 backTopRight = new(centerX + halfWidth + offset, topY - offset);
                    Vector2 backBottomLeft = new(centerX - halfWidth + offset, bottomY - offset);
                    Vector2 backBottomRight = new(centerX + halfWidth + offset, bottomY - offset);


                    if (FillBox)
                    {
                        windowDrawList.AddQuadFilled(frontTopLeft, frontTopRight, frontBottomRight, frontBottomLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddQuadFilled(backTopLeft, backTopRight, backBottomRight, backBottomLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddQuadFilled(frontTopLeft, backTopLeft, backBottomLeft, frontBottomLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddQuadFilled(frontTopRight, backTopRight, backBottomRight, frontBottomRight,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddQuadFilled(frontTopLeft, frontTopRight, backTopRight, backTopLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddQuadFilled(frontBottomLeft, frontBottomRight, backBottomRight, backBottomLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                    }

                    windowDrawList.AddLine(frontTopLeft, frontTopRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontTopRight, frontBottomRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontBottomRight, frontBottomLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontBottomLeft, frontTopLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(backTopLeft, backTopRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(backTopRight, backBottomRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(backBottomRight, backBottomLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(backBottomLeft, backTopLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontTopLeft, backTopLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontTopRight, backTopRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontBottomLeft, backBottomLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(frontBottomRight, backBottomRight, preConvertedOutlineColor);
                    break;
                }

                case 2: // edges
                {
                    DrawEdgeBox(windowDrawList, centerX, halfWidth, topY, bottomY, OutlineEnemyColor,
                        ImGui.ColorConvertFloat4ToU32(OutlineEnemyColor), ImGui.ColorConvertFloat4ToU32(EnemyFill));
                }
                    break;
                case 3:
                {
                    float offset = halfWidth * 0.4f;
                    float apexX = centerX + offset * 0.5f;
                    float apexY = topY - halfWidth / 2;

                    Vector2 baseBottomLeft = new(centerX - halfWidth, bottomY);
                    Vector2 baseBottomRight = new(centerX + halfWidth, bottomY);
                    Vector2 baseBackLeft = new(centerX - halfWidth + offset, bottomY - offset);
                    Vector2 baseBackRight = new(centerX + halfWidth + offset, bottomY - offset);
                    Vector2 apex = new(apexX, apexY);

                    if (FillBox)
                    {
                        windowDrawList.AddQuadFilled(baseBottomLeft, baseBottomRight, baseBackRight, baseBackLeft,
                            ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddTriangleFilled(baseBottomLeft, baseBottomRight, apex, ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddTriangleFilled(baseBottomRight, baseBackRight, apex, ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddTriangleFilled(baseBackRight, baseBackLeft, apex, ImGui.ColorConvertFloat4ToU32(EnemyFill));
                        windowDrawList.AddTriangleFilled(baseBackLeft, baseBottomLeft, apex, ImGui.ColorConvertFloat4ToU32(EnemyFill));
                    }

                    windowDrawList.AddLine(baseBottomLeft, baseBottomRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBottomRight, baseBackRight, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBackRight, baseBackLeft, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBackLeft, baseBottomLeft, preConvertedOutlineColor);

                    windowDrawList.AddLine(baseBottomLeft, apex, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBottomRight, apex, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBackLeft, apex, preConvertedOutlineColor);
                    windowDrawList.AddLine(baseBackRight, apex, preConvertedOutlineColor);
                }
                    break;
                case 4:
                {
                    DrawStarOfDavid(windowDrawList, bottomY, topY, centerX, centerY, OutlineEnemyColor,
                        preConvertedOutlineColor, 2);
                }
                    break;
                case 5:
                {
                    DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 6, OutlineEnemyColor, preConvertedOutlineColor, 2);
                }
                    break;
                case 6:
                {
                    DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 4, OutlineEnemyColor, preConvertedOutlineColor, 2);
                    }
                    break;
                case 7:
                {
                    DrawPentagram(windowDrawList, bottomY, topY, centerX, centerY, OutlineEnemyColor, preConvertedOutlineColor);
                }
                    break;
                case 8:
                {
                    DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 5, OutlineEnemyColor, preConvertedOutlineColor, 2);
                    break;
                }
            }
        }

        private static Vector3 RotateCorner(Vector3 origin, float x, float y, float z, float cos, float sin)
        {
            return new Vector3(
                origin.X + x * cos - y * sin,
                origin.Y + x * sin + y * cos,
                origin.Z + z
            );
        }

        public static (Vector2 TopLeft, Vector2 BottomRight, Vector2 TopRight, Vector2 BottomLeft, Vector2 BottomMiddle)
            ? GetBoxRect(Entity? entity)
        {
            if (entity == null || entity.Position2D == Vector2.Zero || entity.ViewPosition2D == Vector2.Zero)
                return null;

            float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
            float halfWidth = entityHeight / 3f;
            float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
            float topY = entity.Bones2D != null && entity.Bones2D.Count > 2
                ? entity.Bones2D[2].Y
                : entity.ViewPosition2D.Y;
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