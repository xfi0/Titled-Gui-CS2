using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.ImGUI.Widgets
{
    internal class Preview
    {
        private static Vector2 _windowSize = new(250, Renderer.MainWindowSize.Y);
        private static float _paddingLeft = 75f;
        private static Vector2 _windowPos = new((GameState.renderer.ScreenSize.X - 800) / 2f + +800 + _paddingLeft, (GameState.renderer.ScreenSize.Y - 600) / 2f);
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
