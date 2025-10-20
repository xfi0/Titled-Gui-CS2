using ImGuiNET;
using System.Numerics;

namespace Titled_Gui.Classes
{
    internal class DrawHelpers
    {
        public static float now = (float)ImGui.GetTime();

        public static void DrawGlowRect(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom, Vector4 color, float rounding, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float Expansion = glowAmount * i;
                float alpha = color.W * MathF.Exp(-i * 0.6f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, alpha);

                var glowTop = new Vector2(rectTop.X - Expansion, rectTop.Y - Expansion);
                var glowBottom = new Vector2(rectBottom.X + Expansion, rectBottom.Y + Expansion);

                drawList.AddRect(glowTop, glowBottom, ImGui.ColorConvertFloat4ToU32(glowColor), rounding);
            }
        }
        public static void DrawGlowRectFilled(ImDrawListPtr drawList, Vector2 rectTop, Vector2 rectBottom, Vector4 color, float rounding, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float Expansion = glowAmount * i;
                float alpha = color.W * MathF.Exp(-i * 0.6f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, alpha);

                var glowTop = new Vector2(rectTop.X - Expansion, rectTop.Y - Expansion);
                var glowBottom = new Vector2(rectBottom.X + Expansion, rectBottom.Y + Expansion);

                drawList.AddRectFilled(glowTop, glowBottom, ImGui.ColorConvertFloat4ToU32(glowColor), rounding);
            }
        }
        public static void DrawGlowBezier(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector4 color, float glowAmount, float thickness, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float expansion = glowAmount * i;
                float alpha = color.W * MathF.Exp(-i * 0.6f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, alpha);

                Vector2 offset = new Vector2(expansion, expansion);
                drawList.AddBezierCubic(p1 - offset, p2 - offset, p3 + offset, p4 + offset, ImGui.ColorConvertFloat4ToU32(glowColor), thickness + i);
            }
        }

        public static void DrawGradientRect(ImDrawListPtr DrawList, Vector2 RectTop, Vector2 RectBottom, Vector4 ColorStart, Vector4 ColorEnd, float Rounding = 0f)
        {
            float rounded = MathF.Min(Rounding, MathF.Abs(RectBottom.X - RectTop.X) * 0.5f);

            uint topColor = ImGui.ColorConvertFloat4ToU32(ColorStart);
            uint bottomColor = ImGui.ColorConvertFloat4ToU32(ColorEnd);

            DrawList.AddRectFilledMultiColor(RectTop, RectBottom, topColor, topColor, bottomColor, bottomColor);
        }
        
        public static void DrawGlowLine(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector4 color, float glowAmount, int layers = 3, float ThickNess = 0)
        {
            Vector2 dir = Vector2.Normalize(p2 - p1);
            Vector2 normal = new(-dir.Y, dir.X);

            for (int i = 1; i <= layers; i++)
            {
                float Offset = glowAmount * i;
                float Alpha = color.W * MathF.Exp(-i * 0.7f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, Alpha);
                var off = normal * Offset;
                if (ThickNess == 0)
                {
                    drawList.AddLine(p1 - off, p2 - off, ImGui.ColorConvertFloat4ToU32(glowColor));
                    drawList.AddLine(p1 + off, p2 + off, ImGui.ColorConvertFloat4ToU32(glowColor));
                }
                else
                {
                    drawList.AddLine(p1 - off, p2 - off, ImGui.ColorConvertFloat4ToU32(glowColor), ThickNess);
                    drawList.AddLine(p1 + off, p2 + off, ImGui.ColorConvertFloat4ToU32(glowColor), ThickNess);
                }
            }
        }

        public static void DrawGlowCircle(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float r = radius + glowAmount * i;
                float Alpha = color.W * MathF.Exp(-i * 0.7f);
                Vector4 GlowColor = new(color.X, color.Y, color.Z, Alpha);

                drawList.AddCircle(center, r, ImGui.ColorConvertFloat4ToU32(GlowColor));
            }
        }
        public static void DrawGlowCircleFilled(ImDrawListPtr drawList, Vector2 center, float radius, Vector4 color, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float r = radius + glowAmount * i;
                float Alpha = color.W * MathF.Exp(-i * 0.7f);
                Vector4 GlowColor = new(color.X, color.Y, color.Z, Alpha);

                drawList.AddCircleFilled(center, r, ImGui.ColorConvertFloat4ToU32(GlowColor));
            }
        }
        public static void DrawGlowText(ImDrawListPtr drawList, Vector2 pos, Vector4 color, string text, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float Offset = glowAmount * i;
                float Alpha = color.W * MathF.Exp(-i * 0.7f);
                Vector4 GlowColor = new(color.X, color.Y, color.Z, Alpha);

                drawList.AddText(pos + new Vector2(-Offset, -Offset), ImGui.ColorConvertFloat4ToU32(GlowColor), text);
                drawList.AddText(pos + new Vector2(Offset, -Offset), ImGui.ColorConvertFloat4ToU32(GlowColor), text);
                drawList.AddText(pos + new Vector2(-Offset, Offset), ImGui.ColorConvertFloat4ToU32(GlowColor), text);
                drawList.AddText(pos + new Vector2(Offset, Offset), ImGui.ColorConvertFloat4ToU32(GlowColor), text);
            }
        }
        public static void DrawGlowQuad(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector4 color, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float expansion = glowAmount * i;
                float alpha = color.W * MathF.Exp(-i * 0.7f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, alpha);

                drawList.AddQuad(new Vector2(p1.X - expansion, p1.Y - expansion), new Vector2(p2.X + expansion, p2.Y - expansion), new Vector2(p3.X + expansion, p3.Y + expansion), new Vector2(p4.X - expansion, p4.Y + expansion), ImGui.ColorConvertFloat4ToU32(glowColor));
            }
        }
        public static void DrawGlowQuadFilled(ImDrawListPtr drawList, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, Vector4 color, float glowAmount, int layers = 4)
        {
            for (int i = 1; i <= layers; i++)
            {
                float expansion = glowAmount * i;
                float alpha = color.W * MathF.Exp(-i * 0.7f);
                var glowColor = new Vector4(color.X, color.Y, color.Z, alpha);

                drawList.AddQuadFilled(new Vector2(p1.X - expansion, p1.Y - expansion), new Vector2(p2.X + expansion, p2.Y - expansion), new Vector2(p3.X + expansion, p3.Y + expansion), new Vector2(p4.X - expansion, p4.Y + expansion), ImGui.ColorConvertFloat4ToU32(glowColor));
            }
        }
        public static void MakeFloatGoWOO(ref float Value, out float OutValue)
        {
            Value += 0.1f;

            if (Value >= 1.0f)
            {
                Value = 1.0f;
                OutValue = Value;
                return;
            }
            else if (Value <= 0.0f)
            {
                Value = 0.0f;
                OutValue = Value;
                return;
            }
            OutValue = Value;
      
        }
    }
}
