using ImGuiNET;
using System.Numerics;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class ColorPickers
    {
        public static void ColorEdit(string label, ref Vector4 col, ref bool RGB)
        {
            if (ImGui.BeginPopup(label + "##popup"))
            {
                ImGui.ColorPicker4(label, ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaBar);
                ImGui.Checkbox("RGB##" + label, ref RGB);
                ImGui.EndPopup();
            }
        }

        public static void ColorEdit(string label, ref Vector4 col)
        {
            if (ImGui.BeginPopup(label + "##popup"))
            {
                ImGui.ColorPicker4(label, ref col, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoSidePreview | ImGuiColorEditFlags.NoLabel | ImGuiColorEditFlags.AlphaBar);
                ImGui.EndPopup();
            }
        }

        public static void ColorPickerButton(string label, ref Vector4 col, float size = 25f)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(col.X, col.Y, col.Z, 1));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(col.X, col.Y, col.Z, 1));
            if (ImGui.Button("##btn_" + label, new Vector2(size, size)))
                ImGui.OpenPopup(label + "##popup");

            ImGui.PopStyleColor(2);
        }

        public static void RenderColorPicker(string label, ref Vector4 color, ref bool RGB, Action? onChanged = null, float widgetWidth = 25f)
        {
            Vector4 temp = color;
            bool tempRGB = RGB;

            RenderRowRightAligned(label, () =>
            {
                ColorPickerButton(label, ref temp, widgetWidth);
                ColorEdit(label, ref temp, ref tempRGB);
            }, widgetWidth);

            if (!temp.Equals(color))
            {
                color = temp;
                onChanged?.Invoke();
            }
            if (!tempRGB.Equals(RGB))
            {
                RGB = tempRGB;
                onChanged?.Invoke();
            }
        }

        public static void Render2ColorPickers(string label, ref bool teamRGB, ref bool enemyRGB, ref Vector4 color1, ref Vector4 color2, Action? onChanged = null, float widgetWidth = 50f)
        {
            Vector4 temp1 = color1;
            Vector4 temp2 = color2;
            bool tempTeamRGB = teamRGB;
            bool tempEnemyRGB = enemyRGB;
            float gap = 4f;

            RenderRowRightAligned(label, () =>
            {
                ColorPickerButton("##" + label + "_col1", ref temp1, (widgetWidth - gap) / 2f);
                ColorEdit("##" + label + "_col1", ref temp1, ref tempTeamRGB);

                ImGui.SameLine(0, gap);

                ColorPickerButton("##" + label + "_col2", ref temp2, (widgetWidth - gap) / 2f);
                ColorEdit("##" + label + "_col2", ref temp2, ref tempEnemyRGB);
            }, widgetWidth);

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
            if (!tempTeamRGB.Equals(teamRGB))
            {
                teamRGB = tempTeamRGB;
                onChanged?.Invoke();
            }
            if (!tempEnemyRGB.Equals(enemyRGB))
            {
                enemyRGB = tempEnemyRGB;
                onChanged?.Invoke();
            }
        }
    }
}
