using ImGuiNET;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Sliders
    {
        public static void RenderFloatSlider(string label, string? id, ref float value, float min, float max, string format = "%.2f", float widgetWidth = 200f)
        {
            string widgetId = id ?? label;
            float temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.PushID(widgetId);
                ImGui.SliderFloat("##value", ref temp, min, max, format);
                ImGui.PopID();
            }, widgetWidth);

            value = temp;
        }

        public static void RenderIntSlider(string label, string? id, ref int value, int min, int max, string format = "%d", float widgetWidth = 200f)
        {
            string widgetId = id ?? label;
            int temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.PushID(widgetId);
                ImGui.SliderInt("##value", ref temp, min, max, format);
                ImGui.PopID();
            }, widgetWidth);

            value = temp;
        }
    }
}