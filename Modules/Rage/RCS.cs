using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static float Strength = 100f; // in precents so 1 == 100%, 0.5 == 50% etc
        public static Vector3 OldPunch = new(0, 0, 0);
        private static int StartBullet = 1;
        public static void RunRCS()
        {
            if (!Enabled || GameState.LocalPlayer.Health == 0 || (User32.GetAsyncKeyState(0x01) & 0x8000) == 0)
                return;

            if (GameState.LocalPlayer.ShotsFired > StartBullet)
            {
                Vector3 aimPunch = GameState.LocalPlayer.AimPunchAngle * (Strength / 100f);
                Vector3 newAngles;
                aimPunch.X = Calculate.NormalizeAngle(aimPunch.X);
                aimPunch.Y = Calculate.NormalizeAngle(aimPunch.Y);

                newAngles.X = (aimPunch.Y - OldPunch.Y) * 2.0f / (0.022f * GameState.LocalPlayer.Sensitivity) / 1;
                newAngles.Y = -(aimPunch.X - OldPunch.X) * 2.0f / (0.022f * GameState.LocalPlayer.Sensitivity) / 1;
                User32.mouse_event(User32.MOUSEEVENTF_MOVE, (uint)newAngles.X, (uint)newAngles.Y, 0, 0);
                OldPunch = aimPunch;
            }
            else
                OldPunch = new(0, 0, 0);
            
        }


        protected override void FrameAction() => RunRCS();
    }
} 