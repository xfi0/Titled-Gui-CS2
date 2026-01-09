using System;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.HvH
{
    public class SilentAimManager : ThreadService
    {
        // Konfiguracja
        public static bool Enabled = false;
        public static bool TeamCheck = true;
        public static bool VisibilityCheck = true;
        public static float Smoothing = 2.0f;
        public static bool OnlyWhenShooting = true; // Tylko gdy strzelasz
        public static bool AutomaticFire = false; // Automatyczne strzelanie

        public static string[] TargetBones = ["Head", "Neck", "Chest", "Random"];
        public static int CurrentBone = 0;

        // Status
        public static bool IsActive = false;
        public static Entity CurrentTarget = null;

        private static Random _random = new();
        private static bool _wasShooting = false;

        protected override void FrameAction()
        {
            EnableSilentAim();
        }

        public static void EnableSilentAim()
        {
            try
            {
                // Sprawdzanie podstawowych warunków
                if (!Enabled ||
                    GameState.LocalPlayer?.Health == 0 ||
                    GameState.Entities == null)
                {
                    IsActive = false;
                    CurrentTarget = null;
                    return;
                }

                // Sprawdzanie czy gracz strzela
                bool isShooting = IsPlayerShooting();
                bool shouldActivate = !OnlyWhenShooting || isShooting;

                if (!shouldActivate)
                {
                    IsActive = false;
                    CurrentTarget = null;
                    _wasShooting = false;
                    return;
                }

                // Znajdź cel
                CurrentTarget = FindBestTarget();
                if (CurrentTarget == null)
                {
                    IsActive = false;
                    return;
                }

                // Pobierz pozycję kości
                Vector3 targetPosition = GetTargetBonePosition(CurrentTarget);
                if (targetPosition == Vector3.Zero)
                    return;

                // Oblicz kąty
                Vector3 playerView = LocalPlayer.Origin + LocalPlayer.View;
                Vector2 newAngles = Calculate.CalculateAngles(playerView, targetPosition);

                if (float.IsNaN(newAngles.X) || float.IsNaN(newAngles.Y))
                    return;

                // Aplikuj smoothing
                newAngles = ApplySmoothing(newAngles);

                // Normalizuj kąty
                newAngles.X = Math.Clamp(newAngles.X, -89f, 89f);
                newAngles.Y = Calculate.NormalizeAngle(newAngles.Y);

                // Znajdź i zmodyfikuj CUserCmd (kluczowe dla silent aim)
                ModifyUserCmdAngles(newAngles);

                // Opcjonalnie: automatyczne strzelanie
                if (AutomaticFire && ShouldAutoFire())
                {
                    SimulateFire();
                }

                IsActive = true;
                _wasShooting = isShooting;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SilentAim] Error: {ex.Message}");
            }
        }

        private static Entity FindBestTarget()
        {
            if (Entities == null) return null;

            Entity bestTarget = null;
            float closestDistance = float.MaxValue;

            foreach (var entity in Entities)
            {
                // Podstawowe filtry
                if (entity.Health <= 0 ||
                    entity.Position2D == new Vector2(-99, -99) ||
                    entity == LocalPlayer)
                    continue;

                // Check drużyny
                if (TeamCheck && entity.Team == LocalPlayer.Team)
                    continue;

                // Check widoczności
                if (VisibilityCheck && !entity.Visible)
                    continue;

                // Oblicz dystans
                float distance = Vector3.Distance(LocalPlayer.Origin, entity.Origin);

                // Sprawdź czy jest w polu widzenia (opcjonalnie)
                if (!IsInFieldOfView(entity))
                    continue;

                // Wybierz najbliższy cel
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    bestTarget = entity;
                }
            }

            return bestTarget;
        }

        private static Vector3 GetTargetBonePosition(Entity target)
        {
            if (target.Bones == null || target.Bones.Count == 0)
                return target.Position;

            int boneIndex = 2; // Domyślnie głowa

            switch (CurrentBone)
            {
                case 0: boneIndex = 2; break; // Head
                case 1: boneIndex = 1; break; // Neck
                case 2: boneIndex = 6; break; // Chest
                case 3: // Random bone
                    if (target.Bones.Count > 0)
                        boneIndex = _random.Next(target.Bones.Count);
                    break;
            }

            return target.Bones[boneIndex];
        }

        private static Vector2 ApplySmoothing(Vector2 targetAngles)
        {
            if (Smoothing <= 1.0f)
                return targetAngles;

            // Pobierz aktualne kąty
            Vector3 currentViewAngles = GameState.swed.ReadVec(client, Offsets.dwViewAngles);
            Vector2 currentAngles = new(currentViewAngles.X, currentViewAngles.Y);

            // Oblicz różnicę
            Vector2 delta = targetAngles - currentAngles;

            // Normalizuj różnicę Yaw
            if (Math.Abs(delta.Y) > 180)
            {
                if (delta.Y > 0)
                    delta.Y -= 360;
                else
                    delta.Y += 360;
            }

            // Aplikuj smoothing
            delta /= Smoothing;

            return currentAngles + delta;
        }

        private static void ModifyUserCmdAngles(Vector2 angles)
        {
            try
            {
                // Metoda 1: Przez CSGOInput (najlepsza)
                IntPtr csgoInput = GameState.swed.ReadPointer(GameState.client + Offsets.dwCSGOInput);
                if (csgoInput != IntPtr.Zero)
                {
                    ModifyAnglesViaCSGOInput(csgoInput, angles);
                    return;
                }

                // Metoda 2: Przez globalny bufor komend
                ModifyAnglesViaGlobalBuffer(angles);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SilentAim] ModifyAngles error: {ex.Message}");
            }
        }

        private static void ModifyAnglesViaCSGOInput(IntPtr csgoInput, Vector2 angles)
        {
            // Znajdź bufor komend
            IntPtr commandsPtr = GameState.swed.ReadPointer(csgoInput + 0x50); // m_pCommands
            if (commandsPtr == IntPtr.Zero) return;

            // Znajdź aktualny slot (oparte na tick count)
            int currentSlot = GetCurrentCommandSlot();
            IntPtr currentCmd = GameState.swed.ReadPointer(commandsPtr + (currentSlot * 0x68));

            if (currentCmd != IntPtr.Zero)
            {
                // Zapisz nowe kąty do CUserCmd
                GameState.swed.WriteFloat(currentCmd + 0x18, angles.X); // Pitch
                GameState.swed.WriteFloat(currentCmd + 0x1C, angles.Y); // Yaw
            }
        }

        private static void ModifyAnglesViaGlobalBuffer(Vector2 angles)
        {
            // Ta metoda przeszukuje pamięć w poszukiwaniu CUserCmd
            // Jest mniej dokładna, ale działa jako fallback

            IntPtr inputPtr = GameState.swed.ReadPointer(GameState.client + Offsets.dwInputSystem);
            if (inputPtr == IntPtr.Zero) return;

            // Szukaj prawidłowych struktur CUserCmd
            for (int offset = 0; offset < 0x1000; offset += 0x68)
            {
                IntPtr potentialCmd = inputPtr + offset;

                // Sprawdź czy to może być CUserCmd
                int cmdNumber = GameState.swed.ReadInt(potentialCmd);
                if (cmdNumber > 0 && cmdNumber < 100000)
                {
                    // Zapisz kąty
                    GameState.swed.WriteFloat(potentialCmd + 0x18, angles.X);
                    GameState.swed.WriteFloat(potentialCmd + 0x1C, angles.Y);
                    break;
                }
            }
        }

        private static bool IsPlayerShooting()
        {
            // Sprawdź czy gracz strzela (przycisk ataku)
            return GameState.swed.ReadBool(GameState.client, Offsets.attack);
        }

        private static bool IsInFieldOfView(Entity entity)
        {
            // Sprawdź czy cel jest w polu widzenia
            Vector2 screenPos = entity.Position2D;
            Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);

            float distance = Vector2.Distance(screenPos, screenCenter);
            return distance < 500; // Dostosuj wartość według potrzeb
        }

        private static bool ShouldAutoFire()
        {
            // Logika automatycznego strzelania
            if (CurrentTarget == null) return false;

            // Sprawdź dystans
            float distance = Vector3.Distance(LocalPlayer.Origin, CurrentTarget.Origin);

            // Sprawdź czy cel jest wystarczająco blisko
            return distance < 1000 && CurrentTarget.Visible;
        }

        private static void SimulateFire()
        {
            // Symuluj naciśnięcie przycisku strzału
            GameState.swed.WriteBool(GameState.client, Offsets.attack, true);

            // Po krótkim czasie zwolnij przycisk
            System.Threading.Tasks.Task.Delay(50).ContinueWith(_ =>
            {
                GameState.swed.WriteBool(GameState.client, Offsets.attack, false);
            });
        }

        private static int GetCurrentCommandSlot()
        {
            // Pobierz aktualny tick i oblicz slot w buforze
            IntPtr globalVars = GameState.swed.ReadPointer(GameState.client + Offsets.dwGlobalVars);
            if (globalVars != IntPtr.Zero)
            {
                int tickCount = GameState.swed.ReadInt(globalVars + 0x8);
                return tickCount % 150; // Bufor 150 komend
            }
            return 0;
        }

        // GUI Status
        public static string GetStatusString()
        {
            if (!Enabled) return "Disabled";
            if (!IsActive) return "Waiting for target";

            if (CurrentTarget != null)
                return $"Targeting: {CurrentTarget.Name}";

            return "Active - No target";
        }
    }
}