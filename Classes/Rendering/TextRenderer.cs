using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Classes.Rendering
{
    internal class TextRenderer
    {
        public static float Now = (float)ImGui.GetTime();
        public static List<float> animatedFloats = new List<float>();
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
        public static void AnimateFloat(ref float value, out float outValue)
        {
            value += 0.1f;
            value = global::System.Math.Clamp(value, 0.0f, 1.0f);
            outValue = value;
            return;
        }
    }
}
