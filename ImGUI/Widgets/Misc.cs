using ImGuiNET;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Misc
    {
        private const float WidgetColumnWidth = 160f;
        private const float LabelPadding = 4f;
        public static void RenderRow(string label, Action renderWidget, bool rightAlignWidget = false)
        {
            ImGui.Columns(2, null, false);

            ImGui.Indent(LabelPadding);
            ImGui.Text(label);
            ImGui.Unindent(LabelPadding);
            ImGui.NextColumn();

            float colWidth = ImGui.GetColumnWidth();
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float widgetWidth = WidgetColumnWidth;

            if (rightAlignWidget)
            {
                float posX = ImGui.GetCursorPosX() + colWidth - widgetWidth - spacing;
                ImGui.SetCursorPosX(posX);
            }

            ImGui.PushItemWidth(widgetWidth);
            renderWidget();
            ImGui.PopItemWidth();

            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        public static void RenderRowRightAligned(string label, Action renderWidget, float widgetWidth = 0f)
        {
            ImGui.Columns(2, null, false);

            ImGui.Indent(LabelPadding);
            ImGui.Text(label);
            ImGui.Unindent(LabelPadding);
            ImGui.NextColumn();

            float colWidth = ImGui.GetColumnWidth();
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float desired = widgetWidth <= 0f ? 200f : widgetWidth;
            desired = Math.Min(desired, colWidth - spacing);

            float posX = ImGui.GetCursorPosX() + colWidth - desired - spacing;
            ImGui.SetCursorPosX(posX);

            ImGui.PushItemWidth(desired);
            renderWidget();
            ImGui.PopItemWidth();

            ImGui.NextColumn();
            ImGui.Columns(1);
        }
    }
}
