using ImGuiNET;
using System.Numerics;

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
    }
}
