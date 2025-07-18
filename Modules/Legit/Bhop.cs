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
    public class Bhop // TODO make it use the jump action, i tried wouldnt work well
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
                GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
                if (fflag == 65665 || fflag == 65667)
                {
                    GameState.swed.WriteInt(GameState.ForceJump, 65537); // write the value to ForceJump to make the player jump
                    Thread.Sleep(1); // sleep
                }
                else
                {
                    GameState.swed.WriteInt(GameState.ForceJump, 256); // undo jump 
                    Thread.Sleep(1); //sleep
                }
            }
        }
    }
}

