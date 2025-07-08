using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Titled_Gui.Data;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.EntityManager;
using Titled_Gui.ModuleHelpers;
using System.Runtime.InteropServices;

namespace Titled_Gui.Modules.Legit
{
    internal class JumpHack // TODO make it at the peak of the jump
    {
        public static bool JumpHackEnabled = false;
        public static int JumpHotkey = 0x20;
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public static void JumpShot()
        {
            GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, 0x3D4);
            GameState.ForceAttack = GameState.client + Offsets.attack;
            if (JumpHackEnabled && GetAsyncKeyState(JumpHotkey) < 0)
            {
                if (GameState.fflag == 65664)
                {
                    Console.Write("trying to shoot");
                    GameState.swed.WriteInt(GameState.ForceAttack, 65537);
                    Thread.Sleep(100);
                    GameState.swed.WriteInt(Offsets.attack, 256);

                }
                else
                {
                    Console.WriteLine("something with fflag" + GameState.fflag);
                }
            }
        }
    }
}
