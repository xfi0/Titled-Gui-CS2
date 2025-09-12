using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Titled_Gui.Data;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static int Delay = 10;
        public static bool ShootAtTeam = true;
        public static Keys TriggerKey = Keys.XButton2;
        public static bool RequireKeybind = true; // if enabled keybing is needed

        private static bool OnTarget = false;
        private static Stopwatch reacquireTimer = new();
        private static Stopwatch targetGraceTimer = new();

        public static void Start() // kinda buggy with like the delay and stuff TODO: fires on Teamates still
        {
            if (!Enabled || (RequireKeybind && (GetAsyncKeyState((int)TriggerKey) & 0x8000) == 0) || GameState.localPlayer.Health == 0)
                return;

            GameState.crosshairEnt = GameState.swed.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);
            var entity = GameState.swed.ReadPointer(GameState.client, Offsets.dwEntityList + (GameState.crosshairEnt - 1) * 0x10);

            int targetTeam = GameState.swed.ReadInt(entity + Offsets.m_iTeamNum);

            bool isValidTarget = ShootAtTeam || targetTeam != GameState.localPlayer.Team;

            if (GameState.crosshairEnt != -1 && isValidTarget && entity != IntPtr.Zero) // check that the crosshair ent is not empty (-1) TODO: Make it not shoot at chickens
            {
                if (!OnTarget)
                {
                    if (!reacquireTimer.IsRunning)
                    {
                        reacquireTimer.Start();
                    }

                    if (reacquireTimer.ElapsedMilliseconds >= Delay)
                    {
                        Click();
                        OnTarget = true;
                        reacquireTimer.Reset();
                        targetGraceTimer.Restart();
                    }
                }
                else
                {
                    Click();
                    targetGraceTimer.Restart();
                }
            }
            else
            {
                if (OnTarget && targetGraceTimer.ElapsedMilliseconds < 100)
                {
                    return;
                }

                OnTarget = false;
                reacquireTimer.Reset();
                targetGraceTimer.Reset();
            }
        }

        protected override void FrameAction()
        {
            Start();
        }
    }
}
