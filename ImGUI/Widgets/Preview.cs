using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using ValveResourceFormat.IO;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Preview
    {
        private static Vector2 _windowSize = new(250, Renderer.MainWindowSize.Y);
        private static float paddingLeft = 75f;
        private static Vector2 _windowPos = new((GameState.renderer.ScreenSize.X - 800) / 2f + + 800 + paddingLeft, (GameState.renderer.ScreenSize.Y - 600) / 2f);
        public static void DrawWindow()
        {
            ImGui.SetNextWindowSize(_windowSize);
            ImGui.SetNextWindowPos(_windowPos, ImGuiCond.Always);
            ImGui.Begin("preview", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoResize);
            var windowSize = ImGui.GetWindowSize();
            Vector2 windowCenter = new(windowSize.X / 2, windowSize.Y / 2);
            BoxESP.RenderESPPreview(windowCenter);
            ImGui.End();
        }
    }
}
