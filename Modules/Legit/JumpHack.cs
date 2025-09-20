using Titled_Gui.Data.Game;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Modules.Legit
{
    internal class JumpHack : Classes.ThreadService // TODO make it like work
    {
        public static bool JumpHackEnabled = false;
        public static int JumpHotkey = 0x20;
        public static void JumpShot()
        {
            if (!JumpHackEnabled) return; //i gotta get better at returning before anything else

            GameState.fflag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
            GameState.ForceAttack = GameState.client + Offsets.attack;
            if (User32.GetAsyncKeyState(JumpHotkey) < 0)
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
        protected override void FrameAction()
        {
            JumpShot();
        }
    }
}
