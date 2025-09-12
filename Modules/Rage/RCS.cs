using ClickableTransparentOverlay;
using System;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static float Strength = 0.5f; // in precents so 1 == 100%, 0.5 == 50% etc
        public static void RunRCS()
        {
            if (!Enabled || GameState.localPlayer.Health == 0 || (User32.GetAsyncKeyState(0x01) & 0x8000) == 0 || !GameState.localPlayer.IsScoped) return;

            Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);

            var RecoilX = GameState.localPlayer.AimPunchAngle.X * 2;
            var RecoilY = GameState.localPlayer.AimPunchAngle.Y * 2;
            var dx = (int)(screenCenter.X - RecoilX);
            var dy = (int)(screenCenter.Y - RecoilY);

            MoveMouse.MouseMove(dx, dy);
            Console.WriteLine(RecoilY + "Y: " + RecoilX);
            Thread.SpinWait(300);
        }
        protected override void FrameAction()
        {
            if (!Enabled) return;
            //RunRCS();
        }
    }
}