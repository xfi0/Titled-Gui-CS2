using System;
using System.Numerics;
using System.Windows.Forms;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{
    public class BombTimerOverlay
    {
        public static bool EnableTimeOverlay = false;
        private static readonly uint BackgroundColor = 0xAA333333;
        private static readonly uint TextColor = 0xFF0000FF;
        private static readonly float BackgroundPadding = 5f;
        private static int bombCountdown = 40;
        private static bool wasPlanted = false;

        public static void TimeOverlay(Renderer renderer)
        {
            try
            {
                if (GameState.GameRules == IntPtr.Zero) //get game rules if not done already
                {
                    GameState.GameRules = GameState.swed.ReadPointer(GameState.client + Offsets.dwGameRules);
                    if (GameState.GameRules == IntPtr.Zero) return;
                }

                bool planted = GameState.swed.ReadBool(GameState.GameRules + Offsets.m_bBombPlanted);

                if (planted && !wasPlanted)
                {
                    bombCountdown = 40;
                    wasPlanted = true;
                }
                else if (!planted && wasPlanted)
                {
                    wasPlanted = false;
                    bombCountdown = 40;  
                    return;
                }

                // only draw if bomb is planted
                if (planted)
                {
                    string displayText = Math.Max(0, bombCountdown).ToString("0.00") + "s";
                    float screenWidth = Screen.PrimaryScreen.Bounds.Width;
                    Vector2 textPosition = new Vector2(screenWidth / 2 - 100f, 20f);
                    Vector2 backgroundSize = new Vector2(100f, 30f);

                    // draw background and text
                    renderer.drawList.AddRectFilled(
                        textPosition - new Vector2(BackgroundPadding, BackgroundPadding),
                        textPosition + backgroundSize + new Vector2(BackgroundPadding, BackgroundPadding),
                        BackgroundColor);
                    renderer.drawList.AddText(textPosition, TextColor, displayText);

                    if (bombCountdown > 0)
                    {
                        bombCountdown--;
                    }
                }
            }
            catch
            {
                GameState.GameRules = IntPtr.Zero;
                wasPlanted = false;
                bombCountdown = 40;  //if error reset
            }
        }
    }
}