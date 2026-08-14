using System.Globalization;
using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.C4;
using static Titled_Gui.Renderer;

namespace Titled_Gui.Modules.Visual
{
    public class BombTimerOverlay : IModule
    {
        public static bool EnableTimeOverlay = false;

        public static void TimeOverlay() // TODO: diplay more info
        {
            if (!EnableTimeOverlay || GameState.renderer == null)
                return;

            try
            {             
                Vector2 windowSize = new(240f, 100f);
                ImGui.SetNextWindowSize(windowSize,
                    ImGuiCond.Once); // ensure that the size doesn't reset to the default on resize
                ImGui.SetNextWindowPos(new Vector2((GameState.renderer.ScreenSize.X - windowSize.X - 300) / 2, 0), ImGuiCond.FirstUseEver);
                ImGui.Begin("#c4 info",
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoResize);

                C4? c4 = C4Info.C4;
                ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
                if (c4 == null)
                {
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 5), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), "C4 Has Not Been Planted");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 25), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Exploding In: 40");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 45), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Planted At Site: None");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 65), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Being Defused: false");
                }
                else
                {
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 5), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), c4.Planted ? "C4 Has Been Planted" : "C4 Has Not Been Planted");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 25), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Exploding In: {(c4.ExplosionTime > 0 ? MathF.Round(c4.ExplosionTime, 2).ToString() : "40")}");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 45), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Planted At Site: {(c4.Planted ? c4.PlantedSite.ToString() : "None")}");
                    windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 65), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Being Defused: {(c4.BeingDefused ? "True" : "False")}");
                }

                ImGui.End();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TimeOverlay: " + ex);
            }
        }
    }
}
