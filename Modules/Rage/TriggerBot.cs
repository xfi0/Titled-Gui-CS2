using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;
using Titled_Gui.Data;
using static Titled_Gui.ModuleHelpers.Actions;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot
    {
        public static bool Enabled = false;
        public static int Delay = 10;
        public static bool ShootAtTeam = true;
        public static Keys TriggerKey = Keys.XButton2;
        public static bool RequireKeybind = true; // if enabled keybing is needed

        private static bool OnTarget = false;
        private static Stopwatch reacquireTimer = new Stopwatch();
        private static Stopwatch targetGraceTimer = new Stopwatch();
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(Keys vKey);

        public static void Update()
        {
            if (!RequireKeybind)
            {
                return;
            }
            Enabled = (GetAsyncKeyState(TriggerKey) & 0x8000) != 0;
        }

        public static void Start() // kinda buggy with like the delay and stuff TODO: fires on teamates still
        {
            if (!Enabled || (RequireKeybind && (GetAsyncKeyState(TriggerKey) & 0x8000) == 0))
                return;
            var localPlayer = GameState.swed.ReadPointer(GameState.client, Offsets.dwLocalPlayerPawn);
            if (localPlayer == 0) return; // if dead return

            int crosshairEnt = GameState.swed.ReadInt(localPlayer + Offsets.m_iIDEntIndex);
            var entity = GameState.swed.ReadPointer(GameState.client, Offsets.dwEntityList + (crosshairEnt - 1) * 0x10);

            int targetTeam = GameState.swed.ReadInt(entity + Offsets.m_iTeamNum);
            int localTeam = GameState.swed.ReadInt(localPlayer + Offsets.m_iTeamNum);

            bool isValidTarget = ShootAtTeam || targetTeam != localTeam;

            if (crosshairEnt == -1 || entity == 0) return;
            Console.WriteLine(crosshairEnt);
            if (crosshairEnt != -1) // if its a entity TODO: Make it not shoot at chickens
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
    }
}
