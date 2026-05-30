using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes
{
    internal class DrawHelpers
    {
        public static float now = (float)ImGui.GetTime();

        public static void DrawGlowRect(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom, Vector4 color,
            float rounding, float glowAmount, int layers = 12)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                drawList.AddRect(
                    new Vector2(rectTop.X - expansion, rectTop.Y - expansion),
                    new Vector2(rectBottom.X + expansion, rectBottom.Y + expansion),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha)),
                    rounding + expansion, ImDrawFlags.None, glowAmount * 2f);
            }
        }

        public static void DrawGlowRectFilled(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom,
            Vector4 color,
            float rounding, float glowAmount, int layers = 12)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                drawList.AddRectFilled(
                    new Vector2(rectTop.X - expansion, rectTop.Y - expansion),
                    new Vector2(rectBottom.X + expansion, rectBottom.Y + expansion),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha)),
                    rounding + expansion);
            }
        }

        public static void DrawGlowLine(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector4 color,
            float glowAmount, int layers = 12, float thickness = 0f)
        {
            Vector2 dir = Vector2.Normalize(p2 - p1);
            Vector2 normal = new(-dir.Y, dir.X);

            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float offset = glowAmount * t * 1.25f;
                float alpha = color.W * 0.075f * (1f - t);
                if (alpha < 0.001f) continue;

                uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha));
                var off = normal * offset;
                float th = thickness == 0f ? 1f : thickness;

                drawList.AddLine(p1 - off, p2 - off, col, th);
                drawList.AddLine(p1 + off, p2 + off, col, th);
            }
        }

        public static void DrawGlowCircle(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color,
            float glowAmount, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = radius + glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                drawList.AddCircle(center, expansion,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha)));
            }
        }

        public static void DrawGlowCircleFilled(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color,
            float glowAmount, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = radius + glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                drawList.AddCircleFilled(center, expansion,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha)));
            }
        }

        public static void DrawGlowText(ImDrawListPtr drawList, Vector2 pos, Vector4 color, string text,
            float glowAmount, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha));
                drawList.AddText(pos + new Vector2(-expansion, -expansion), col, text);
                drawList.AddText(pos + new Vector2(expansion, -expansion), col, text);
                drawList.AddText(pos + new Vector2(-expansion, expansion), col, text);
                drawList.AddText(pos + new Vector2(expansion, expansion), col, text);
            }
        }

        public static void DrawGlowBezier(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            Vector4 color, float glowAmount, float thickness, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                var offset = new Vector2(expansion, expansion);
                drawList.AddBezierCubic(p1 - offset, p2 - offset, p3 + offset, p4 + offset,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha)),
                    thickness + expansion);
            }
        }

        public static void DrawGlowQuad(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            Vector4 color, float glowAmount, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha));
                drawList.AddQuad(
                    new Vector2(p1.X - expansion, p1.Y - expansion),
                    new Vector2(p2.X + expansion, p2.Y - expansion),
                    new Vector2(p3.X + expansion, p3.Y + expansion),
                    new Vector2(p4.X - expansion, p4.Y + expansion), col);
            }
        }

        public static void DrawGlowQuadFilled(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4,
            Vector4 color, float glowAmount, int layers = 8)
        {
            for (int i = 1; i <= layers; i++)
            {
                float t = i / (float)layers;
                float expansion = glowAmount * t * 1.25f;
                float alpha = color.W * 0.025f * (1f - t);
                if (alpha < 0.001f) continue;

                uint col = ImGui.ColorConvertFloat4ToU32(new Vector4(color.X, color.Y, color.Z, alpha));
                drawList.AddQuadFilled(
                    new Vector2(p1.X - expansion, p1.Y - expansion),
                    new Vector2(p2.X + expansion, p2.Y - expansion),
                    new Vector2(p3.X + expansion, p3.Y + expansion),
                    new Vector2(p4.X - expansion, p4.Y + expansion), col);
            }
        }

        public static void DrawGradientRect(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom,
            Vector4 colorStart, Vector4 colorEnd, float rounding = 0f)
        {
            uint topColor = ImGui.ColorConvertFloat4ToU32(colorStart);
            uint bottomColor = ImGui.ColorConvertFloat4ToU32(colorEnd);
            drawList.AddRectFilledMultiColor(rectTop, rectBottom, topColor, topColor, bottomColor, bottomColor);
        }

        public static void AnimateFloat(ref float value, out float outValue)
        {
            value += 0.1f;
            value = Math.Clamp(value, 0.0f, 1.0f);
            outValue = value;
            return;
        }
        // https://www.unknowncheats.me/forum/counter-strike-2-a/732412-external-hitbox-overlay-aka-chams.html
        public static void DrawCapsule3D(
     Vector3 vMin, Vector3 vMax, float oRadius,
     Quaternion rotation, Vector3 origPos,
     float[] viewMatrix, uint color, int segments = 12, float thickness = 1.0f)
        {
            Vector3 bottom = origPos + Vector3.Transform(vMax, rotation);
            Vector3 top = origPos + Vector3.Transform(vMin, rotation);
            Vector3 point = Extend(top, bottom, Vector3.Distance(top, bottom) * 2);

            float radius = oRadius;
            float radiusHalf = radius * 0.70710678f;

            var topSmallCircle = new List<Vector3>();
            var topCircle = new List<Vector3>();
            var bottomSmallCircle = new List<Vector3>();
            var bottomCircle = new List<Vector3>();

            CreateCircle(Extend(top, bottom, -radiusHalf), point, radiusHalf, topSmallCircle, segments);
            CreateCircle(top, point, radius, topCircle, segments);
            CreateCircle(Extend(bottom, top, -radiusHalf), point, radiusHalf, bottomSmallCircle, segments);
            CreateCircle(bottom, point, radius, bottomCircle, segments);

            var worldPoints = new List<Vector3>(segments * 4 + 2);
            worldPoints.Add(Extend(top, bottom, -radius));
            worldPoints.Add(Extend(bottom, top, -radius));
            worldPoints.AddRange(topSmallCircle);
            worldPoints.AddRange(topCircle);
            worldPoints.AddRange(bottomSmallCircle);
            worldPoints.AddRange(bottomCircle);

            var screenPoints = new List<Vector2>(worldPoints.Count);
            foreach (var wp in worldPoints)
            {
                var sp = Calculate.WorldToScreen(viewMatrix, wp);
                if (sp != new Vector2(-99, -99))
                    screenPoints.Add(sp);
            }

            if (screenPoints.Count < 3) return;

            var hull = ConvexHull(screenPoints);
            if (hull.Count < 3) return;

            uint fillColor = color & 0x00FFFFFF | 0x55000000;
            uint outlineColor = color;

            var drawList = GameState.renderer.drawList;
            var hullArr = hull.ToArray();

            drawList.AddConvexPolyFilled(ref hullArr[0], hull.Count, fillColor);
            drawList.AddPolyline(ref hullArr[0], hull.Count, outlineColor, ImDrawFlags.Closed, thickness);
        }

        private static List<Vector2> ConvexHull(List<Vector2> points)
        {
            int n = points.Count;
            if (n < 3) return points;

            points.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            var hull = new List<Vector2>(n);

            foreach (var p in points)
            {
                while (hull.Count >= 2 && Cross(hull[^2], hull[^1], p) <= 0)
                    hull.RemoveAt(hull.Count - 1);

                hull.Add(p);
            }

            int lowerSize = hull.Count;
            for (int i = n - 2; i >= 0; i--)
            {
                while (hull.Count > lowerSize && Cross(hull[^2], hull[^1], points[i]) <= 0)
                    hull.RemoveAt(hull.Count - 1);

                hull.Add(points[i]);
            }

            hull.RemoveAt(hull.Count - 1);
            return hull;
        }

        private static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        public static void DrawConvexHull(List<Vector2> points, uint fill, uint outline, float thickness = 2f)
        {
            points.Sort((a, b) => a.X != b.X ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            List<Vector2> lower = new();
            List<Vector2> upper = new();

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

            if (lower.Count < 3) return;

            var hullArray = lower.ToArray();
            GameState.renderer.drawList.AddConvexPolyFilled(ref hullArray[0], hullArray.Length, fill);
            GameState.renderer.drawList.AddPolyline(ref hullArray[0], hullArray.Length, outline, ImDrawFlags.Closed, thickness);
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

        private static Vector3 Extend(Vector3 from, Vector3 to, float distance)
        {
            return from + Vector3.Normalize(to - from) * distance;
        }
        public static void DrawGradientText(string text, Vector4 startColor, Vector4 endColor)
        {
            var drawList = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetCursorScreenPos();
            float step = 1f / (text.Length - 1);

            for (int i = 0; i < text.Length; i++)
            {
                float t = i * step;
                Vector4 color = startColor + t * (endColor - startColor);
                drawList.AddText(pos, ImGui.ColorConvertFloat4ToU32(color), text[i].ToString());
                pos.X += ImGui.CalcTextSize(text[i].ToString()).X;
            }

            ImGui.Dummy(new Vector2(ImGui.CalcTextSize(text).X, 0));
        }
        public static void Draw3DCircle(float[] viewMatrix, Vector3 center, float radius, Vector3 normal, uint color, int segments = 32)
        {
            Vector3 up = MathF.Abs(normal.Z) < 0.999f ? Vector3.UnitZ : Vector3.UnitX;
            Vector3 right = Vector3.Normalize(Vector3.Cross(up, normal));
            Vector3 forward = Vector3.Normalize(Vector3.Cross(normal, right));

            List<Vector2> positions2D = new();

            for (int i = 0; i < segments; i++)
            {
                float angle = i * MathF.PI * 2f / segments;
                Vector3 worldPoint = center + right * MathF.Cos(angle) * radius + forward * MathF.Sin(angle) * radius;

                Vector2 screen = Calculate.WorldToScreen(viewMatrix, worldPoint);
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
                GameState.renderer.drawList.AddLine(a, b, color, 2f);
            }
        }
        public static void Draw3DSphere(float[] viewMatrix, Vector3 center, float radius, uint color, int segments = 32)
        {
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitX, color, segments);
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitY, color, segments);
            Draw3DCircle(viewMatrix, center, radius, Vector3.UnitZ, color, segments);
        }
    }
}
