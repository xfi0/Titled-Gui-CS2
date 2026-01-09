using ImGuiNET;
using NAudio.Wave;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    internal class HitStuff : Classes.ThreadService
    {
        public static string[] HitSounds = new string[] { "Never Lose", "Skeet", "bottle", "gliter", "Tutamaka", "Minecraft", "Drum" };
        public static int CurrentHitSound = 0;
        public static bool Enabled = false;
        public static bool EnableHeadshotText = false;
        public static int PreviousDamage = 0;
        public static int PreviousHeadshots = 0;
        public static float Volume = 1.0f;
        public static Vector4 TextColor = new(1f, 1f, 1f, 1f);

        private static bool _firstEnable = true;
        private static bool _wasEnabled = false;
        private static int _currentRoundDamage = 0;
        private static int _currentRoundHeadshots = 0;

        public class HitText
        {
            public string? Text { get; set; }
            public DateTime ExpireAt { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 BasePosition { get; set; }
            public float State { get; set; } = 0f;
        }
        public static readonly List<HitText> Texts = new();

        public static void Update()
        {
            // Najpierw pobierz aktualne wartości
            GameState.LocalController = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerController);

            // Sprawdź czy kontroler jest poprawny
            if (GameState.LocalController == IntPtr.Zero)
            {
                _currentRoundDamage = 0;
                _currentRoundHeadshots = 0;
                return;
            }

            GameState.ActionTrackingServices = GameState.swed.ReadPointer(GameState.LocalController, Offsets.m_pActionTrackingServices);

            if (GameState.ActionTrackingServices == IntPtr.Zero)
            {
                _currentRoundDamage = 0;
                _currentRoundHeadshots = 0;
                return;
            }

            // Pobierz aktualne wartości z gry
            int currentRoundHeadshots = GameState.swed.ReadInt(GameState.ActionTrackingServices + Offsets.m_iNumRoundKillsHeadshots);
            int currentRoundDamage = (int)GameState.swed.ReadFloat(GameState.ActionTrackingServices + Offsets.m_flTotalRoundDamageDealt);

            // Sprawdź czy włączono moduł - zabezpieczenie przed dźwiękiem przy włączeniu
            if (_firstEnable && Enabled)
            {
                PreviousDamage = currentRoundDamage;
                PreviousHeadshots = currentRoundHeadshots;
                _currentRoundDamage = currentRoundDamage;
                _currentRoundHeadshots = currentRoundHeadshots;
                _firstEnable = false;
                return;
            }

            // Sprawdź czy Enabled się zmieniło z false na true
            if (!_wasEnabled && Enabled)
            {
                PreviousDamage = currentRoundDamage;
                PreviousHeadshots = currentRoundHeadshots;
                _currentRoundDamage = currentRoundDamage;
                _currentRoundHeadshots = currentRoundHeadshots;
                _wasEnabled = true;
                return;
            }

            if (!_wasEnabled && !Enabled)
                return;

            // Sprawdź zmianę obrażeń (BEZ resetowania przy nowej rundzie)
            if (Enabled && currentRoundDamage > PreviousDamage)
            {
                int damageDifference = currentRoundDamage - PreviousDamage;
                PreviousDamage = currentRoundDamage;

                // Odtwórz dźwięk tylko jeśli różnica obrażeń jest dodatnia
                if (damageDifference > 0)
                {
                    string soundFileName = HitSounds[CurrentHitSound].Replace(" ", "") + ".wav";
                    PlaySound(soundFileName);
                }
            }
            else if (Enabled && currentRoundDamage < PreviousDamage)
            {
                // Jeśli damage się zmniejszył (nowa runda), zaktualizuj poprzednią wartość
                PreviousDamage = currentRoundDamage;
            }

            // Sprawdź zmianę headshotów (BEZ resetowania przy nowej rundzie)
            if (EnableHeadshotText && currentRoundHeadshots > PreviousHeadshots)
            {
                int headshotDifference = currentRoundHeadshots - PreviousHeadshots;
                PreviousHeadshots = currentRoundHeadshots;

                // Dodaj tekst dla każdego nowego headshota
                if (headshotDifference > 0)
                {
                    Vector2 TextPos = new Vector2(
                        GameState.renderer.screenSize.X / 2,
                        GameState.renderer.screenSize.Y / 2
                    );

                    Texts.Add(new HitText
                    {
                        Text = "HEADSHOT",
                        ExpireAt = DateTime.Now.AddSeconds(1.5),
                        Position = TextPos,
                        BasePosition = TextPos
                    });
                }
            }
            else if (EnableHeadshotText && currentRoundHeadshots < PreviousHeadshots)
            {
                // Jeśli headshoty się zmniejszyły (nowa runda), zaktualizuj poprzednią wartość
                PreviousHeadshots = currentRoundHeadshots;
            }

            _currentRoundDamage = currentRoundDamage;
            _currentRoundHeadshots = currentRoundHeadshots;
            _wasEnabled = Enabled;
        }

        private static void PlaySound(string soundFileName)
        {
            if (string.IsNullOrEmpty(soundFileName) || !Enabled) return;

            try
            {
                // Użyj głównego systemu dźwięku z Renderer.cs
                Renderer.PlaySound(soundFileName, Volume);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error playing sound: {e.Message}");
            }
        }

        public static void CreateHitText()
        {
            if (!EnableHeadshotText || Texts.Count == 0) return;

            ImGui.PushFont(Renderer.TextFont48);

            for (int i = Texts.Count - 1; i >= 0; i--)
            {
                HitText hitText = Texts[i];

                if (DateTime.Now > hitText.ExpireAt)
                {
                    Texts.RemoveAt(i);
                    continue;
                }

                hitText.State += 1f;

                float X = hitText.BasePosition.X + 100f * MathF.Sin(hitText.State / 50f) - 50f;
                float Y = hitText.BasePosition.Y - 50f - (hitText.State * 2);

                Vector2 TextPos = new(X, Y);

                float LifeTime = (float)(hitText.ExpireAt - DateTime.Now).TotalMilliseconds;
                float totalLife = 1500f; // 1.5 s
                float alpha = Math.Clamp(LifeTime / totalLife, 0.1f, 1f);

                Vector4 TextColorAdjusted = new(TextColor.X, TextColor.Y, TextColor.Z, alpha);

                GameState.renderer.drawList.AddText(TextPos, ImGui.ColorConvertFloat4ToU32(TextColorAdjusted), hitText.Text);
            }

            ImGui.PopFont();
        }

        protected override async void FrameAction()
        {
            if (!Enabled && !EnableHeadshotText)
            {
                // Reset przy wyłączeniu
                if (_wasEnabled)
                {
                    _firstEnable = true;
                    _wasEnabled = false;
                    PreviousDamage = 0;
                    PreviousHeadshots = 0;
                }
                await Task.Delay(100);
                return;
            }

            Update();
            await Task.Delay(15);
        }

        // Dodana metoda do ręcznego resetu
        public static void Reset()
        {
            PreviousDamage = 0;
            PreviousHeadshots = 0;
            _currentRoundDamage = 0;
            _currentRoundHeadshots = 0;
            Texts.Clear();
            _firstEnable = true;
            _wasEnabled = false;
        }
    }
}