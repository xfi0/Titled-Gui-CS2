using System.Globalization;
using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.C4;
using static Titled_Gui.Renderer;

namespace Titled_Gui.Modules.Visual
{
    public class BombTimerOverlay
    {
        public static bool EnableTimeOverlay = false;

        public static void TimeOverlay() // TODO diplay more info
        {
            if (!EnableTimeOverlay) return; // if false dont draw

            try
            {
                Types.C4? c4 = C4Info.C4;
                if (c4 == null)
                    return;
                // overlay
                var style = ImGui.GetStyle();
                style.WindowRounding = 5f;
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.10f, 1);
                style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.11f, 0.11f, 0.12f, 1);
                style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.09f, 1);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.11f, 0.11f, 0.12f, 1);
                style.Colors[(int)ImGuiCol.Border] = new Vector4(0.15f, 0.15f, 0.16f, 1);
                style.Colors[(int)ImGuiCol.Button] = new Vector4(0.18f, 0.18f, 0.19f, 1);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.22f, 0.22f, 0.23f, 1);
                style.Colors[(int)ImGuiCol.ButtonActive] = accentColor;
                style.Colors[(int)ImGuiCol.Header] = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.4f);
                style.Colors[(int)ImGuiCol.HeaderHovered] =
                    new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.6f);
                style.Colors[(int)ImGuiCol.HeaderActive] = accentColor;
                Vector2 windowSize = new(240f, 100f);
                ImGui.SetNextWindowSize(windowSize,
                    ImGuiCond.Once); // ensure that the like size doesnt reset to the defualt on resize
                ImGui.SetNextWindowPos(new Vector2((GameState.renderer.ScreenSize.X - windowSize.X - 300) / 2, 0));
                ImGui.Begin("#c4 info",
                    ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoResize);

                ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
                windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 5), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), c4.Planted ? "C4 Has Been Planted" : "C4 Has Not Been Planted");
                windowDrawList.AddText(Renderer.TextFontNormal, 18f,ImGui.GetWindowPos() + new Vector2(20, 25), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Exploding In: {(c4.ExplosionTime > 0 ? MathF.Round(c4.ExplosionTime, 2).ToString() : "40")}");
                windowDrawList.AddText(Renderer.TextFontNormal, 18f,ImGui.GetWindowPos() + new Vector2(20, 45), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Planted At Site: {(c4.Planted ? c4.PlantedSite.ToString() : "None")}");
                windowDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(20, 65), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), $"Being Defused: {(c4.BeingDefused ? "True" : "False")}");
                ImGui.End();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in TimeOverlay: " + ex);
            }
        }
    }
}
