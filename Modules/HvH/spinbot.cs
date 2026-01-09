using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Titled_Gui.Modules.HvH
{
    public class SpinBot : Classes.ThreadService
    {
        // ✅ PUBLIC SETTINGS
        public static bool SpinbotEnabled = false;
        public static float SpinbotSpeed = 15f;
        public static string[] SpinbotModes = ["Spin", "Jiggle", "Random", "Custom", "Backwards"];
        public static int currentSpinbotMode = 4; // 4 = Backwards (domyślnie)

        public static float CustomAngleX = 0f;
        public static float CustomAngleY = 0f;
        public static float CustomAngleZ = 0f;

        public static bool EnableX = true;
        public static bool EnableY = true;
        public static bool EnableZ = false;
        public static bool MovementFix = true;

        public static bool AntiAimEnabled = true;
        public static float JiggleIntensity = 30f;
        public static float JiggleFrequency = 0.1f;
        public static float RandomMin = -180f;
        public static float RandomMax = 180f;
        public static float PitchAngle = 89f;
        public static float YawAngle = 180f;
        public static bool InvertYaw = false;
        public static bool JitterYaw = false;
        public static float JitterAmount = 10f;

        // ✅ PRIVATE VARIABLES
        private static float _currentAngleY = 0f;
        private static float _currentAngleX = 0f;
        private static float _currentAngleZ = 0f;
        private static Random _random = new Random();
        private static bool _firstRun = true;
        private static float _jiggleTimer = 0f;
        private static float _jitterTimer = 0f;

        // ✅ WAŻNE: Zapisujemy kąty PRZED modyfikacją
        private static Vector3 _viewAnglesBeforeAA = Vector3.Zero;
        private static bool _isLocalPlayerValid = false;

        // ✅ PROPERTY
        public static string CurrentMode
        {
            get => SpinbotModes[currentSpinbotMode];
            set
            {
                for (int i = 0; i < SpinbotModes.Length; i++)
                {
                    if (SpinbotModes[i] == value)
                    {
                        currentSpinbotMode = i;
                        return;
                    }
                }
                currentSpinbotMode = 0;
            }
        }

        // ✅ GŁÓWNA FUNKCJA - UPROSZCZONA I DZIAŁAJĄCA
        public static void EnableSpinbot()
        {
            try
            {
                // Sprawdzenie czy gracz żyje
                if (!SpinbotEnabled || GameState.LocalPlayer == null || GameState.LocalPlayer.Health <= 0)
                {
                    _firstRun = true;
                    _isLocalPlayerValid = false;
                    return;
                }

                _isLocalPlayerValid = true;

                // 1. ✅ ODCZYTAJ ORYGINALNE KĄTY GRACZA (przed naszą modyfikacją)
                Vector3 originalAngles = GameState.swed.ReadVec(client, Offsets.dwViewAngles);

                // Zapisz kąty przed AA (tylko jeśli to pierwsze uruchomienie w tej klatce)
                _viewAnglesBeforeAA = originalAngles;

                // 2. ✅ OBLICZ NOWE KĄTY (dla backwards mode = +180°)
                Vector3 newAngles = originalAngles;

                if (currentSpinbotMode == 4) // BACKWARDS MODE
                {
                    // Dodaj 180° do YAW (obrót w poziomie)
                    newAngles.Y = NormalizeAngle(originalAngles.Y + 180f);

                    // Ustaw pitch (góra/dół)
                    if (AntiAimEnabled)
                    {
                        newAngles.X = Math.Clamp(PitchAngle, -89f, 89f);
                    }

                    // Roll zawsze 0
                    newAngles.Z = 0f;
                }
                else
                {
                    newAngles = CalculateOtherModes(originalAngles);
                }

                // 3. ✅ ZAPISZ NOWE KĄTY
                GameState.swed.WriteVec(client, Offsets.dwViewAngles, newAngles);

                // 4. ✅✅✅ NIE RUSZAJ MOVEMENT! ŻADNYCH ZAPISÓW DO FORWARDMOVE/SIDEMOVE!
                // Problem: External cheat NIE MOŻE nadpisywać movement input w CS2
                // Zamiast tego: ruch musi być poprawiany PRZEZ GRĘ

                _jiggleTimer += 0.05f;
                _jitterTimer += 0.1f;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SpinBot Error: {ex.Message}");
            }
        }

        // ✅ OBLICZANIE INNYCH TRYBÓW (Spin, Jiggle, Random, Custom)
        private static Vector3 CalculateOtherModes(Vector3 currentAngles)
        {
            Vector3 newAngles = currentAngles;

            if (currentSpinbotMode == 0) // SPIN
            {
                if (EnableY)
                {
                    _currentAngleY += SpinbotSpeed;
                    if (_currentAngleY >= 360f) _currentAngleY -= 360f;
                    newAngles.Y = _currentAngleY;
                }

                if (EnableX)
                {
                    _currentAngleX += SpinbotSpeed;
                    if (_currentAngleX >= 360f) _currentAngleX -= 360f;
                    newAngles.X = Math.Clamp(_currentAngleX % 180f - 90f, -89f, 89f);
                }

                if (EnableZ)
                {
                    _currentAngleZ += SpinbotSpeed;
                    if (_currentAngleZ >= 360f) _currentAngleZ -= 360f;
                    newAngles.Z = _currentAngleZ;
                }
            }
            else if (currentSpinbotMode == 1) // JIGGLE
            {
                _jiggleTimer += JiggleFrequency;

                if (EnableY)
                {
                    float jiggleY = (float)(Math.Sin(_jiggleTimer * 10) +
                                           Math.Cos(_jiggleTimer * 7) * 0.5) * JiggleIntensity;
                    newAngles.Y = currentAngles.Y + jiggleY;
                }

                if (EnableX)
                {
                    float jiggleX = (float)(Math.Sin(_jiggleTimer * 8) +
                                           Math.Cos(_jiggleTimer * 11) * 0.7) * JiggleIntensity;
                    newAngles.X = Math.Clamp(currentAngles.X + jiggleX, -89f, 89f);
                }

                if (EnableZ)
                {
                    float jiggleZ = (float)(Math.Sin(_jiggleTimer * 9) * JiggleIntensity * 0.5f);
                    newAngles.Z = currentAngles.Z + jiggleZ;
                }
            }
            else if (currentSpinbotMode == 2) // RANDOM
            {
                if (EnableY)
                {
                    newAngles.Y = (float)(_random.NextDouble() * (RandomMax - RandomMin) + RandomMin);
                }

                if (EnableX)
                {
                    newAngles.X = Math.Clamp((float)(_random.NextDouble() * 178f - 89f), -89f, 89f);
                }

                if (EnableZ)
                {
                    newAngles.Z = (float)(_random.NextDouble() * 360f - 180f);
                }
            }
            else if (currentSpinbotMode == 3) // CUSTOM
            {
                if (EnableX) newAngles.X = Math.Clamp(CustomAngleX, -89f, 89f);
                if (EnableY) newAngles.Y = CustomAngleY;
                if (EnableZ) newAngles.Z = CustomAngleZ;
            }

            // Anti-aim
            if (AntiAimEnabled)
            {
                ApplyAntiAim(ref newAngles);
            }

            // Normalizacja
            newAngles.Y = NormalizeAngle(newAngles.Y);
            newAngles.X = Math.Clamp(newAngles.X, -89f, 89f);
            newAngles.Z = 0f;

            return newAngles;
        }

        private static void ApplyAntiAim(ref Vector3 angles)
        {
            angles.X = Math.Clamp(PitchAngle, -89f, 89f);

            float baseYaw = YawAngle;
            if (InvertYaw)
            {
                baseYaw = NormalizeAngle(baseYaw + 180f);
            }

            if (JitterYaw)
            {
                _jitterTimer += 0.1f;
                float jitter = (float)Math.Sin(_jitterTimer) * JitterAmount;
                angles.Y = NormalizeAngle(baseYaw + jitter);
            }
            else
            {
                angles.Y = baseYaw;
            }
        }

        // ✅ POMOCNICZE FUNKCJE
        private static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        // ✅ GETTERY DLA GUI
        public static string CurrentModeName => SpinbotModes[currentSpinbotMode];

        public static string GetMovementStatus()
        {
            if (!_isLocalPlayerValid) return "NO PLAYER";
            if (!SpinbotEnabled) return "DISABLED";
            return "ACTIVE (CS2 Auto-Fix)";
        }

        public static Vector3 GetCurrentAngles()
        {
            return _viewAnglesBeforeAA;
        }

        // ✅ RESET
        public static void Reset()
        {
            _firstRun = true;
            _viewAnglesBeforeAA = Vector3.Zero;
            _isLocalPlayerValid = false;
        }

        protected override void FrameAction()
        {
            EnableSpinbot();
        }
    }
}