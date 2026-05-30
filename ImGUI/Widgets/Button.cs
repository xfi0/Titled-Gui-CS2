using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Button
    {
        public static void RenderButton(string label, Action value)
        {
            if (ImGui.Button(label)) {
                value.Invoke();
            }
        }
    }
}
