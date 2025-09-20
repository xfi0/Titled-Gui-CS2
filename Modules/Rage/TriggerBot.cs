using System.Diagnostics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Vortice.Mathematics;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static int MinDelay = 0;
        public static int MaxDelay = 10;  
        public static bool ShootAtTeam = true;
        public static Keys TriggerKey = Keys.XButton2;
        public static bool RequireKeybind = true; // if enabled keybind is needed
        public static bool OnTarget = false;
        public static Stopwatch ReacquireTimer = new();
        public static Stopwatch TargetGraceTimer = new();
        public static int CurrentDelay = 0;
        private static readonly Random random = new(); 

        public const float MaxVelocityThreshold = 18f;
        public const int EntityListMultiplier = 0x8;
        public const int EntityEntryOffset = 0x10;
        public const int EntityStride = 120;
        public const int EntityIndexMask = 0x1FF;
        public const int EntityIndexShift = 9;

        protected override void FrameAction()
        {
            RunTriggerBot();
        }

        public static void RunTriggerBot()
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

                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
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
                    if (!ReacquireTimer.IsRunning)
                    {
                        ReacquireTimer.Start();
                        CurrentDelay = random.Next(MinDelay, MaxDelay + 1);
                    }

                    if (ReacquireTimer.ElapsedMilliseconds >= CurrentDelay)
                    {
                        ExecuteTriggerAsync();
                        OnTarget = true;
                        ReacquireTimer.Reset();
                        TargetGraceTimer.Restart();
                    }
                }
                else
                {
                    ExecuteTriggerAsync();
                    TargetGraceTimer.Restart();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private static async void ExecuteTriggerAsync()
        {
            await Task.Delay(5);
            User32.Click();
        }

        private static void ClearTargetState()
        {
            if (OnTarget && TargetGraceTimer.ElapsedMilliseconds < 100)
            {
                return;
            }

            OnTarget = false;
            ReacquireTimer.Reset();
            TargetGraceTimer.Reset();
            CurrentDelay = 0;
        }
    }
}
