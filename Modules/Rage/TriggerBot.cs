using System.Diagnostics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static int Delay = 10;
        public static bool ShootAtTeam = true;
        public static Keys TriggerKey = Keys.XButton2;
        public static bool RequireKeybind = true; // if enabled keybind is needed
        public static bool OnTarget = false;
        public static Stopwatch reacquireTimer = new();
        public static Stopwatch targetGraceTimer = new();
        public const float MaxVelocityThreshold = 18f;
        public const int TriggerDelayMs = 5;
        public const int EntityListMultiplier = 0x8;
        public const int EntityEntryOffset = 0x10;
        public const int EntityStride = 120;
        public const int EntityIndexMask = 0x1FF;
        public const int EntityIndexShift = 9;

        protected override void FrameAction()
        {
            Start();
        }

        public static void Start()
        {
            try
            {
                if (!Enabled || (RequireKeybind && (GetAsyncKeyState((int)TriggerKey) & 0x8000) == 0) || GameState.localPlayer.Health == 0)
                    return;

                int crosshairEnt = GameState.swed.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);

                if (crosshairEnt < 0)
                {
                    ClearTargetState();
                    return;
                }

                IntPtr entityList = IntPtr.Zero;

                entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);

                if (entityList == IntPtr.Zero)
                {
                    entityList = EntityManager.listEntry;
                }

                IntPtr entityEntry = GameState.swed.ReadPointer(entityList + EntityListMultiplier * (crosshairEnt >> EntityIndexShift) + EntityEntryOffset);
                

                IntPtr entityPtr = GameState.swed.ReadPointer(entityEntry + EntityStride * (crosshairEnt & EntityIndexMask));
               
                int EntityTeam = GameState.swed.ReadInt(entityPtr + Offsets.m_iTeamNum);

                if ((!ShootAtTeam && GameState.localPlayer.Team != EntityTeam) || entityPtr == IntPtr.Zero || entityEntry == IntPtr.Zero || entityList == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }

                if (!OnTarget)
                {
                    if (!reacquireTimer.IsRunning)
                        reacquireTimer.Start();

                    if (reacquireTimer.ElapsedMilliseconds >= Delay)
                    {
                        ExecuteTriggerAsync();
                        OnTarget = true;
                        reacquireTimer.Reset();
                        targetGraceTimer.Restart();
                    }
                }
                else
                {
                    ExecuteTriggerAsync();
                    targetGraceTimer.Restart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private static async void ExecuteTriggerAsync()
        {
            await Task.Delay(TriggerDelayMs);
            User32.Click();
        }

        private static void ClearTargetState()
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
