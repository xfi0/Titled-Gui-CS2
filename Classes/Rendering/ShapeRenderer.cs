using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes.Math;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes.Rendering
{
    internal class ShapeRenderer
    {
        public static void Draw3DCircle(float[] viewMatrix, Vector3 center, float radius, Vector3 normal, uint color, int segments = 32)
        {
            if (GameState.renderer == null)
                return;

            Vector3 up = MathF.Abs(normal.Z) < 0.999f ? Vector3.UnitZ : Vector3.UnitX;
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, normal));
            Vector3 forward = Vector3.Normalize(Vector3.Cross(normal, right));

            List<Vector2> positions2D = new();

            for (int i = 0; i < segments; i++)
            {
                float angle = i * MathF.PI * 2f / segments;
                Vector3 worldPoint = center + right * MathF.Cos(angle) * radius + forward * MathF.Sin(angle) * radius;

                Vector2 screen = MathUtils.WorldToScreen(viewMatrix, worldPoint);
                if (screen == new Vector2(-99, -99))
                {
                    positions2D.Clear();
                    continue;
                }

                positions2D.Add(screen);
            }

            for (int i = 0; i < positions2D.Count; i++)
            {
                Vector2 a = positions2D[i];
                Vector2 b = positions2D[(i + 1) % positions2D.Count];
                GameState.renderer.DrawList.AddLine(a, b, color, 2f);
            }
        }
        public static void Draw3DSphere(float[] viewMatrix, Vector3 center, float radius, uint color, int segments = 32)
        {
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitX, color, segments);
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitY, color, segments);
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitZ, color, segments);
        }
        public static void DrawConvexHull(List<Vector2> points, uint fill, uint outline, float thickness = 2f)
        {
            if (GameState.renderer == null)
                return;

            points.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            List<Vector2> lower = [];
            List<Vector2> upper = [];

            foreach (var p in points)
            {
                while (lower.Count >= 2)
                {
                    var p1 = lower[^2]; var p2 = lower[^1];
                    if ((p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X) > 0f)
                        break;
                    lower.RemoveAt(lower.Count - 1);
                }
                lower.Add(p);
            }

            for (int i = points.Count - 1; i >= 0; i--)
            {
                var p = points[i];
                while (upper.Count >= 2)
                {
                    var p1 = upper[^2]; var p2 = upper[^1];
                    if ((p2.X - p1.X) * (p.Y - p1.Y) - (p2.Y - p1.Y) * (p.X - p1.X) > 0f)
                        break;
                    upper.RemoveAt(upper.Count - 1);
                }
                upper.Add(p);
            }

            lower.RemoveAt(lower.Count - 1);
            upper.RemoveAt(upper.Count - 1);
            lower.AddRange(upper);

            if (lower.Count < 3)
                return;

            var hullArray = lower.ToArray();
            GameState.renderer.DrawList.AddConvexPolyFilled(ref hullArray[0], hullArray.Length, fill);
            GameState.renderer.DrawList.AddPolyline(ref hullArray[0], hullArray.Length, outline, ImDrawFlags.Closed, thickness);
        }
        // https://www.unknowncheats.me/forum/counter-strike-2-a/732412-external-hitbox-overlay-aka-chams.html
        public static void DrawCapsule3D(Vector3 vMin, Vector3 vMax, float oRadius, Quaternion rotation, Vector3 origPos, float[] viewMatrix, uint color, int segments = 12, float thickness = 1.0f)
        {
            if (GameState.renderer == null)
                return;

            Vector3 bottom = origPos + Vector3.Transform(vMax, rotation);
            Vector3 top = origPos + Vector3.Transform(vMin, rotation);
            Vector3 point = MathUtils.Extend(top, bottom, Vector3.Distance(top, bottom) * 2);

            float radius = oRadius;
            float radiusHalf = radius * 0.70710678f;

            var topSmallCircle = new List<Vector3>();
            var topCircle = new List<Vector3>();
            var bottomSmallCircle = new List<Vector3>();
            var bottomCircle = new List<Vector3>();

            CreateCircle(MathUtils.Extend(top, bottom, -radiusHalf), point, radiusHalf, topSmallCircle, segments);
            CreateCircle(top, point, radius, topCircle, segments);
            CreateCircle(MathUtils.Extend(bottom, top, -radiusHalf), point, radiusHalf, bottomSmallCircle, segments);
            CreateCircle(bottom, point, radius, bottomCircle, segments);

            var worldPoints = new List<Vector3>(segments * 4 + 2);
            worldPoints.Add(MathUtils.Extend(top, bottom, -radius));
            worldPoints.Add(MathUtils.Extend(bottom, top, -radius));
            worldPoints.AddRange(topSmallCircle);
            worldPoints.AddRange(topCircle);
            worldPoints.AddRange(bottomSmallCircle);
            worldPoints.AddRange(bottomCircle);

            var screenPoints = new List<Vector2>(worldPoints.Count);
            foreach (var wp in worldPoints)
            {
                var sp = MathUtils.WorldToScreen(viewMatrix, wp);
                if (sp != new Vector2(-99, -99))
                    screenPoints.Add(sp);
            }

            if (screenPoints.Count < 3) return;

            var hull = ConvexHull(screenPoints);
            if (hull.Count < 3) return;

            uint fillColor = color & 0x00FFFFFF | 0x55000000;
            uint outlineColor = color;

            var drawList = GameState.renderer.DrawList;
            var hullArr = hull.ToArray();

            drawList.AddConvexPolyFilled(ref hullArr[0], hull.Count, fillColor);
            drawList.AddPolyline(ref hullArr[0], hull.Count, outlineColor, ImDrawFlags.Closed, thickness);
        }

        private static List<Vector2> ConvexHull(List<Vector2> points)
        {
            int n = points.Count;
            if (n < 3)
                return points;

            points.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            var hull = new List<Vector2>(n);

            foreach (var p in points)
            {
                while (hull.Count >= 2 && MathUtils.Cross(hull[^2], hull[^1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);

                hull.Add(p);
            }

            int lowerSize = hull.Count;
            for (int i = n - 2; i >= 0; i--)
            {
                while (hull.Count > lowerSize && MathUtils.Cross(hull[^2], hull[^1], points[i]) <= 0)
                    hull.RemoveAt(hull.Count - 1);

                hull.Add(points[i]);
            }

            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private static void CreateCircle(Vector3 point, Vector3 center, float radius, List<Vector3> vec,
            int segments = 12)
        {
            vec.Clear();

            Vector3 normal = Vector3.Normalize(point - center);

            Vector3 arbitrary = (MathF.Abs(normal.X) < 0.99f) ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);
            Vector3 u = Vector3.Normalize(Vector3.Cross(normal, arbitrary));
            Vector3 v = Vector3.Normalize(Vector3.Cross(normal, u));

            for (int i = 0; i <= segments; i++)
            {
                float angle = (2.0f * MathF.PI * i) / segments;
                Vector3 circlePoint = point + (u * MathF.Cos(angle) + v * MathF.Sin(angle)) * radius;
                vec.Add(circlePoint);
            }
        }
        public static void DrawGradientRect(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom,
           Vector4 colorStart, Vector4 colorEnd, float rounding = 0f)
        {
            uint topColor = ImGui.ColorConvertFloat4ToU32(colorStart);
            uint bottomColor = ImGui.ColorConvertFloat4ToU32(colorEnd);
            drawList.AddRectFilledMultiColor(rectTop, rectBottom, topColor, topColor, bottomColor, bottomColor);
        }
    }
}
