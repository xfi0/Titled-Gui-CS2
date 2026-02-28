using ImGuiNET;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Sliders
    {
        public static void RenderFloatSlider(string label, ref float value, float min, float max, string format = "%.2f", float widgetWidth = 200f)
        {
            float temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.SliderFloat("##" + label, ref temp, min, max, format);
            }, widgetWidth);

            value = temp;
        }

        public static void RenderIntSlider(string label, ref int value, int min, int max, string format = "%d", float widgetWidth = 200f)
        {
            int temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.SliderInt("##" + label, ref temp, min, max, format);
            }, widgetWidth);

            value = temp;
        }
    }
}
