using ImGuiNET;
using System.Numerics;
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
        public static void RenderColorSetting(string label, ref Vector4 color, Action? onChanged = null, float widgetWidth = 160f)
        {
            Vector4 temp = color;
            RenderRowRightAligned(label, () =>
            {
                ColorEdit("##" + label, ref temp, ImGuiColorEditFlags.None);
            }, widgetWidth);

            if (!temp.Equals(color))
            {
                color = temp;
                onChanged?.Invoke();
            }
        }

    }
}
