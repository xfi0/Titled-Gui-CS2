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
    internal class JumpHack // TODO make it like work
    {
        public static bool JumpHackEnabled = false;
        public static int JumpHotkey = 0x20;
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public static void JumpShot()
        {
            GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
            GameState.ForceAttack = GameState.client + Offsets.attack;
            if (JumpHackEnabled && GetAsyncKeyState(JumpHotkey) < 0)
            {
                if (GameState.fflag == 65665 || GameState.fflag == 65667)
                {
                    foreach (Entity entity in GameState.Entities)
                    {
                        Console.WriteLine("Velocity:" + entity.Velocity); //oh my gosh he didnt use string interpolation
                    }
                    //GameState.swed.WriteInt(GameState.ForceAttack, 65537);
                    Thread.Sleep(100);
                    //GameState.swed.WriteInt(Offsets.attack, 256);

                }
                else
                {
                    Console.WriteLine(GameState.fflag);
                }
            }
        }
    }
}
