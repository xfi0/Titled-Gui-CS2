using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool enabled = false;
        public static float strength = 1.0f; // 1.0 = 100% kompensacji

        private static Vector3 lastAimPunch = Vector3.Zero;
        private static bool isActive = false;

        public static void RunPerfectRCS()
        {
            if (!enabled || LocalPlayer == null || LocalPlayer.Health <= 0)
            {
                lastAimPunch = Vector3.Zero;
                isActive = false;
                return;
            }

            // Sprawdzamy czy strzelamy
            bool shootingNow = (User32.GetAsyncKeyState(0x01) & 0x8000) != 0;

            if (!shootingNow)
            {
                lastAimPunch = Vector3.Zero;
                isActive = false;
                return;
            }

            // Pobierz aktualny aimpunch
            Vector3 currentAimPunch = swed.ReadVec(LocalPlayerPawn, Offsets.m_aimPunchAngle);

            // Jeśli strzelamy więcej niż 1 pocisk
            if (LocalPlayer.ShotsFired > 1)
            {
                // Jeśli mamy poprzednią wartość do porównania
                if (lastAimPunch != Vector3.Zero)
                {
                    // Oblicz różnicę w recoilu
                    Vector3 recoilDelta = currentAimPunch - lastAimPunch;

                    // Jeśli jest jakaś różnica (nowy pocisk)
                    if (Math.Abs(recoilDelta.X) > 0.001f || Math.Abs(recoilDelta.Y) > 0.001f)
                    {
                        // Pobierz aktualne viewangles
                        Vector3 viewAngles = swed.ReadVec(client, Offsets.dwViewAngles);

                        // OBLICZ KOMPENSACJĘ:
                        // Odejmij recoil od aktualnych kątów = brak recoilu
                        Vector3 newAngles = new Vector3(
                            viewAngles.X - (recoilDelta.Y * strength),  // Pitch - vertical recoil
                            viewAngles.Y - (recoilDelta.X * strength),  // Yaw - horizontal recoil
                            0
                        );

                        // Normalizuj kąty
                        newAngles.X = Normalize(newAngles.X);
                        newAngles.Y = Normalize(newAngles.Y);

                        // ZAPISZ DO PAMIĘCI - TO USUWA RECOIL
                        swed.WriteVec(client + Offsets.dwViewAngles, newAngles);

                        isActive = true;
                    }
                }
            }

            // Zapisz aktualny aimpunch dla następnej klatki
            lastAimPunch = currentAimPunch;
        }

        private static float Normalize(float angle)
        {
            // Utrzymuj kąt w zakresie -180 do 180
            while (angle > 180.0f) angle -= 360.0f;
            while (angle < -180.0f) angle += 360.0f;
            return angle;
        }

        protected override void FrameAction()
        {
            if (!enabled) return;
            RunPerfectRCS();
        }
    }
}