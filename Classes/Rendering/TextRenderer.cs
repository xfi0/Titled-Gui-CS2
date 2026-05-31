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
        private static Dictionary<string, (float value, float dir)> _animStates = new();
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

        public static float AnimateFloat(string key, float speed = 1f)
        {
            if (!_animStates.ContainsKey(key))
                _animStates[key] = (0f, 1f);

            var (value, dir) = _animStates[key];
            value += dir * speed * ImGui.GetIO().DeltaTime;

            if (value >= 1f) 
            { 
                value = 1f; dir = -1f; 
            }
            else if (value <= 0f) 
            {
                value = 0f; dir = 1f; 
            }

            _animStates[key] = (value, dir);
            return value;
        }
    }
}
