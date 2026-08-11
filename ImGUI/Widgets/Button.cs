using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Button
    {
        public static void RenderButton(string label, string? id = null, Action? value = null)
        {
            ImGui.PushID(id ?? label);
            if (ImGui.Button(label))
                value?.Invoke();

            ImGui.PopID();
        }
    }
}