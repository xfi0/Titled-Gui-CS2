using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

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
            if (!Enabled || GameState.LocalPlayer.Health == 0 || (User32.GetAsyncKeyState(0x01) & 0x8000) == 0) return;

            Vector3 PunchAngle = GameState.LocalPlayer.AimPunchAngle * Strength / 100 * 2;

            if (GameState.LocalPlayer.ShotsFired > StartBullet)
            {
                Vector3 NewAngles = GameState.LocalPlayer.ViewAngles + OldPunch - PunchAngle;
                NewAngles.X = Calculate.NormalizeAngle(NewAngles.X);
                NewAngles.Y = Calculate.NormalizeAngle(NewAngles.Y);

                int dx = (int)(NewAngles.X - GameState.LocalPlayer.ViewAngles.X) / (int)Smoothing;
                int dy = (int)(NewAngles.Y - GameState.LocalPlayer.ViewAngles.Y) / (int)Smoothing;

                MoveMouse.MouseMove(dx, dy);
                //GameState.swed.WriteVec(GameState.client, Offsets.dwViewAngles, NewAngles);
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