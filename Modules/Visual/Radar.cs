using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using ImGuiNET;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{

    public class RadarPoint
    {
        public Vector2 ScreenPos;
        public Color Col;
        public int Type;
        public float Yaw;
        public RadarPoint(Vector2 p, Color c, int t, float y) { ScreenPos = p; Col = c; Type = t; Yaw = y; }
    }

    internal class Radar : Classes.ThreadService
    {
        public static bool IsEnabled = true;
        public static float Size = 200f;            // radar diameter in pixels
        public static float RenderRange = 100f;     // world units shown radius
        public static float Proportion = 2.0f;      // scale divisor from your C++ code
        public static Vector2 Pos = new Vector2(200, 200); // center on screen
        public static float LocalYaw = 0f;          // player yaw in degrees
        public static float CircleSize = 3f;
        public static float ArrowSize = 12f;
        public static float ArcArrowSize = 10f;
        public static bool ShowCrossLine = true;
        public static Color CrossColor = Color.FromArgb(200, 255, 255, 255);
        public static bool Opened = true;

        private static readonly List<RadarPoint> Points = new List<RadarPoint>();

        private static Vector2 RevolveCoordinatesSystem(float revolveAngleDeg, Vector2 origin, Vector2 dest)
        {
            if (Math.Abs(revolveAngleDeg) < 1e-6f) return dest;
            float rad = revolveAngleDeg * (float)Math.PI / 180f;
            float c = (float)Math.Cos(rad), s = (float)Math.Sin(rad);
            float dx = dest.X - origin.X;
            float dy = dest.Y - origin.Y;
            return new Vector2(origin.X + dx * c + dy * s, origin.Y - dx * s + dy * c);
        }

        public static void SetPos(Vector2 p) => Pos = p;
        public static void SetSize(float s) => Size = s;
        public static float GetSize() => Size;
        public static void SetRange(float r) => RenderRange = r;
        public static void SetProportion(float p) => Proportion = p;
        public static void SetLocalYaw(float yaw) => LocalYaw = yaw;

        public static void AddPoint(Vector3 localPos, float localYaw, Vector3 worldPos, Color col, int type, float yaw)
        {
            LocalYaw = localYaw;

            float dx = localPos.X - worldPos.X;
            float dy = localPos.Y - worldPos.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            // angle from local to target, converted to radians; keep C++ ordering
            float angleRad = (localYaw * ((float)Math.PI / 180f)) - (float)Math.Atan2(worldPos.Y - localPos.Y, worldPos.X - localPos.X);

            float scale = (2.0f * RenderRange) / Proportion;
            distance *= scale;

            Vector2 pointPos = new Vector2(
                Pos.X + distance * (float)Math.Sin(angleRad),
                Pos.Y - distance * (float)Math.Cos(angleRad)
            );

            float renderRange = RenderRange;
            if (pointPos.X < Pos.X - renderRange || pointPos.X > Pos.X + renderRange ||
                pointPos.Y > Pos.Y + renderRange || pointPos.Y < Pos.Y - renderRange)
            {
                return;
            }

            Points.Add(new RadarPoint(pointPos, col, type, yaw));
        }

        private static void DrawTriangle(ImDrawListPtr dl, Vector2 center, Color col, float width, float height, float yaw)
        {
            Vector2 a = new Vector2(center.X - width / 2f, center.Y);
            Vector2 b = new Vector2(center.X + width / 2f, center.Y);
            Vector2 c = new Vector2(center.X, center.Y - height);

            a = RevolveCoordinatesSystem(-yaw, center, a);
            b = RevolveCoordinatesSystem(-yaw, center, b);
            c = RevolveCoordinatesSystem(-yaw, center, c);

            uint ucol = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f));
            dl.AddTriangleFilled(a, b, c, ucol);
        }

        public static void DrawRadar()
        {
            if (!IsEnabled) return;
            if (Size <= 0) return;

            ImDrawListPtr dl = GameState.renderer.drawList; // or GameState.renderer.drawList if you have one

            float half = Size * 0.5f;
            Vector2 left = new Vector2(Pos.X - half, Pos.Y);
            Vector2 right = new Vector2(Pos.X + half, Pos.Y);
            Vector2 top = new Vector2(Pos.X, Pos.Y - half);
            Vector2 bottom = new Vector2(Pos.X, Pos.Y + half);

            if (Opened)
            {
                if (ShowCrossLine)
                {
                    uint crossCol = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(CrossColor.R / 255f, CrossColor.G / 255f, CrossColor.B / 255f, CrossColor.A / 255f));
                    dl.AddLine(left, right, crossCol, 1f);
                    dl.AddLine(top, bottom, crossCol, 1f);
                }

                float degToRad = (float)Math.PI / 180f;

                foreach (var p in Points)
                {
                    if (p == null) return;

                    Vector2 imP = p.ScreenPos;
                    uint pcol = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(p.Col.R / 255f, p.Col.G / 255f, p.Col.B / 255f, p.Col.A / 255f));

                    if (p.Type == 0)
                    {
                        dl.AddCircle(imP, CircleSize, pcol, 12, 1f);
                        uint black = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0, 0, 1));
                        dl.AddCircleFilled(imP, CircleSize, black, 12);
                    }
                    else if (p.Type == 1)
                    {
                        float angle = (LocalYaw - p.Yaw) + 180f;
                        Vector2 rePoint = RevolveCoordinatesSystem(angle, Pos, p.ScreenPos);

                        Vector2 re_a = new Vector2(rePoint.X, rePoint.Y + ArrowSize);
                        Vector2 re_b = new Vector2(rePoint.X - ArrowSize / 1.5f, rePoint.Y - ArrowSize / 2f);
                        Vector2 re_c = new Vector2(rePoint.X + ArrowSize / 1.5f, rePoint.Y - ArrowSize / 2f);

                        Vector2 a = RevolveCoordinatesSystem(-angle, Pos, re_a);
                        Vector2 b = RevolveCoordinatesSystem(-angle, Pos, re_b);
                        Vector2 c = RevolveCoordinatesSystem(-angle, Pos, re_c);

                        dl.AddQuadFilled(a, b, imP, c, pcol);
                        uint outline = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0, 0, 150 / 255f));
                        dl.AddQuad(a, b, imP, c, outline, 0.1f);
                    }
                    else
                    {
                        float angle = (LocalYaw - p.Yaw) - 90f;
                        uint fillCol = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(p.Col.R / 255f, p.Col.G / 255f, p.Col.B / 255f, p.Col.A / 255f));
                        uint outline = (uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(0, 0, 0, 150 / 255f));

                        dl.AddCircleFilled(imP, 0.85f * ArcArrowSize, fillCol, 30);
                        dl.AddCircle(imP, 0.95f * ArcArrowSize, outline, 30, 0.1f);

                        Vector2 tri = new Vector2(
                            p.ScreenPos.X + (ArcArrowSize + ArcArrowSize / 3f) * (float)Math.Cos(-angle * degToRad),
                            p.ScreenPos.Y - (ArcArrowSize + ArcArrowSize / 3f) * (float)Math.Sin(-angle * degToRad)
                        );
                        Vector2 tri1 = new Vector2(
                            p.ScreenPos.X + ArcArrowSize * (float)Math.Cos(-(angle - 30f) * degToRad),
                            p.ScreenPos.Y - ArcArrowSize * (float)Math.Sin(-(angle - 30f) * degToRad)
                        );
                        Vector2 tri2 = new Vector2(
                            p.ScreenPos.X + ArcArrowSize * (float)Math.Cos(-(angle + 30f) * degToRad),
                            p.ScreenPos.Y - ArcArrowSize * (float)Math.Sin(-(angle + 30f) * degToRad)
                        );

                        dl.PathLineTo(tri);
                        dl.PathLineTo(tri1);
                        dl.PathLineTo(tri2);
                        dl.PathFillConvex((uint)ImGui.ColorConvertFloat4ToU32(new System.Numerics.Vector4(220 / 255f, 220 / 255f, 220 / 255f, 240 / 255f)));
                    }
                }
            }

            Points.Clear();
        }

        protected override void FrameAction()
        {
            foreach (var e in GameState.Entities)
            {
                AddPoint(GameState.localPlayer.Position, GameState.localPlayer.ViewAngles.Y, e.Position, Color.White, 0, e.ViewAngles.Y);
            }
        }
    }
}
