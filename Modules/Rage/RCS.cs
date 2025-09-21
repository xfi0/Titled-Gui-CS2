using System.ComponentModel.DataAnnotations;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Legit;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static float Strength = 100f; // in precents so 1 == 100%, 0.5 == 50% etc
        public static Vector3 OldPunch = new();
        public static int StartBullet = 1;
        public static float Smoothing = 5f;
        public static void RunRCS()
        {
            if (!Enabled || GameState.localPlayer.Health == 0 || (User32.GetAsyncKeyState(0x01) & 0x8000) == 0) return;
            
            Vector3 PunchAngle = GameState.localPlayer.AimPunchAngle * Strength / 100 * 2;

            Vector2 ScreenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);

            if (GameState.localPlayer.ShotsFired > StartBullet)
            {
                Vector3 NewAngles = GameState.localPlayer.ViewAngles + OldPunch - PunchAngle;
                //int dx = (int)((int)NewAngles.X - ScreenCenter.X);
                //int dY = (int)((int)NewAngles.Y - ScreenCenter.Y);

                //MoveMouse.MouseMove(dx, dY);
                GameState.swed.WriteVec(GameState.client, Offsets.dwViewAngles, NewAngles);
            }
            OldPunch = PunchAngle;
        }

        protected override void FrameAction()
        {
            if (!Enabled) return;

            RunRCS();
        }
    }
} 