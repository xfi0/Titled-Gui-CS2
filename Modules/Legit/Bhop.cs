using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Titled_Gui.Data;
using Swed64;
using static Titled_Gui.Data.GameState;
using System.Runtime.InteropServices;

namespace Titled_Gui.Modules.Legit
{
    public class Bhop
    {
        [DllImport ("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);
        public static bool BhopEnable = false;
        public static int HopKey = 0x20; // Spacebar key
        public static void AutoBhop()
        {
            if ((GetAsyncKeyState(HopKey) & 0x8000) != 0)
            {
                GameState.ForceJump = client + Offsets.jump; 
                uint fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, 0x3D4);
                if (fflag == 265535 || fflag == 65535)
                {
                    GameState.swed.WriteInt(GameState.ForceJump, 65537); // write the value to ForceJump to make the player jump
                    Thread.Sleep(1); // sleep
                    Console.WriteLine($"1 {fflag}");
                }
                else
                {
                    //GameState.swed.WriteInt(GameState.ForceJump, 256); // unfoprce the jump
                    Console.WriteLine($"2 {fflag}");
                    Thread.Sleep(1); //sleep
                }
            }
        }
    }
}

