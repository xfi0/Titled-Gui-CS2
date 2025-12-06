using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool enabled = false;
        public static float strength = 100f; // in precents so 1 == 100%, 0.5 == 50% etc
        public static Vector3 oldPunch = new();
        private static int startBullet = 1;
        public static void RunRCS()
        {
            if (!enabled || GameState.LocalPlayer.Health == 0 || (User32.GetAsyncKeyState(0x01) & 0x8000) == 0)
                return;

            if (GameState.LocalPlayer.ShotsFired > startBullet)
            {
                Vector3 aimPunch = GameState.LocalPlayer.AimPunchAngle * (strength / 100f);
                Vector3 newAngles;
                aimPunch.X = Calculate.NormalizeAngle(aimPunch.X);
                aimPunch.Y = Calculate.NormalizeAngle(aimPunch.Y);

                newAngles.X = (aimPunch.Y - oldPunch.Y) * 2.0f / (0.022f * GameState.LocalPlayer.sensitivity) / 1;
                newAngles.Y = -(aimPunch.X - oldPunch.X) * 2.0f / (0.022f * GameState.LocalPlayer.sensitivity) / 1;
                User32.mouse_event(User32.MOUSEEVENTF_MOVE, (uint)(-newAngles.X * -1), (uint)newAngles.Y, 0, 0);
                oldPunch = aimPunch;
            }
            else
                oldPunch = new(0, 0, 0);
            
        }


        protected override void FrameAction()
        {
            if (!enabled) return;

            RunRCS();
        }
    }
} 