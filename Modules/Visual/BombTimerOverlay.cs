using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;
using static Titled_Gui.Renderer;

namespace Titled_Gui.Modules.Visual
{
    public class BombTimerOverlay : Classes.ThreadService
    {
        public static bool EnableTimeOverlay = false;
        public static bool BombPlanted = false;
        private static int? TimePlanted = 0;

        public static void Initialize()
        {
            try
            {
                // counting
                GameState.GameRules = GameState.swed.ReadPointer(GameState.client, Offsets.dwGameRules);

                if (GameState.GameRules == IntPtr.Zero)
                    Thread.Sleep(10);
                

                BombPlanted = GameState.swed.ReadBool(GameState.GameRules, Offsets.m_bBombPlanted);

                if (BombPlanted)
                {
                    for (int i = 0; i < 40; i++) // if bomb gets planted start counting
                    {
                        BombPlanted = GameState.swed.ReadBool(GameState.GameRules, Offsets.m_bBombPlanted);
                        if (!BombPlanted) { TimePlanted = 40; break; } // defusal

                        TimePlanted = 40 - i;
                        Thread.Sleep(1000); // sleep 1 sec on success
                    }
                }
                else
                    Thread.Sleep(100);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Thread.Sleep(1000);
            }
        }
        

        public static void TimeOverlay() // TODO diplay more info
        {
            if (!EnableTimeOverlay) return; // if false dont draw

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
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.6f);
            style.Colors[(int)ImGuiCol.HeaderActive] = accentColor;
            Vector2 windowSize = new(240f, 100f);
            ImGui.SetNextWindowSize(windowSize, ImGuiCond.Once); // ensure that the like size doesnt reset to the defualt on resize
            ImGui.SetNextWindowPos(new Vector2((GameState.renderer.screenSize.X - windowSize.X) / 2, 10));
            ImGui.Begin(BombPlanted ? "C4 Has Been Planted" : "C4 Has Not Been Planted", ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar);

            ImDrawListPtr imDrawList = ImGui.GetWindowDrawList();
            imDrawList.AddText(Renderer.TextFontNormal, 18f, ImGui.GetWindowPos() + new Vector2(10, 0), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), BombPlanted ? "C4 Has Been Planted" : "C4 Has Not Been Planted");
            imDrawList.AddText(ImGui.GetWindowPos() + new Vector2(20, 20), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), TimePlanted.ToString());
            ImGui.End();
        }

        protected override void FrameAction()
        {
            Initialize(); // call everyframe without checking if its enabled to keep timer up to date holy smart
        }
    }
}