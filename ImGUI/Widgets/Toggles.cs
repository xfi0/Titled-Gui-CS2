using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using static Titled_Gui.ImGUI.Widgets.ColorPickers;
using static Titled_Gui.ImGUI.Widgets.Misc;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Toggles
    {
        private static Vector4 trackColor = new(0.18f, 0.18f, 0.20f, 1f);
        private static Vector4 KnobOffColor = new(0.15f, 0.15f, 0.15f, 1f);
        private static Vector4 KnobOnColor = new(0.2745f, 0.3176f, 0.4510f, 1.0f);

        private static Dictionary<string, bool> _openPopups = [];
        private static HashSet<string> _warnedLabels = new();
        private static Dictionary<string, (int, int)> _actions = []; // lable,  (action, keybind)
        private static Dictionary<string, int> _openRightClickOption = new();
        private static Dictionary<string, Func<bool>> _getters = new();
        private static Dictionary<string, Action<bool>> _setters = new();

        private static List<string> _toggleLabels = new() { "Toggle", "Hold" };

        public static void RegisterToggle(string label, Func<bool> getter, Action<bool> setter)
        {
            _getters[label] = () => getter();
            _setters[label] = setter;
        }

        public static void RenderBoolSettingWith1ColorPicker(string label, string? id, Func<bool> getter, Action<bool> setter, ref bool rgb, ref Vector4 color1, Action? onChanged = null)
        {
            string widgetId = id ?? label;
            ImGui.PushID(widgetId);
            Vector4 tmpColor = color1;
            bool tempRGB = rgb;

            bool tmpVal = getter();
            RegisterToggle(widgetId, getter, setter);
            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                var (knobPosition, clicked) = CreateKnob(ref tmpVal);
                RenderRightClickMenu(widgetId);

                float height = ImGui.GetFrameHeight();
                float gap = 6f;

                ImGui.SetCursorScreenPos(new Vector2(knobPosition.X - height - gap, knobPosition.Y));
                ColorEdit("##col1", ref tmpColor, ref tempRGB);

            });

            if (!tmpColor.Equals(color1))
                color1 = tmpColor;

            if (!tempRGB.Equals(rgb))
            {
                rgb = tempRGB;
                onChanged?.Invoke();
            }

            if (tmpVal != getter())
            {
                setter(tmpVal);
                onChanged?.Invoke();
            }

            ImGui.PopID();
        }

        public static void RenderBoolSettingWith1ColorPicker(string label, string? id, Func<bool> getter, Action<bool> setter, ref Vector4 color1, Action? onChanged = null)
        {
            string widgetId = id ?? label;
            ImGui.PushID(widgetId);
            Vector4 tmpColor = color1;

            bool tmpVal = getter();
            RegisterToggle(widgetId, getter, setter);
            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                var (knobPosition, clicked) = CreateKnob(ref tmpVal);
                RenderRightClickMenu(widgetId);

                float height = ImGui.GetFrameHeight();
                float gap = 6f;

                ImGui.SetCursorScreenPos(new Vector2(knobPosition.X - height - gap, knobPosition.Y));
                ColorPickerButton("##col1", ref tmpColor, height);
                ColorEdit("##col1", ref tmpColor);
            });

            if (!tmpColor.Equals(color1))
                color1 = tmpColor;

            if (tmpVal != getter())
            {
                setter(tmpVal);
                onChanged?.Invoke();
            }

            ImGui.PopID();
        }

        public static void RenderRightClickMenu(string widgetId)
        {
            if (!_actions.ContainsKey(widgetId))
                _actions[widgetId] = (0, 0);

            if (!_openRightClickOption.ContainsKey(widgetId))
                _openRightClickOption[widgetId] = 0;

            if (ImGui.BeginPopup("##rightclick"))
            {
                var keybind = _actions[widgetId].Item2;
                _getters.TryGetValue(widgetId, out var moduleState);
                Keybind.RenderKeybindChooser(widgetId + "Keybind", null, ref keybind, moduleState);

                var action = _actions[widgetId].Item1;
                Widgets.Combos.RenderIntCombo("Action", widgetId + "Action", ref action, _toggleLabels, _toggleLabels.Count);
                _actions[widgetId] = (action, keybind);
                ImGui.EndPopup();
            }
        }

        public static void RenderBoolSettingWith2ColorPickers(string label, string? id, Func<bool> getter, Action<bool> setter, ref bool teamRGB, ref bool enemyRGB, ref Vector4 color1,
            ref Vector4 color2)
        {
            string widgetId = id ?? label;
            ImGui.PushID(widgetId);
            var tmpColor1 = color1;
            var tmpColor2 = color2;
            bool tmpVal = getter();
            bool tempTeamRGB = teamRGB;
            bool tempEnemyRGB = enemyRGB;

            RegisterToggle(widgetId, getter, setter);

            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                var (knobPosition, clicked) = CreateKnob(ref tmpVal);
                RenderRightClickMenu(widgetId);

                float height = ImGui.GetFrameHeight();
                float gap = 32f;

                ImGui.SetCursorScreenPos(new Vector2(knobPosition.X - height - gap, knobPosition.Y));

                ColorPickerButton("##col1", ref tmpColor1, height);
                ColorEdit("##col1", ref tmpColor1, ref tempTeamRGB);

                ImGui.SameLine(0, 4f);

                ColorPickerButton("##col2", ref tmpColor2, height);
                ColorEdit("##col2", ref tmpColor2, ref tempEnemyRGB);

            });

            if (!tmpColor1.Equals(color1))
            {
                color1 = tmpColor1;
            }

            if (!tmpColor2.Equals(color2))
            {
                color2 = tmpColor2;
            }

            if (tmpVal != getter())
            {
                setter(tmpVal);
            }
            if (!tempTeamRGB.Equals(teamRGB))
                teamRGB = tempTeamRGB;

            if (!tempEnemyRGB.Equals(enemyRGB))
                enemyRGB = tempEnemyRGB;

            ImGui.PopID();
        }
        public static void RenderBoolSetting(string label, string? id = null, Func<bool>? getter = null, Action<bool>? setter = null, Action? onChanged = null,
    float widgetWidth = 0f)
        {
            if (getter == null || setter == null)
                return;

            string widgetId = id ?? label;
            RegisterToggle(widgetId, getter, setter);

            bool tmpVal = getter();
            ImGui.PushID(widgetId);
            RenderRowRightAligned(label, () =>
            {
                var (knobPosition, clicked) = CreateKnob(ref tmpVal);
                RenderRightClickMenu(widgetId);

            }, widgetWidth);
            ImGui.PopID();

            if (tmpVal != getter())
            {
                setter(tmpVal);
                onChanged?.Invoke();
            }
        }


        public static void RenderBoolSettingWithWarning(string label, string? id = null, Func<bool>? getter = null, Action<bool>? setter = null, Action? onChanged = null,
            float widgetWidth = 0f)
        {
            if (getter == null || setter == null)
                return;

            string widgetId = id ?? label;
            if (!_openPopups.ContainsKey(widgetId))
                _openPopups[widgetId] = false;

            RegisterToggle(widgetId, getter, setter);
            bool tmpVal = getter();
            bool wasEnabled = getter();

            ImGui.PushID(widgetId);
            RenderRowRightAligned(label, () =>
            {
                var (knobPosition, clicked) = CreateKnob(ref tmpVal);
                RenderRightClickMenu(widgetId);

                if (clicked && !wasEnabled && tmpVal && !_warnedLabels.Contains(widgetId))
                {
                    _openPopups[widgetId] = true;
                    _warnedLabels.Add(widgetId);
                }
            }, widgetWidth);

            string popupId = "##warning";
            if (_openPopups[widgetId])
                ImGui.OpenPopup(popupId);


            bool tempref = _openPopups[widgetId];
            //ImGui.SetNextWindowSize(new Vector2(200, 200));

            if (ImGui.BeginPopupModal(popupId, ref tempref, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text("WARNING\nThis feature uses WPM and or may be detected.\n Use at your own risk.");
                ImGui.Separator();

                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    _openPopups[widgetId] = false;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            ImGui.PopID();

            if (tmpVal != getter())
            {
                setter(tmpVal);
                onChanged?.Invoke();
            }
        }

        private static (Vector2 pos, bool clicked) CreateKnob(ref bool value)
        {
            bool clicked = false;
            Vector2 rowStart = ImGui.GetCursorScreenPos();
            float rowWidth = ImGui.GetColumnWidth();
            float paddingRight = 7f;
            float height = ImGui.GetFrameHeight();
            float width = height * 1.7f;
            float radius = height / 2f - 2f;
            Vector2 knobPos = new(rowStart.X + rowWidth - width - paddingRight, rowStart.Y);

            var drawList = ImGui.GetWindowDrawList();
            ImGui.SetCursorScreenPos(knobPos);

            ImGui.InvisibleButton("##toggle", new Vector2(width, height));
            if (ImGui.IsItemClicked(ImGuiMouseButton.Left))
            {
                clicked = true;
                value = !value;
            }
            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                ImGui.OpenPopup("##rightclick");


            float t = value ? 1f : 0f;
            drawList.AddRectFilled(knobPos, new Vector2(knobPos.X + width, knobPos.Y + height),
                ImGui.ColorConvertFloat4ToU32(trackColor), height);
            float knobX = knobPos.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
            float knobY = knobPos.Y + radius + 2f;
            drawList.AddCircleFilled(new Vector2(knobX, knobY), radius,
                ImGui.ColorConvertFloat4ToU32(value ? KnobOnColor : KnobOffColor), 36);
            drawList.AddCircle(new Vector2(knobX, knobY), radius,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);

            return (knobPos, clicked);
        }

        public static void LoopAllActions()
        {
            foreach (var (label, (actionType, keybind)) in _actions)
            {
                if (keybind == 0)
                    continue;

                if (!_getters.ContainsKey(label) || !_setters.ContainsKey(label))
                    continue;

                if (actionType == 0) // toggle
                {
                    if (User32.GetKeyPressed(keybind))
                        _setters[label](!_getters[label]());
                }
                else // hold
                {
                    _setters[label](User32.GetKeyHeld(keybind));
                }
            }
        }
    }
}