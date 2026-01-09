using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public class HealthBar 
    {
        public static bool EnableHealthBar = false;
        public static bool DrawOnSelf = false;
        public static float HealthBarWidth = 5f;
        public static float Rounding = 2.3f;
        public static Vector4 HealthBarBackGround = new(0.2f, 0.2f, 0.2f, 1f);
        public static Vector4 HealthColor = new(0, 1, 0, 1);

        public static void DrawHealthBar(float Health, float maxHealth, Vector2 topLeft, float height, Entity e)
        {
            if (!EnableHealthBar || (!DrawOnSelf && e.PawnAddress == GameState.LocalPlayer.PawnAddress) || e == null || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) || (BoxESP.TeamCheck && e.Team == GameState.LocalPlayer.Team)) return;

            float HealthPercentage = Math.Clamp(Health / maxHealth, 0f, 1f); // like percentage of box to be filled
            float filledHeight = height * HealthPercentage;

            GameState.renderer.drawList.AddRectFilled(topLeft, topLeft + new Vector2(HealthBarWidth, height), ImGui.ColorConvertFloat4ToU32(HealthBarBackGround), Rounding);

            Vector2 filledTop = topLeft + new Vector2(0, height - filledHeight);

            if (Colors.RGB)
                HealthColor = Colors.Rgb(e.Health);
            
            else
            {
                if (e.Health > 80)
                    HealthColor = new(0f, 1f, 0f, 1f);

                else if (e.Health > 50)
                {
                    float t = (80 - e.Health) / 30; 
                    HealthColor = Vector4.Lerp(new(0f, 1f, 0f, 1f), new(1f, 1f, 0f, 1f), t); 
                }
                else if (e.Health > 20)
                {
                    float t = (50 - e.Health) / 30;
                    HealthColor = Vector4.Lerp(new(1f, 1f, 0f, 1f), new(1f, 0f, 0f, 1f), t);
                }
                else
                    HealthColor = new(1f, 0f, 0f, 1f);   
            }


            GameState.renderer.drawList.AddRectFilled(filledTop, filledTop + new Vector2(HealthBarWidth, filledHeight), ImGui.ColorConvertFloat4ToU32(HealthColor), Rounding);
        }
        public static void DrawHealthBarPreview(Vector2 position)
        {
            float BarHeight = 200f;
            float BarWidth = 6f;
            float HealthPercent = 0.75f;
            DrawHelpers.MakeFloatGoWOO(ref HealthPercent, out float HealthPercent1);

            Vector2 bottom = position + new Vector2(0, BarHeight);

            ImGui.GetWindowDrawList().AddRectFilled(position, bottom + new Vector2(BarWidth, 0), ImGui.ColorConvertFloat4ToU32(HealthBarBackGround), Rounding);
            ImGui.GetWindowDrawList().AddRectFilled(position + new Vector2(0, BarHeight * (1 - HealthPercent1)), position + new Vector2(BarWidth, BarHeight), ImGui.ColorConvertFloat4ToU32(HealthColor), Rounding);
        }

    }
}