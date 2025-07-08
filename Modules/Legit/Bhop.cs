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
        public static int HopKey = 0x20; // space

        public static void AutoBhop()
        {
            if ((GetAsyncKeyState(HopKey) < 0))
            {
                GameState.ForceJump = client + Offsets.jump;
                GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, 0x3EC);
                if (fflag == 65665 || fflag == 65667)
                {
                    Thread.Sleep(1); // sleep
                    GameState.swed.WriteInt(GameState.ForceJump, 65537); // write the value to ForceJump to make the player jump
                    Console.WriteLine($"1 {fflag}");
                }
                else
                {
                    GameState.swed.WriteInt(GameState.ForceJump, 256); // undo jump 
                    Console.WriteLine($"2 {fflag}");
                    Thread.Sleep(1); //sleep
                }
            }
        }
    }
}

