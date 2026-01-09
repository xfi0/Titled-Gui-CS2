using System.Diagnostics;
using System.Threading;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Rage
{
    public class TriggerBot : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static int MinDelay = 0;
        public static int MaxDelay = 10;
        public static bool TeamCheck = true;
        public static int TriggerKey = (int)Keys.MButton;
        public static bool RequireKeybind = true;
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

        // Śledzenie strzałów dla Bullet Trail
        private static int lastTriggerShotsFired = 0;
        private static float lastTriggerShotTime = 0f;
        private const float TRIGGER_SHOT_COOLDOWN = 0.15f;

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
                int indexHigh = (crosshairEnt & 0x7FFF) >> 9;
                int indexLow = (crosshairEnt & EntityIndexMask);

                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                {
                    ClearTargetState();
                    entityList = GameState.EntityList;
                    return;
                }
                IntPtr entityEntry = GameState.swed.ReadPointer(entityList, EntityListMultiplier * indexHigh + EntityEntryOffset);
                if (entityEntry == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }
                IntPtr entityPtr = GameState.swed.ReadPointer(entityEntry, 0x70 * indexLow);
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
            // Wykonaj kliknięcie
            User32.Click();

            // Powiadom BulletTrailManager o strzale
            NotifyBulletTrailAboutShot();
        }

        private static void NotifyBulletTrailAboutShot()
        {
            try
            {
                if (GameState.LocalPlayer == null || GameState.LocalPlayer.PawnAddress == IntPtr.Zero)
                    return;

                // Pobierz aktualną liczbę strzałów
                int currentShotsFired = GameState.swed.ReadInt(GameState.LocalPlayer.PawnAddress, Offsets.m_iShotsFired);

                float currentTime = BulletTrailManager.GetCurrentTime();

                // Sprawdź czy to nowy strzał
                if (currentShotsFired > lastTriggerShotsFired &&
                    (currentTime - lastTriggerShotTime) > TRIGGER_SHOT_COOLDOWN)
                {
                    // Powiadom BulletTrailManager o strzale
                    BulletTrailManager.OnShootDetected();
                    lastTriggerShotsFired = currentShotsFired;
                    lastTriggerShotTime = currentTime;
                }
            }
            catch { }
        }

        private static void ClearTargetState()
        {
            if (OnTarget && TargetGraceTimer.ElapsedMilliseconds < 100)
                return;

            OnTarget = false;
            ReacquireTimer.Reset();
            TargetGraceTimer.Reset();
            CurrentDelay = 0;

            // Resetuj śledzenie strzałów
            try
            {
                if (GameState.LocalPlayer != null && GameState.LocalPlayer.PawnAddress != IntPtr.Zero)
                {
                    int currentShotsFired = GameState.swed.ReadInt(GameState.LocalPlayer.PawnAddress, Offsets.m_iShotsFired);
                    if (currentShotsFired == 0)
                    {
                        lastTriggerShotsFired = 0;
                    }
                }
            }
            catch { }
        }
    }
}