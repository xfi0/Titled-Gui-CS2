using System.Diagnostics;
using System.Threading;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static int MinDelay = 0;
        public static int MaxDelay = 10;  
        public static bool TeamCheck = false;
        public static int TriggerKey = (int)Keys.MButton;
        public static bool RequireKeybind = true; // if enabled keybind is needed
        public static bool OnTarget = false;
        public static Stopwatch ReacquireTimer = new();
        public static Stopwatch TargetGraceTimer = new();
        public static int CurrentDelay = 0;
        private static readonly Random random = new(); 
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
                if (!Enabled || (RequireKeybind && (GetAsyncKeyState(TriggerKey) & 0x8000) == 0) || GameState.LocalPlayer.Health == 0) return;

                int crosshairEnt = GameState.swed.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);
                if (crosshairEnt == -1 || crosshairEnt == 0)
                {
                    ClearTargetState();
                    return;
                }
                int IndexHigh = (crosshairEnt & 0x7FFF) >> 9;
                int IndexLow = (crosshairEnt & EntityIndexMask);

                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                {
                    ClearTargetState();
                    entityList = GameState.EntityList; // fallback, may not work
                    return;
                }
                IntPtr entityEntry = GameState.swed.ReadPointer(entityList, EntityListMultiplier * IndexHigh + EntityEntryOffset);
                if (entityEntry == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }
                IntPtr entityPtr = GameState.swed.ReadPointer(entityEntry, 0x70 * IndexLow);
                if (entityPtr == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }

                int EntityTeam = GameState.swed.ReadInt(entityPtr + Offsets.m_iTeamNum);
                int Health = GameState.swed.ReadInt(entityPtr + Offsets.m_iHealth);
                int LifeState = GameState.swed.ReadInt(entityPtr + Offsets.m_lifeState);
                
                if ((TeamCheck && GameState.LocalPlayer.Team == EntityTeam) || entityPtr == IntPtr.Zero || entityEntry == IntPtr.Zero || entityList == IntPtr.Zero || Health == 0 || GameState.LocalPlayer.Health == 0 || LifeState != 256)
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
                        Shoot();
                        OnTarget = true;
                        ReacquireTimer.Reset();
                        TargetGraceTimer.Restart();
                    }
                }
                else
                {
                    Shoot();
                    TargetGraceTimer.Restart();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static void Shoot()
        {
            User32.Click();
        }

        private static void ClearTargetState()
        {
            if (OnTarget && TargetGraceTimer.ElapsedMilliseconds < 100)
                return;
            

            OnTarget = false;
            ReacquireTimer.Reset();
            TargetGraceTimer.Reset();
            CurrentDelay = 0;
        }
    }
}
