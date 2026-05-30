using ImGuiNET;
using System.Numerics;
using ValveResourceFormat.ResourceTypes;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class ColorPickers
    {
        public static void ColorEdit(string label, ref Vector4 col, ImGuiColorEditFlags flags)
        {
            // TODO: make a custom color picker with options like gradient and stuff
            ImGui.ColorEdit4(label, ref col, flags);
        }
        public static void RenderColorPicker(string label, ref Vector4 color, Action? onChanged = null, float widgetWidth = 25f)
        {
            Vector4 temp = color;
            RenderRowRightAligned(label, () =>
            {
                ColorEdit("##" + label, ref temp, ImGuiColorEditFlags.NoInputs); 
            }, widgetWidth);

            if (!temp.Equals(color))
            {
                color = temp;
                onChanged?.Invoke();
            }
        }
        public static void Render2ColorPickers(string label, ref Vector4 color1, ref Vector4 color2, Action? onChanged = null, float widgetWidth = 50f)
        {
            Vector4 temp1 = color1;
            Vector4 temp2 = color2;
            float gap = 4f;
            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                float height = ImGui.GetFrameHeight();

                ImGui.SetCursorScreenPos(new Vector2(rowStart.X, rowStart.Y));

                ColorEdit("##" + label + "_col1", ref temp1, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.NoInputs);
                ImGui.SameLine(0, gap);
                ColorEdit("##" + label + "_col2", ref temp2, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.NoInputs);
            }, widgetWidth - gap);

            if (!temp1.Equals(color1))
            {
                color1 = temp1;
                onChanged?.Invoke();
            }
            if (!temp2.Equals(color2))
            {
                color2 = temp2;
                onChanged?.Invoke();
            }
        }
    }
}
