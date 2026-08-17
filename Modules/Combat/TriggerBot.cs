using System.Diagnostics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Classes.User32;

namespace Titled_Gui.Modules.Combat
{
    public class TriggerBot : Classes.ThreadService, IModule
    {
        public static bool Enabled = false;
        public static int MinDelay = 0;
        public static int MaxDelay = 10;
        public static bool TeamCheck = true;
        public static int TriggerKey = (int)Keys.MButton;
        public static bool OnTarget = false;
        public static int CurrentDelay = 0;
        private static readonly Random _random = new();
        private const int _entityListMultiplier = 0x8;
        private const int _entityEntryOffset = 0x10;
        private const int _entityStride = 120;
        private const int _entityIndexMask = 0x1FF;
        private const int _entityIndexShift = 9;
        private static Stopwatch _reacquireTimer = new();
        private static Stopwatch _targetGraceTimer = new();
        protected override void FrameAction()
        {
            RunTriggerBot();
        }

        public static void RunTriggerBot()
        {
            try
            {
                if (!Enabled || GameState.LocalPlayer == null || GameState.memory == null || (TriggerKey != 0 && (GetAsyncKeyState(TriggerKey) & 0x8000) == 0) || GameState.LocalPlayer.Health <= 0)
                    return;

                int crosshairEnt = GameState.memory.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);
                if (crosshairEnt == -1 || crosshairEnt == 0)
                {
                    ClearTargetState();
                    return;
                }
                int indexHigh = (crosshairEnt & 0x7FFF) >> 9;
                int indexLow = (crosshairEnt & _entityIndexMask);

                IntPtr entityEntry = GameState.memory.ReadPointer(GameState.EntityList, _entityListMultiplier * indexHigh + _entityEntryOffset);
                if (entityEntry == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }

                IntPtr pawnAddress = GameState.memory.ReadPointer(entityEntry, 0x70 * indexLow);
                if (pawnAddress == IntPtr.Zero)
                {
                    ClearTargetState();
                    return;
                }

                int entityTeam = GameState.memory.ReadInt(pawnAddress + Offsets.m_iTeamNum);
                int health = GameState.memory.ReadInt(pawnAddress + Offsets.m_iHealth);
                int lifeState = GameState.memory.ReadInt(pawnAddress + Offsets.m_lifeState);

                if ((TeamCheck && GameState.LocalPlayer.Team == entityTeam) || health == 0 || GameState.LocalPlayer.Health == 0 || lifeState != 256)
                {
                    ClearTargetState();
                    return;
                }

                if (!OnTarget)
                {
                    if (!_reacquireTimer.IsRunning)
                    {
                        _reacquireTimer.Start();
                        CurrentDelay = _random.Next(MinDelay, MaxDelay + 1);
                    }

                    if (_reacquireTimer.ElapsedMilliseconds >= CurrentDelay)
                    {
                        Shoot();
                        OnTarget = true;
                        _reacquireTimer.Reset();
                        _targetGraceTimer.Restart();
                    }
                }
                else
                {
                    Shoot();
                    _targetGraceTimer.Restart();
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
            if (OnTarget && _targetGraceTimer.ElapsedMilliseconds < 100)
                return;


            OnTarget = false;
            _reacquireTimer.Reset();
            _targetGraceTimer.Reset();
            CurrentDelay = 0;
        }
    }
}
