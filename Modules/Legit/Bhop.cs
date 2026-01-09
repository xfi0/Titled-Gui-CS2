using System.Runtime.InteropServices;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;
using System.Numerics;

namespace Titled_Gui.Modules.Legit
{
    public class Bhop : Classes.ThreadService
    {
        public static bool BhopEnable = false;
        public static bool AutoStrafeEnable = false;
        public static float AutoStrafeSmoothness =  1.0f; // 0.0 - 1.0
        public static float Chance = 100;
        public static int minDelay = 4;
        public static int maxDelay = 9;
        public static int HopKey = 0x20; // space

        private static Random random = new();
        private static float previousYaw = 1000f; // Inicjalizacja wartością spoza zakresu
        private static Vector3 previousVelocity = Vector3.Zero;

        // Offsety dla autostrafe
        private const int ON_GROUND = 0x1;
        private const uint PLUS_JUMP = 65537;
        private const uint MINUS_JUMP = 256;
        private const uint PLUS_LEFT = 65537;
        private const uint MINUS_LEFT = 256;
        private const uint PLUS_RIGHT = 65537;
        private const uint MINUS_RIGHT = 256;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vKey);

        // Helper function do normalizacji kąta
        private static float NormalizeYaw(float yaw)
        {
            while (yaw > 180) yaw -= 360;
            while (yaw < -180) yaw += 360;
            return yaw;
        }

        // Oblicz kąt prędkości 2D
        private static float GetVelocityAngle(Vector3 velocity)
        {
            if (velocity.X == 0 && velocity.Y == 0)
                return 0f;

            float angle = (float)(Math.Atan2(velocity.Y, velocity.X) * (180 / Math.PI));
            return NormalizeYaw(angle);
        }

        // Oblicz różnicę kątów
        private static float GetAngleDifference(float angle1, float angle2)
        {
            float diff = NormalizeYaw(angle1 - angle2);
            return diff;
        }

        // Główna funkcja AutoStrafe
        public static void AutoStrafe()
        {
            if (!AutoStrafeEnable || GameState.LocalPlayerPawn == IntPtr.Zero)
                return;

            // Sprawdź czy gracz wciska SPACE
            if ((GetAsyncKeyState(HopKey) & 0x8000) == 0)
            {
                // Jeśli nie wciskamy SPACE, wyłącz autostrafe
                ReleaseStrafeKeys();
                previousYaw = 1000f;
                return;
            }

            try
            {
                // Pobierz aktualne dane gracza
                uint fFlag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
                bool isOnGround = (fFlag & ON_GROUND) != 0;

                // Jeśli jesteśmy na ziemi, wyłącz autostrafe
                if (isOnGround)
                {
                    ReleaseStrafeKeys();
                    previousYaw = 1000f;
                    return;
                }

                // Pobierz aktualne kąty widzenia
                Vector3 viewAngles = GameState.swed.ReadVec(GameState.client, Offsets.dwViewAngles);
                float currentYaw = viewAngles.Y;

                // Pobierz aktualną prędkość
                Vector3 velocity = GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vecAbsVelocity);
                float speed2D = (float)Math.Sqrt(velocity.X * velocity.X + velocity.Y * velocity.Y);

                // Jeśli prędkość jest zbyt niska, nie ma potrzeby autostrafe
                if (speed2D < 30)
                {
                    ReleaseStrafeKeys();
                    previousYaw = currentYaw;
                    return;
                }

                // Sprawdź czy gracz używa klawiszy ruchu (WASD)
                bool usingMovementKeys = (GetAsyncKeyState(0x57) & 0x8000) != 0 || // W
                                         (GetAsyncKeyState(0x41) & 0x8000) != 0 || // A
                                         (GetAsyncKeyState(0x53) & 0x8000) != 0 || // S
                                         (GetAsyncKeyState(0x44) & 0x8000) != 0;   // D

                if (usingMovementKeys)
                {
                    ReleaseStrafeKeys();
                    previousYaw = currentYaw;
                    return;
                }

                // Jeśli to pierwsza iteracja (poprzedni yaw = 1000), zapisz i wyjdź
                if (Math.Abs(previousYaw - 1000f) < 0.1f)
                {
                    previousYaw = currentYaw;
                    return;
                }

                // Oblicz różnicę w kątach
                float yawDifference = NormalizeYaw(currentYaw - previousYaw);

                // Określ optymalny kąt skrętu na podstawie prędkości
                float optimalAngle = (float)Math.Clamp(Math.Atan2(15.0, speed2D) * (180 / Math.PI), 0, 45);

                // Zastosuj smoothing
                optimalAngle *= (1.0f - AutoStrafeSmoothness * 0.5f);

                // Sprawdź w którą stronę się obracamy
                bool turningRight = yawDifference < 0;
                bool turningLeft = yawDifference > 0;

                // Określ kąt prędkości
                float velocityAngle = GetVelocityAngle(velocity);
                float angleToVelocity = GetAngleDifference(currentYaw, velocityAngle);

                // Zaawansowana logika autostrafe
                if (Math.Abs(angleToVelocity) > 170 && speed2D > 80)
                {
                    // Jeśli lecimy prawie pod prąd, wykonaj ostry skręt
                    if (angleToVelocity > 0)
                    {
                        StrafeRight();
                    }
                    else
                    {
                        StrafeLeft();
                    }
                }
                else if (angleToVelocity > optimalAngle && speed2D > 80)
                {
                    StrafeRight();
                }
                else if (angleToVelocity < -optimalAngle && speed2D > 80)
                {
                    StrafeLeft();
                }
                else
                {
                    // Normalny autostrafe oparty na obrocie kamery
                    if (turningRight)
                    {
                        StrafeLeft();
                    }
                    else if (turningLeft)
                    {
                        StrafeRight();
                    }
                    else
                    {
                        // Jeśli nie obracamy się, używaj naprzemiennego strafingu
                        bool shouldStrafeLeft = (DateTime.Now.Millisecond % 2) == 0;
                        if (shouldStrafeLeft)
                        {
                            StrafeLeft();
                        }
                        else
                        {
                            StrafeRight();
                        }
                    }
                }

                // Zapisz aktualny yaw dla następnej iteracji
                previousYaw = currentYaw;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"AutoStrafe error: {ex.Message}");
            }
        }

        // Funkcje pomocnicze do kontroli strafingu
        private static void StrafeLeft()
        {
            // Zwolnij prawy strafe jeśli był wciśnięty
            GameState.swed.WriteInt(GameState.client + GetStrafeRightOffset(), (int)MINUS_RIGHT);

            // Wciśnij lewy strafe
           
        }

        private static void StrafeRight()
        {
            // Zwolnij lewy strafe jeśli był wciśnięty
           

            // Wciśnij prawy strafe
            GameState.swed.WriteInt(GameState.client + GetStrafeRightOffset(), (int)PLUS_RIGHT);
        }

        private static void ReleaseStrafeKeys()
        {
           
            GameState.swed.WriteInt(GameState.client + GetStrafeRightOffset(), (int)MINUS_RIGHT);
        }

        // Funkcje pomocnicze do pobierania offsetów (potrzebujesz dodać te offsety do Offsets.cs)
        //private static int GetStrafeLeftOffset()
        //{
            // Jeśli nie masz offsetu dla +left, użyj tymczasowego workaround
            // W idealnym przypadku dodaj te offsety do klasy Offsets
            //return Offsets.attack; // Tymczasowo używamy attack jako placeholder
        //}

        private static int GetStrafeRightOffset()
        {
            // Jeśli nie masz offsetu dla +right, użyj tymczasowego workaround
            return Offsets.jump; // Tymczasowo używamy jump jako placeholder
        }

        // Ulepszona funkcja Bhop z autostrafe
        public static void AutoBhopWithStrafe()
        {
            if (!BhopEnable || GameState.LocalPlayerPawn == IntPtr.Zero)
                return;

            bool spacePressed = (GetAsyncKeyState(HopKey) & 0x8000) != 0;

            if (!spacePressed)
            {
                GameState.swed.WriteInt(GameState.client + Offsets.jump, (int)MINUS_JUMP);
                ReleaseStrafeKeys();
                previousYaw = 1000f;
                return;
            }

            try
            {
                uint fFlag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
                bool onGround = (fFlag & ON_GROUND) != 0;

                if (onGround)
                {
                    // Standardowy bhop na ziemi
                    Thread.Sleep(1);
                    GameState.swed.WriteInt(GameState.client + Offsets.jump, (int)PLUS_JUMP);
                    Thread.Sleep(random.Next(2, 5));

                    // Wyłącz autostrafe gdy jesteśmy na ziemi
                    ReleaseStrafeKeys();
                    previousYaw = 1000f;
                }
                else
                {
                    // Gdy w powietrzu - wyłącz skok i włącz autostrafe
                    GameState.swed.WriteInt(GameState.client + Offsets.jump, (int)MINUS_JUMP);

                    if (AutoStrafeEnable)
                    {
                        AutoStrafe();
                    }
                    else
                    {
                        ReleaseStrafeKeys();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BhopWithStrafe error: {ex.Message}");
            }
        }

        // Uproszczona wersja autostrafe dla łatwiejszego zrozumienia
        public static void SimpleAutoStrafe()
        {
            if (!AutoStrafeEnable || GameState.LocalPlayerPawn == IntPtr.Zero)
                return;

            if ((GetAsyncKeyState(HopKey) & 0x8000) == 0)
            {
                ReleaseStrafeKeys();
                return;
            }

            try
            {
                uint fFlag = GameState.swed.ReadUInt(GameState.LocalPlayerPawn, Offsets.m_fFlags);
                bool isOnGround = (fFlag & ON_GROUND) != 0;

                if (isOnGround)
                {
                    ReleaseStrafeKeys();
                    return;
                }

                // Pobierz aktualny yaw
                Vector3 viewAngles = GameState.swed.ReadVec(GameState.client, Offsets.dwViewAngles);
                float currentYaw = viewAngles.Y;

                // Jeśli to pierwsza iteracja
                if (Math.Abs(previousYaw - 1000f) < 0.1f)
                {
                    previousYaw = currentYaw;
                    return;
                }

                // Oblicz zmianę yaw
                float yawChange = NormalizeYaw(currentYaw - previousYaw);

                // Prosta logika autostrafe
                if (yawChange > 0.1f) // Obracamy się w lewo
                {
                    StrafeRight(); // Strafe w prawo dla utrzymania prędkości
                }
                else if (yawChange < -0.1f) // Obracamy się w prawo
                {
                    StrafeLeft(); // Strafe w lewo dla utrzymania prędkości
                }

                previousYaw = currentYaw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SimpleAutoStrafe error: {ex.Message}");
            }
        }

        protected override void FrameAction()
        {
            if (!BhopEnable || GameState.client == IntPtr.Zero)
                return;

            // Użyj zaawansowanego bhopa z autostrafe
            AutoBhopWithStrafe();

            // Alternatywnie możesz użyć:
            // AutoBhopPerfect(); // Tylko bhop bez autostrafe
            SimpleAutoStrafe(); // Prosty autostrafe
        }
    }
}