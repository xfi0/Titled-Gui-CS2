using SharpGen.Runtime.Win32;
using System.Runtime.InteropServices;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Legit
{
    public class Bhop : Classes.ThreadService // TODO make it use the jump action, i tried wouldnt work well
    {
        public static bool BhopEnable = false;
        public static float Chance = 100;
        public static int minDelay = 25;
        public static int maxDelay = 35;
        public static int HopKey = 0x20; // space
        private static Random random = new();
        private static int lastJumped = GlobalVar.GetTickCount();

        public static void AutoBhop()
        {
            //int now = GlobalVar.GetTickCount();

            if ((User32.GetAsyncKeyState(HopKey) & 0x8000) == 0)
                return;

            //if (now - lastJumped < random.Next(minDelay, maxDelay))
            //    return;

            //if (((int)Fflag & (int)FFlag.FFlagStates.OnGround) == 0)
            //{
            //    return;
            //}

            if (Fflag == 65665 || Fflag == 65667)
            {
                Console.WriteLine(Fflag);
                GameState.swed.WriteInt(GameState.client + Offsets.jump, 65537); // write the value to ForceJump to make the player jump
                Thread.Sleep(15); // sleep 
            }
            else
            { 
                GameState.swed.WriteInt(GameState.client+Offsets.jump, 256); // unfoprce the jump
                Thread.Sleep(15); //sleep
            }
            //lastJumped = now;

            //var inputs = new User32.INPUT[2];
            //inputs[0].type = User32.INPUT_KEYBOARD;
            //inputs[0].U.ki.wVk = User32.VK_SPACE;
            //inputs[0].U.ki.wScan = 0x39;
            //inputs[1].U.ki.dwFlags = User32.KEYEVENTF_KEYDOWN;
            //inputs[0].U.ki.time = 0;
            //inputs[0].U.ki.dwExtraInfo = 0;
            //inputs[1] = inputs[0];
            //inputs[1].U.ki.dwFlags =User32.KEYEVENTF_KEYUP;
            ////Console.WriteLine(now);
            //User32.SendInput(2, inputs, Marshal.SizeOf<User32.INPUT>());
        }
        protected override void FrameAction()
        {
            if (!BhopEnable) return;
            GameState.Fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
            AutoBhop();
        }
    }
}

