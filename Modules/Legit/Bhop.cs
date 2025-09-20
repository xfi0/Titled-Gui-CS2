using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Swed64;
using static Titled_Gui.Data.Game.GameState;
using System.Runtime.InteropServices;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    public class Bhop : Classes.ThreadService // TODO make it use the jump action, i tried wouldnt work well
    {
        public static bool BhopEnable = false;
        public static float Chance = 100;
        public static int HopKey = 0x20; // space
        private static Random RandomGen = new();
        public static void AutoBhop()
        {
            if (User32.GetAsyncKeyState(HopKey) < 0)
            {
                for (int i = 0; i < 100; i++) // thanks stack overflow i dontg know how to do this outside unity
                {
                    int randomValueBetween0And99 = RandomGen.Next(100);
                    if (randomValueBetween0And99 < Chance)
                    {
                        if (fflag == 65665 || fflag == 65667)
                        {
                            GameState.swed.WriteInt(GameState.ForceJump, 65537); 
                            Thread.Sleep(5);
                        }
                        else
                        {
                            GameState.swed.WriteInt(GameState.ForceJump, 256);
                            Thread.Sleep(5); 
                        }

                    }
                }

            }
        }
        protected override void FrameAction()
        {
            GameState.ForceJump = client + Offsets.jump;
            GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
            if (!BhopEnable) return;
            AutoBhop();
        }
    }
}

