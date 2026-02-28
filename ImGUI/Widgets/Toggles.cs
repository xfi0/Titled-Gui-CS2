using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static Titled_Gui.ImGUI.Widgets.Misc;
using static Titled_Gui.ImGUI.Widgets.ColorPickers;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Toggles
    {
        public static Vector4 trackCol = new(0.18f, 0.18f, 0.20f, 1f);
        public static Vector4 knobOff = new(0.15f, 0.15f, 0.15f, 1f);
        public static Vector4 knobOn = new(0.2745f, 0.3176f, 0.4510f, 1.0f);

        private static Dictionary<string, bool> OpenPopups = [];
        private static Dictionary<string, bool> PreviousValues = [];
        public static void RenderBoolSettingWith1ColorPicker(string label, ref bool value, ref Vector4 color1)
        {
            ImGui.PushID(label);

            bool tmpVal = value;
            Vector4 tmpColor = color1;

            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                float rowWidth = ImGui.GetColumnWidth();
                float paddingRight = 7f;

                ImGui.SetCursorScreenPos(rowStart + new Vector2(0, 0));
                ColorEdit("##" + label + "_col1", ref tmpColor,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);

                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;
                Vector2 knobPos = new(rowStart.X + rowWidth - width - paddingRight, rowStart.Y);

                var drawList = ImGui.GetWindowDrawList();
                ImGui.SetCursorScreenPos(knobPos);

                ImGui.InvisibleButton("##" + label + "_toggle", new Vector2(width, height));
                if (ImGui.IsItemClicked()) tmpVal = !tmpVal;

                float t = tmpVal ? 1f : 0f;
                drawList.AddRectFilled(knobPos, new Vector2(knobPos.X + width, knobPos.Y + height),
                    ImGui.ColorConvertFloat4ToU32(trackCol), height);
                float knobX = knobPos.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = knobPos.Y + radius + 2f;
                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(tmpVal ? knobOn : knobOff), 36);
                drawList.AddCircle(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);
            });

            if (!tmpColor.Equals(color1)) color1 = tmpColor;
            value = tmpVal;

            ImGui.PopID();
        }

        public static void RenderBoolSettingWith2ColorPickers(string label, ref bool value, ref Vector4 color1,
            ref Vector4 color2)
        {
            ImGui.PushID(label);

            var tmp1 = color1;
            var tmp2 = color2;
            var tmpVal = value;

            RenderRowRightAligned(label, () =>
            {
                ImGui.ColorEdit4("##" + label + "col1", ref tmp1,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);
                ImGui.SameLine();

                ImGui.ColorEdit4("##" + label + "col2", ref tmp2,
                    ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);
                ImGui.SameLine();

                ImGui.Checkbox("##" + label + "checkmark", ref tmpVal);
            }, widgetWidth: 73f);

            if (!tmp1.Equals(color1))
            {
                color1 = tmp1;
            }

            if (!tmp2.Equals(color2))
            {
                color2 = tmp2;
            }

            if (tmpVal != value)
            {
                value = tmpVal;
            }

            ImGui.PopID();
        }
        public static void RenderBoolSetting(string label, ref bool value, Action? onChanged = null,
    float widgetWidth = 0f)
        {
            bool temp = value;
            RenderRowRightAligned(label, () =>
            {
                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;

                float colWidth = ImGui.GetColumnWidth();
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float posX = ImGui.GetCursorPosX() + colWidth - width - spacing;
                ImGui.SetCursorPosX(posX);

                Vector2 p = ImGui.GetCursorScreenPos();
                var drawList = ImGui.GetWindowDrawList();
                string strId = "##" + label;

                ImGui.InvisibleButton(strId, new Vector2(width, height));
                if (ImGui.IsItemClicked()) temp = !temp;

                float t = temp ? 1f : 0f;

                drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height),
                    ImGui.ColorConvertFloat4ToU32(trackCol), height); // track

                float knobX = p.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = p.Y + radius + 2f;
                Vector4 knobColor = temp ? knobOn : knobOff;
                // knob
                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius, ImGui.ColorConvertFloat4ToU32(knobColor),
                    36);
                drawList.AddCircle(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);
            }, widgetWidth);

            if (temp != value)
            {
                value = temp;
                onChanged?.Invoke();
            }
        }

        public static void RenderBoolSettingWithWarning(string label, ref bool value, Action? onChanged = null,
            float widgetWidth = 0f)
        {
            if (!OpenPopups.ContainsKey(label))
                OpenPopups[label] = false;

            if (!PreviousValues.ContainsKey(label))
                PreviousValues[label] = value;

            bool temp = value;

            RenderRowRightAligned(label, () =>
            {
                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;
                float colWidth = ImGui.GetColumnWidth();
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float posX = ImGui.GetCursorPosX() + colWidth - width - spacing;

                ImGui.SetCursorPosX(posX);

                Vector2 p = ImGui.GetCursorScreenPos();
                var drawList = ImGui.GetWindowDrawList();
                string strId = "##" + label;
                ImGui.InvisibleButton(strId, new Vector2(width, height));

                if (ImGui.IsItemClicked())
                {
                    temp = !temp;
                    if (!PreviousValues[label] && temp)
                        OpenPopups[label] = true;
                }

                float t = temp ? 1f : 0f;

                drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height),
                    ImGui.ColorConvertFloat4ToU32(trackCol), height);

                float knobX = p.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = p.Y + radius + 2f;

                Vector4 knobColor = temp ? knobOn : knobOff;

                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(knobColor), 36);
                drawList.AddCircle(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);

            }, widgetWidth);

            string popupId = "warning##" + label;
            if (OpenPopups[label])
                ImGui.OpenPopup(popupId);

            bool tempref = OpenPopups[label];
            //ImGui.SetNextWindowSize(new Vector2(200, 200));

            if (ImGui.BeginPopupModal(popupId, ref tempref, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("WARNING\nThis feature uses WPM and or may be detected.\n Use at your own risk.");
                ImGui.Separator();

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    OpenPopups[label] = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            PreviousValues[label] = temp;

            if (temp != value)
            {
                value = temp;
                onChanged?.Invoke();
            }
        }
    }
}
