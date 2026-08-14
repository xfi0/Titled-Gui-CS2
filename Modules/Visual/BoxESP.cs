using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes.Math;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;
using Entity = Titled_Gui.Data.Entity.Entity;

namespace Titled_Gui.Modules.Visual
{
    public class BoxESP : IModule
    {
        public static bool TeamCheck = false;
        public static bool EnableESP = false;
        public static bool FillBox = true;
        public static string[] Shapes =
            ["2D Box", "3D Box", "Edges", "Pyramid", "Star", "Hexagon", "Rhombus", "Pentagram", "Pentagon"];

        public static int CurrentShape = 0;
        public static bool InnerOutline = false;
        public static bool OuterOutline = true;
        public static bool BoxFillGradient = false;
        public static bool EnableESPPreview = true;
        public static bool VisibilityCheck = true;
        public static float Rounding = 0f;
        public static bool FlashCheck = true; // THIS APPLIES TO ALL VISUALS BESIDES LIKE ONES THAT DONT HAVE ANYTHING TO DO WITH THE ENTITIES
        public static float GlowAmount = 0f;
        public static float EdgeMultiplier = 0.25f;
        public static Vector2 InnerOutlineThickness = new(1f, 1f);
        private static Vector2 OuterOutlineThickness = new(1.4f, 1.4f);
        private static Vector4 _occludedEnemy = new(1, 1, 0, 0.2f);
        private static Vector4 _occludedTeam = new(0, 0, 1, 0.2f);
        private static Vector4 _enemyFill = new(1, 0, 0, 0.2f);
        private static Vector4 _teamFill = new(0, 1, 0, 0.2f);
        private static Vector4 _boxFillGradientColorTop = new(1f, 1f, 1f, 0.2f);
        private static Vector4 _boxFillGradientBottom = new(0f, 0f, 0f, 0.2f);
        private static Vector4 _innerOutlineColor = new(0, 0, 0, 1f);
        private static Vector4 _outlineEnemyColor = new(1, 0, 0, 1);
        private static Vector4 _outlineTeamColor = new(0, 1, 0, 1);
        public static Colors FillColors = new(_teamFill, _enemyFill, null, null, false, false, false, false);
        public static Colors OutlineColors = new(_outlineTeamColor, _outlineEnemyColor, null, null, false, false, false, false);
        public static Colors InnerOutlineColors = new(_innerOutlineColor, _innerOutlineColor, null, null, false, false, false, false);
        public static Colors OccludedColors = new(_occludedTeam, _occludedEnemy, null, null, false, false, false, false);
        public static Colors GradientColors = new(_boxFillGradientColorTop, _boxFillGradientBottom, null, null, false, false, false, false);

        public static void DrawBoxESP(Entity? entity)
        {
            if (!EnableESP || entity == null || GameState.LocalPlayer == null ||
                (TeamCheck && entity.Team == GameState.LocalPlayer.Team) ||
                entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Health <= 0 ||
                (FlashCheck && GameState.LocalPlayer.IsFlashed) || entity?.Bones?.Count < 0 ||
                entity?.Bones == null || entity.Position2D == new Vector2(-99, -99) ||
                GameState.renderer == null || GameState.memory == null) return;

            try
            {
                bool isTeam = entity.Team == GameState.LocalPlayer.Team;
                Vector4 outlineColor = GetOutlineColor(isTeam);
                float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                Vector4 fillColor = GetFillColor(entity);

                var preConvertedFillColor = ImGui.ColorConvertFloat4ToU32(fillColor);
                var preConvertedOutlineColor = ImGui.ColorConvertFloat4ToU32(outlineColor);
                //var min2D = MathUtils.WorldToScreen(viewMatrix, entity.vecMin);
                //var max2D = MathUtils.WorldToScreen(viewMatrix, entity.vecMax);

                float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                float halfWidth = entityHeight / 3f;
                float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
                Vector3 hitboxTop = entity.Position + new Vector3(0, 0, entity.VecMax.Z);
                Vector3 hitboxBottom = entity.Position + new Vector3(0, 0, entity.VecMin.Z);
                Vector2 top2D = MathUtils.WorldToScreen(viewMatrix, hitboxTop);
                Vector2 bottom2D = MathUtils.WorldToScreen(viewMatrix, hitboxBottom);

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
                            Draw2DBox(GameState.renderer.DrawList, rectTop, rectBottom, preConvertedFillColor,
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
                            DrawEdgeBox(GameState.renderer.DrawList, centerX, halfWidth, topY, bottomY, outlineColor,
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
                            DrawStarOfDavid(GameState.renderer.DrawList, bottomY, topY, centerX, centerY, outlineColor,
                                preConvertedOutlineColor, thickness);
                            break;
                        }
                    case 5: // hexagon
                        {
                            DrawPolygon(GameState.renderer.DrawList, centerX, centerY, bottomY, topY, 6, outlineColor, preConvertedOutlineColor, 2);
                            break;
                        }
                    case 6: // rhombus
                        {
                            DrawPolygon(GameState.renderer.DrawList, centerX, centerY, bottomY, topY, 4, outlineColor, preConvertedOutlineColor, 2);
                            break;
                        }
                    case 7: // pentagram
                        {
                            DrawPentagram(GameState.renderer.DrawList, bottomY, topY, centerX, centerY, outlineColor, preConvertedOutlineColor);
                            break;
                        }
                    case 8: // pentagon
                        {
                            DrawPolygon(GameState.renderer.DrawList, centerX, centerY, bottomY, topY, 5, outlineColor, preConvertedOutlineColor, 2);
                            break;
                        }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"An exception was thrown: {e}");
            }
        }

        private static Vector4 GetOutlineColor(bool isTeam)
        {
            if (isTeam)
                return OutlineColors.TeamRGB ? Colors.Rgb(OutlineColors.TeamColor.W) : OutlineColors.TeamColor;
            else
                return OutlineColors.EnemyRGB ? Colors.Rgb(OutlineColors.EnemyColor.W) : OutlineColors.EnemyColor;
        }

        private static Vector4 GetFillColor(Entity entity)
        {
            Colors c = (VisibilityCheck && !entity.Visible) ? OccludedColors : FillColors;
            if (entity.IsTeammate)
                return c.TeamRGB ? Colors.Rgb(c.TeamColor.W) : c.TeamColor;

            else
                return c.EnemyRGB ? Colors.Rgb(c.EnemyColor.W) : c.EnemyColor;
        }

        private static Vector4 GetInnerOutlineColor()
        {
            return InnerOutlineColors.TeamRGB ? Colors.Rgb(InnerOutlineColors.TeamColor.W) : InnerOutlineColors.TeamColor;
        }

        private static Vector4 GetGradientColor(bool top)
        {
            if (top)
                return GradientColors.TeamRGB ? Colors.Rgb(GradientColors.TeamColor.W) : GradientColors.TeamColor;
            else
                return GradientColors.EnemyRGB ? Colors.Rgb(GradientColors.EnemyColor.W) : GradientColors.EnemyColor;
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
                    GlowRenderer.DrawGlowLine(imDrawListPtr, current, next, outlineColor,
                        GlowAmount);


                imDrawListPtr.AddLine(current, next, preConvertedOutlineColor);
            }
        }


        private static void DrawPolygon(ImDrawListPtr imDrawListPtr, float centerX, float centerY, float bottomY, float topY, int sides,
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
                    GlowRenderer.DrawGlowLine(imDrawListPtr, current, next, outlineColor,
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
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle1Top, triangle1BottomLeft, boxColor,
                    GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle1BottomLeft, triangle1BottomRight,
                    boxColor, GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle1BottomRight, triangle1Top, boxColor,
                    GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle2Bottom, triangle2TopLeft, boxColor,
                    GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle2TopLeft, triangle2TopRight, boxColor,
                    GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, triangle2TopRight, triangle2Bottom, boxColor,
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

        private static bool DrawPyramid(Entity entity, float[] viewMatrix, uint preConvertedColor, uint preConvertedFillColor, float thickness)
        {
            if (GameState.renderer == null)
                return true;

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
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99))
                    return true;
            }

            GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[3], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[0], preConvertedColor, thickness);

            GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[4], preConvertedColor, thickness);
            GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[4], preConvertedColor, thickness);

            if (FillBox)
            {
                GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2],
                    preConvertedFillColor);
                GameState.renderer.DrawList.AddTriangleFilled(corners2D[0], corners2D[1], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.DrawList.AddTriangleFilled(corners2D[1], corners2D[3], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.DrawList.AddTriangleFilled(corners2D[3], corners2D[2], corners2D[4],
                    preConvertedFillColor);
                GameState.renderer.DrawList.AddTriangleFilled(corners2D[2], corners2D[0], corners2D[4],
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
                GlowRenderer.DrawGlowLine(imDrawListPtr, rectTopLeft,
                    new(rectTopLeft.X + edgeWidth, rectTopLeft.Y), boxColor, GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, rectTopLeft,
                    new(rectTopLeft.X, rectTopLeft.Y + edgeHeight), boxColor, GlowAmount);

                GlowRenderer.DrawGlowLine(imDrawListPtr, rectTopRight,
                    new(rectTopRight.X - edgeWidth, rectTopRight.Y), boxColor, GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, rectTopRight,
                    new(rectTopRight.X, rectTopRight.Y + edgeHeight), boxColor, GlowAmount);

                GlowRenderer.DrawGlowLine(imDrawListPtr, rectBottomLeft,
                    new(rectBottomLeft.X + edgeWidth, rectBottomLeft.Y), boxColor, GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, rectBottomLeft,
                    new(rectBottomLeft.X, rectBottomLeft.Y - edgeHeight), boxColor, GlowAmount);

                GlowRenderer.DrawGlowLine(imDrawListPtr, rectBottomRight,
                    new(rectBottomRight.X - edgeWidth, rectBottomRight.Y), boxColor, GlowAmount);
                GlowRenderer.DrawGlowLine(imDrawListPtr, rectBottomRight,
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
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return true;
            }

            Draw3DBoxESP(corners2D, preConvertedOutlineColor, FillBox, rounding, preConvertedFillColor); // applicable here - no it was not
            return false;
        }

        public static void Draw3DBoxESP(Vector2[] corners2D, uint preConvertedColor, bool filled, float rounding, uint preConvertedFilledColor = 0)
        {
            if (GameState.renderer == null)
                return;

            try
            {
                if (filled)
                {
                    // bottom face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[1], corners2D[1], preConvertedFilledColor);
                    // top face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[4], corners2D[5], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // front face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[5], corners2D[4], preConvertedFilledColor);
                    // back face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[2], corners2D[3], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // left face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[2], corners2D[6], corners2D[4], preConvertedFilledColor);
                    // right face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[1], corners2D[3], corners2D[7], corners2D[5], preConvertedFilledColor);
                }

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[3], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[0], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[4], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[5], corners2D[7], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[7], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[6], corners2D[4], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[7], preConvertedColor, rounding);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Index Out Of Bounds Of The Array Drawing 3D Boxes");
            }
        }

        private static void Draw2DBox(ImDrawListPtr imDrawListPtr, Vector2 rectTop, Vector2 rectBottom,
            uint preConvertedFillColor, uint preConvertedOutlineColor, Vector4 outlineColor)
        {

            if (GlowAmount > 0f)
                GlowRenderer.DrawGlowRect(imDrawListPtr, rectTop, rectBottom, outlineColor, Rounding, GlowAmount);


            if (InnerOutline)
                imDrawListPtr.AddRect(rectTop - InnerOutlineThickness,
                    rectBottom + InnerOutlineThickness, ImGui.ColorConvertFloat4ToU32(InnerOutlineColors.EnemyColor),
                    Rounding);

            if (BoxFillGradient)
                ShapeRenderer.DrawGradientRect(imDrawListPtr, rectTop, rectBottom, GetGradientColor(true), GetGradientColor(false), Rounding);
            else
            {
                if (FillBox)
                    imDrawListPtr.AddRectFilled(rectTop, rectBottom, preConvertedFillColor, Rounding);
            }

            if (OuterOutline)
            {
                imDrawListPtr.AddRect(rectTop - OuterOutlineThickness,
                    rectBottom + OuterOutlineThickness, preConvertedOutlineColor,
                    Rounding); // outside
            }
        }

        public static void RenderESPPreview(Vector2 center)
        {
            if (!EnableESPPreview) return;
            float entityHeight = 225f;
            Vector2 windowPos = ImGui.GetWindowPos();

            if (EnableESP)
                DrawBoxPreview(center + windowPos, entityHeight);

            if (DistanceText.Enabled)
                DistanceText.DrawDistancePreview(center + windowPos, entityHeight);

            if (BoneESP.EnableBoneESP)
                BoneESP.DrawBonePreview(center + windowPos, entityHeight);

            if (HealthBar.EnableHealthBar)
                HealthBar.DrawHealthBarPreview(center + windowPos, entityHeight);

            if (ArmorBar.EnableArmorBar)
                ArmorBar.DrawArmorBarPreview(center + windowPos, entityHeight);

            if (NameDisplay.Enabled)
                NameDisplay.DrawNamePreview(center + windowPos, entityHeight);

            if (Tracers.EnableTracers)
                Tracers.DrawTracerPreview(center + windowPos, entityHeight);
        }

        public static void DrawBoxPreview(Vector2 position, float entityHeight)
        {
            float halfWidth = entityHeight / 3f;
            float centerX = position.X;

            float topY = position.Y - entityHeight / 2;
            float bottomY = position.Y + entityHeight / 2;
            float centerY = (topY + bottomY) / 2f;

            Vector2 rectTop = new(centerX - halfWidth, topY);
            Vector2 rectBottom = new(centerX + halfWidth, bottomY);

            Vector4 fillColor = (BoneESP.visibilityCheck && false) ? OccludedColors.EnemyColor : FillColors.EnemyColor;
            uint preConvertedOutlineColor = ImGui.ColorConvertFloat4ToU32(OutlineColors.EnemyColor);

            var windowDrawList = ImGui.GetWindowDrawList();
            switch (CurrentShape)
            {
                case 0: // 2D box
                    Draw2DBox(windowDrawList, rectTop, rectBottom, ImGui.ColorConvertFloat4ToU32(fillColor),
                        preConvertedOutlineColor, OutlineColors.EnemyColor);
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
                                ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddQuadFilled(backTopLeft, backTopRight, backBottomRight, backBottomLeft,
                                ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddQuadFilled(frontTopLeft, backTopLeft, backBottomLeft, frontBottomLeft,
                                ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddQuadFilled(frontTopRight, backTopRight, backBottomRight, frontBottomRight,
                                ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddQuadFilled(frontTopLeft, frontTopRight, backTopRight, backTopLeft,
                                ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddQuadFilled(frontBottomLeft, frontBottomRight, backBottomRight, backBottomLeft,
                                ImGui.ColorConvertFloat4ToU32(fillColor));
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
                        DrawEdgeBox(windowDrawList, centerX, halfWidth, topY, bottomY, OutlineColors.EnemyColor,
                            preConvertedOutlineColor, ImGui.ColorConvertFloat4ToU32(fillColor));
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
                            windowDrawList.AddQuadFilled(baseBottomLeft, baseBottomRight, baseBackRight, baseBackLeft, ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddTriangleFilled(baseBottomLeft, baseBottomRight, apex, ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddTriangleFilled(baseBottomRight, baseBackRight, apex, ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddTriangleFilled(baseBackRight, baseBackLeft, apex, ImGui.ColorConvertFloat4ToU32(fillColor));
                            windowDrawList.AddTriangleFilled(baseBackLeft, baseBottomLeft, apex, ImGui.ColorConvertFloat4ToU32(fillColor));
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
                        DrawStarOfDavid(windowDrawList, bottomY, topY, centerX, centerY, OutlineColors.EnemyColor,
                            preConvertedOutlineColor, 2);
                    }
                    break;
                case 5:
                    {
                        DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 6, OutlineColors.EnemyColor, preConvertedOutlineColor, 2);
                    }
                    break;
                case 6:
                    {
                        DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 4, OutlineColors.EnemyColor, preConvertedOutlineColor, 2);
                    }
                    break;
                case 7:
                    {
                        DrawPentagram(windowDrawList, bottomY, topY, centerX, centerY, OutlineColors.EnemyColor, preConvertedOutlineColor);
                    }
                    break;
                case 8:
                    {
                        DrawPolygon(windowDrawList, centerX, centerY, bottomY, topY, 5, OutlineColors.EnemyColor, preConvertedOutlineColor, 2);
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

        public static BoxRect? GetBoxRect(Entity? entity)
        {
            if (entity == null || entity.Position2D == Vector2.Zero || entity.Position2D == new Vector2(-99, -99) || entity.ViewPosition2D == Vector2.Zero || entity.ViewPosition2D == new Vector2(-99, -99) || GameState.memory == null)
                return null;

            float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
            float halfWidth = entityHeight / 3f;
            float centerX = (entity.ViewPosition2D.X + entity.Position2D.X) / 2f;
            Vector3 hitboxTop = entity.Position + new Vector3(0, 0, entity.VecMax.Z);
            Vector3 hitboxBottom = entity.Position + new Vector3(0, 0, entity.VecMin.Z);

            Vector2 top2D = MathUtils.WorldToScreen(viewMatrix, hitboxTop);
            Vector2 bottom2D = MathUtils.WorldToScreen(viewMatrix, hitboxBottom);

            if (top2D == new Vector2(-99, -99) || bottom2D == new Vector2(-99, -99))
                return null;

            float topY = top2D.Y;
            float bottomY = bottom2D.Y;

            Vector2 topLeft = new(centerX - halfWidth, topY);
            Vector2 topRight = new(centerX + halfWidth, topY);
            Vector2 bottomLeft = new(centerX - halfWidth, bottomY);
            Vector2 bottomRight = new(centerX + halfWidth, bottomY);
            Vector2 bottomMiddle = new(centerX, bottomY);

            return new(topLeft, bottomRight, topRight, bottomLeft, bottomMiddle);
        }
    }
}