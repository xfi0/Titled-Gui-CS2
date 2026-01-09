using ClickableTransparentOverlay;
using ImGuiNET;
using NAudio.Wave;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using Titled_Gui.Modules.HvH;

namespace Titled_Gui
{
    public class Renderer : Overlay
    {
        #region WinAPI Imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentProcessId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;
        #endregion

        #region Fields
        public Vector2 screenSize = new(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        private IntPtr menuLogoTexture;
        private uint Width;
        private uint Height;
        private SpectatorList spectatorList;

        public List<Entity> entities = [];
        public Entity localPlayer = new();
        private readonly object entityLock = new();

        private int selectedTab = 0;

        public ImDrawListPtr drawList;
        public ImDrawListPtr BGdrawList;
        public ImDrawListPtr FGdrawList;
        public static Vector2 tabSize;
        public static Vector4 CurrentAccentColor => currentAccentColor;
        public static Vector4 CurrentSidebarColor => currentSidebarColor;
        public static Vector4 CurrentMainContentCol => currentMainContentCol;
        public static Vector4 CurrentTextCol => currentTextCol;

        public static bool DrawWindow = false;
        public static bool CanOpenMenu = false;
        public static float fpsUpdateInterval = 1.0f;
        public static float timeSinceLastUpdate = 0.0f;
        public static float lastFPS = 0.0f;

        // Konfigurowalne kolory menu
        public static Vector4 accentColor = new(0.26f, 0.59f, 0.98f, 1.00f);
        public static Vector4 SidebarColor = new(0.07f, 0.075f, 0.09f, 1.0f);
        public static Vector4 MainContentCol = new(0.094f, 0.102f, 0.118f, 1.0f);
        public static Vector4 TextCol = new(0.274f, 0.317f, 0.450f, 1.0f);
        public static Vector4 HeaderStartCol = TextCol;
        public static Vector4 HeaderEndCol = new(1, 1, 1, 0);

        public static float windowAlpha = 1f;
        private float animationSpeed = 0.15f;
        private static float WidgetColumnWidth = 160f;
        private static float LabelPadding = 4f;

        public static ImFontPtr TextFontNormal;
        public static ImFontPtr TextFontBig;
        public static ImFontPtr TextFont48;
        public static ImFontPtr TextFont60;
        public static ImFontPtr IconFont;
        public static ImFontPtr IconFont1;
        public static ImFontPtr GunIconsFont;

        public static bool EnableWaterMark = true;
        public static bool IsTextFontNormalLoaded => !TextFontNormal.Equals(default(ImFontPtr));
        public static bool IsTextFontBigLoaded => !TextFontBig.Equals(default(ImFontPtr));
        public static bool IsTextFont48Loaded => !TextFont48.Equals(default(ImFontPtr));
        public static bool IsTextFont60Loaded => !TextFont60.Equals(default(ImFontPtr));
        public static bool IsIconFontLoaded => !IconFont.Equals(default(ImFontPtr));
        public static bool IsIconFont1Loaded => !IconFont1.Equals(default(ImFontPtr));
        public static bool IsGunIconFontLoaded => !GunIconsFont.Equals(default(ImFontPtr));

        public static Vector4 trackCol = new(0.18f, 0.18f, 0.20f, 1f);
        public static Vector4 knobOff = new(0.15f, 0.15f, 0.15f, 1f);
        public static Vector4 knobOn = new(0.2745f, 0.3176f, 0.4510f, 1.0f);

        public static Vector4 ParticleColor = new(1f, 1f, 1f, 1f);
        public static Vector4 LineColor = new(1, 1, 1, 0.33f);
        public static float ParticleRadius = 2.5f;
        public static Vector2 BaseParticlePos = new();
        public static int NumberOfParticles = 50;
        public static Random random = new();
        public float ParticleSpeed = 0.53f;
        public static List<Vector2> Positions = [];
        public static List<Vector2> Velocities = [];
        public static float MaxLineDistance = 300f;
        public static ImGuiKey OpenKey = ImGuiKey.Insert;

        public static bool EnableFPSLimit = false;
        public static int TargetFPS = 144;
        public static int CurrentFPS = 0;
        private static float frameTimeAccumulator = 0f;
        private static int frameCount = 0;
        private static System.Diagnostics.Stopwatch fpsTimer = System.Diagnostics.Stopwatch.StartNew();

        private static bool wasInsertPressed = false;

        private IntPtr cs2WindowHandle = IntPtr.Zero;
        private IntPtr overlayWindowHandle = IntPtr.Zero;
        private bool isFocusManagementEnabled = true;

        // System muzyki
        private static AudioFileReader audioFileReader;
        private static WaveOutEvent outputDevice;
        private static List<string> musicFiles = new();
        private static int currentMusicIndex = 0;
        private static float targetVolume = 0.3f;
        private static float currentVolume = 0.3f;
        private static float menuVolume = 0.5f;
        private static float backgroundVolume = 0.1f;
        private static bool isFadingIn = false;
        private static bool isFadingOut = false;
        private static float fadeSpeed = 0.02f;
        public static bool EnableBackgroundMusic = true;
        public static bool MusicOnlyInMenu = false;
        private static bool musicSystemInitialized = false;
        private static object musicLock = new object();

        // System dźwięków
        private static Dictionary<string, WaveOutEvent> soundPlayers = new Dictionary<string, WaveOutEvent>();
        private static Dictionary<string, AudioFileReader> soundReaders = new Dictionary<string, AudioFileReader>();
        private static object soundLock = new object();

        public static Dictionary<string, bool> KeyBind = [];

        // Smooth transition dla kolorów
        private static Vector4 currentAccentColor;
        private static Vector4 currentSidebarColor;
        private static Vector4 currentMainContentCol;
        private static Vector4 currentTextCol;
        private static float colorTransitionSpeed = 0.1f;

        private static bool isDisposing = false;
        private DateTime lastSpectatorUpdate = DateTime.Now;

        // Animacja startowa
        private static bool showStartupAnimation = false;
        private static bool hasPlayedStartupAnimation = false;
        private static float startupAnimationProgress = 0f;
        private static float startupAnimationDuration = 4.0f;
        private static float startupAnimationTimer = 0f;
        private static Vector2 logoPosition;
        private static float logoScale = 1.0f;
        private static float logoAlpha = 0f;
        private static float lineTopProgress = 0f;
        private static float lineBottomProgress = 0f;
        private static float textFadeProgress = 0f;
        private static float glowIntensity = 0f;
        private static float fpsSlideProgress = 0f;
        private static string displayedText = "";
        private static float charRevealTimer = 0f;
        private static float charRevealDelay = 0.05f;
        private static bool wasCS2Active = false;
        private static float tScaleProgress = 0f;
        private static float fullTextAlpha = 0f;
        private static float textGlowAlpha = 0f;
        private static float particleExplosionProgress = 0f;
        private const string FULL_TEXT = "Tutamaka";
        private static List<Particle> textParticles = new List<Particle>();


        #endregion

        #region Particle Class for Animation
        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public Vector4 Color;
            public float Size;
            public float Life;
            public float MaxLife;
        }
        #endregion

        public void UpdateEntities(IEnumerable<Entity> newEntities) => entities = newEntities.ToList();

        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        #region CS2 Detection
        private bool IsCS2Active()
        {
            var cs2Processes = Process.GetProcessesByName("cs2");
            if (cs2Processes.Length == 0) return false;

            IntPtr foreground = GetForegroundWindow();
            GetWindowThreadProcessId(foreground, out uint foregroundPid);

            foreach (var proc in cs2Processes)
            {
                if (proc.Id == foregroundPid)
                    return true;
            }

            return false;
        }
        #endregion

        #region Music System
        private static void InitializeMusicSystem()
        {
            try
            {
                if (isDisposing) return;

                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string musicDir = Path.Combine(exeDirectory, "Resources", "music2");

                if (!Directory.Exists(musicDir))
                {
                    musicDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "music2");
                }

                if (!Directory.Exists(musicDir))
                {
                    Console.WriteLine("Folder z muzyką nie istnieje. Tworzę...");
                    Directory.CreateDirectory(musicDir);
                    musicSystemInitialized = false;
                    return;
                }

                musicFiles = Directory.GetFiles(musicDir, "*.mp3")
                    .Concat(Directory.GetFiles(musicDir, "*.wav"))
                    .ToList();

                if (musicFiles.Count == 0)
                {
                    musicSystemInitialized = false;
                    return;
                }

                Console.WriteLine($"Znaleziono {musicFiles.Count} plików muzycznych");
                currentVolume = backgroundVolume;
                musicSystemInitialized = true;

                if (EnableBackgroundMusic)
                {
                    PlayNextTrack();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd inicjalizacji systemu muzyki: {ex.Message}");
                musicSystemInitialized = false;
            }
        }

        private static void PlayNextTrack()
        {
            lock (musicLock)
            {
                try
                {
                    if (isDisposing || musicFiles.Count == 0) return;

                    StopMusic();

                    currentMusicIndex = (currentMusicIndex + 1) % musicFiles.Count;
                    string trackPath = musicFiles[currentMusicIndex];

                    audioFileReader = new AudioFileReader(trackPath);
                    outputDevice = new WaveOutEvent();
                    outputDevice.Init(audioFileReader);
                    outputDevice.Volume = currentVolume;
                    outputDevice.PlaybackStopped += OnPlaybackStopped;
                    outputDevice.Play();

                    Console.WriteLine($"Odtwarzam: {Path.GetFileName(trackPath)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd odtwarzania muzyki: {ex.Message}");
                }
            }
        }

        private static void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if (isDisposing) return;

            if (EnableBackgroundMusic && (!MusicOnlyInMenu || DrawWindow))
            {
                PlayNextTrack();
            }
        }

        private static void StopMusic()
        {
            lock (musicLock)
            {
                try
                {
                    if (outputDevice != null)
                    {
                        outputDevice.PlaybackStopped -= OnPlaybackStopped;
                        outputDevice.Stop();
                        outputDevice.Dispose();
                        outputDevice = null;
                    }

                    if (audioFileReader != null)
                    {
                        audioFileReader.Dispose();
                        audioFileReader = null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd zatrzymywania muzyki: {ex.Message}");
                }
            }
        }

        private static void UpdateMusicVolume()
        {
            lock (musicLock)
            {
                try
                {
                    if (outputDevice == null || !EnableBackgroundMusic || isDisposing) return;

                    // Fade in/out
                    if (isFadingIn && currentVolume < targetVolume)
                    {
                        currentVolume = Math.Min(currentVolume + fadeSpeed, targetVolume);
                        if (currentVolume >= targetVolume) isFadingIn = false;
                    }
                    else if (isFadingOut && currentVolume > targetVolume)
                    {
                        currentVolume = Math.Max(currentVolume - fadeSpeed, targetVolume);
                        if (currentVolume <= targetVolume) isFadingOut = false;
                    }

                    outputDevice.Volume = Math.Clamp(currentVolume, 0f, 1f);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd aktualizacji głośności: {ex.Message}");
                }
            }
        }

        private static void HandleMusicState(bool menuOpened)
        {
            if (!EnableBackgroundMusic || !musicSystemInitialized || isDisposing) return;

            lock (musicLock)
            {
                try
                {
                    if (MusicOnlyInMenu)
                    {
                        if (menuOpened && outputDevice == null)
                        {
                            PlayNextTrack();
                            targetVolume = menuVolume;
                            isFadingIn = true;
                            isFadingOut = false;
                        }
                        else if (!menuOpened && outputDevice != null)
                        {
                            StopMusic();
                        }
                    }
                    else
                    {
                        if (outputDevice == null)
                        {
                            PlayNextTrack();
                        }

                        if (menuOpened)
                        {
                            targetVolume = menuVolume;
                            isFadingIn = true;
                            isFadingOut = false;
                        }
                        else
                        {
                            targetVolume = backgroundVolume;
                            isFadingOut = true;
                            isFadingIn = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd zmiany stanu muzyki: {ex.Message}");
                }
            }
        }
        #endregion

        #region Sound System
        public static void PlaySound(string soundName, float volume = 1.0f)
        {
            if (isDisposing) return;

            Task.Run(() =>
            {
                lock (soundLock)
                {
                    try
                    {
                        // Najpierw zatrzymaj istniejący dźwięk jeśli gra
                        if (soundPlayers.ContainsKey(soundName))
                        {
                            var existingPlayer = soundPlayers[soundName];
                            var existingReader = soundReaders[soundName];

                            existingPlayer.Stop();
                            existingPlayer.Dispose();
                            existingReader.Dispose();

                            soundPlayers.Remove(soundName);
                            soundReaders.Remove(soundName);
                        }

                        string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                        string soundsDir = Path.Combine(exeDirectory, "Resources", "sounds");

                        if (!Directory.Exists(soundsDir))
                        {
                            soundsDir = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "sounds");

                            // Jeśli nadal nie istnieje, spróbuj utworzyć
                            if (!Directory.Exists(soundsDir))
                            {
                                Directory.CreateDirectory(soundsDir);
                                Console.WriteLine($"Created sounds directory: {soundsDir}");
                                return;
                            }
                        }

                        string soundPath = Path.Combine(soundsDir, soundName);

                        if (!File.Exists(soundPath))
                        {
                            Console.WriteLine($"Plik dźwiękowy nie istnieje: {soundPath}");

                            // DEBUG: Wypisz listę dostępnych plików
                            var availableFiles = Directory.GetFiles(soundsDir, "*.wav");
                            Console.WriteLine($"Dostępne pliki w {soundsDir}:");
                            foreach (var file in availableFiles)
                            {
                                Console.WriteLine($"  {Path.GetFileName(file)}");
                            }
                            return;
                        }

                        Console.WriteLine($"Playing sound: {soundName} from {soundPath}"); // DEBUG

                        var audioFileReader = new AudioFileReader(soundPath);
                        var waveOut = new WaveOutEvent();

                        waveOut.Init(audioFileReader);
                        waveOut.Volume = volume;

                        soundPlayers[soundName] = waveOut;
                        soundReaders[soundName] = audioFileReader;

                        waveOut.PlaybackStopped += (sender, e) =>
                        {
                            lock (soundLock)
                            {
                                if (soundPlayers.ContainsKey(soundName))
                                {
                                    var player = soundPlayers[soundName];
                                    var reader = soundReaders[soundName];

                                    player.Dispose();
                                    reader.Dispose();

                                    soundPlayers.Remove(soundName);
                                    soundReaders.Remove(soundName);
                                }
                            }
                        };

                        waveOut.Play();
                        Console.WriteLine($"Sound started: {soundName}"); // DEBUG
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Błąd odtwarzania dźwięku {soundName}: {ex.Message}");
                    }
                }
            });
        }

        public static void StopAllSounds()
        {
            lock (soundLock)
            {
                try
                {
                    foreach (var kvp in soundPlayers)
                    {
                        kvp.Value.Stop();
                        kvp.Value.Dispose();
                    }

                    foreach (var kvp in soundReaders)
                    {
                        kvp.Value.Dispose();
                    }

                    soundPlayers.Clear();
                    soundReaders.Clear();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Błąd zatrzymywania dźwięków: {ex.Message}");
                }
            }
        }

        private static void InitializeSoundSystem()
        {
            // Tworzenie folderu dźwięków jeśli nie istnieje
            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string soundsDir = Path.Combine(exeDirectory, "Resources", "sounds");

            if (!Directory.Exists(soundsDir))
            {
                Directory.CreateDirectory(soundsDir);
            }
        }
        #endregion

        #region Font Loading
        public static void LoadFonts()
        {
            try
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero) ImGui.CreateContext();

                var io = ImGui.GetIO();

                string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string fontsDir = Path.Combine(exeDirectory, "Resources", "fonts");

                if (!Directory.Exists(fontsDir))
                {
                    Console.WriteLine($"BŁĄD: Nie znaleziono folderu z fontami: {fontsDir}");
                    string alternativePath1 = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "fonts");
                    string alternativePath2 = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "Resources", "fonts");

                    if (Directory.Exists(alternativePath1))
                        fontsDir = alternativePath1;
                    else if (Directory.Exists(alternativePath2))
                        fontsDir = alternativePath2;
                    else
                    {
                        Console.WriteLine("Nie można znaleźć folderu Resources/fonts!");
                        return;
                    }
                }

                string notoSansPath = Path.Combine(fontsDir, "NotoSans-Bold.ttf");
                string glyphPath = Path.Combine(fontsDir, "glyph.ttf");
                string undefeatedPath = Path.Combine(fontsDir, "undefeated.ttf");
                string lineIconsPath = Path.Combine(fontsDir, "Lineicons.ttf");

                if (!File.Exists(notoSansPath))
                {
                    Console.WriteLine($"BŁĄD: Nie znaleziono pliku: {notoSansPath}");
                    return;
                }

                Console.WriteLine($"Ładowanie fontów z: {fontsDir}");

                TextFontNormal = io.Fonts.AddFontFromFileTTF(notoSansPath, 18.0f);
                TextFontBig = io.Fonts.AddFontFromFileTTF(notoSansPath, 24.0f);
                TextFont48 = io.Fonts.AddFontFromFileTTF(notoSansPath, 48.0f);
                TextFont60 = io.Fonts.AddFontFromFileTTF(notoSansPath, 80.0f);

                if (File.Exists(glyphPath))
                    IconFont = io.Fonts.AddFontFromFileTTF(glyphPath, 18.0f);

                if (File.Exists(undefeatedPath))
                    GunIconsFont = io.Fonts.AddFontFromFileTTF(undefeatedPath, 24.0f);

                if (File.Exists(lineIconsPath))
                {
                    ushort[] icons = [0xEB54, 0xEB55, 0];
                    unsafe
                    {
                        fixed (ushort* pIcons = icons)
                            IconFont1 = io.Fonts.AddFontFromFileTTF(lineIconsPath, 36.0f, null, (IntPtr)pIcons);
                    }
                }

                io.Fonts.Build();
                Console.WriteLine("Fonty załadowane pomyślnie!");
            }
            catch (Exception e)
            {
                Console.WriteLine($"BŁĄD przy ładowaniu fontów: {e.Message}");
            }
        }
        #endregion

        #region Focus Management
        private void FindCS2Window()
        {
            if (cs2WindowHandle == IntPtr.Zero)
            {
                cs2WindowHandle = FindWindow(null, "Counter-Strike 2");
                if (cs2WindowHandle == IntPtr.Zero)
                    cs2WindowHandle = FindWindow(null, "cs2");

                if (cs2WindowHandle != IntPtr.Zero)
                    Console.WriteLine("Znaleziono okno CS2!");
            }
        }

        private void SwitchFocus(bool toOverlay)
        {
            if (!isFocusManagementEnabled) return;

            try
            {
                if (toOverlay)
                {
                    IntPtr currentWindow = GetForegroundWindow();
                    if (currentWindow != IntPtr.Zero)
                    {
                        overlayWindowHandle = FindWindow(null, "TitledOverlay");
                        if (overlayWindowHandle == IntPtr.Zero)
                            overlayWindowHandle = currentWindow;

                        if (overlayWindowHandle != IntPtr.Zero)
                        {
                            uint currentThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
                            uint targetThreadId = GetWindowThreadProcessId(overlayWindowHandle, out _);

                            AttachThreadInput(currentThreadId, targetThreadId, true);
                            ShowWindow(overlayWindowHandle, SW_RESTORE);
                            SetForegroundWindow(overlayWindowHandle);
                            BringWindowToTop(overlayWindowHandle);
                            AttachThreadInput(currentThreadId, targetThreadId, false);
                        }
                    }
                }
                else
                {
                    FindCS2Window();
                    if (cs2WindowHandle != IntPtr.Zero)
                    {
                        uint currentThreadId = GetWindowThreadProcessId(GetForegroundWindow(), out _);
                        uint targetThreadId = GetWindowThreadProcessId(cs2WindowHandle, out _);

                        AttachThreadInput(currentThreadId, targetThreadId, true);
                        ShowWindow(cs2WindowHandle, SW_RESTORE);
                        SetForegroundWindow(cs2WindowHandle);
                        BringWindowToTop(cs2WindowHandle);
                        AttachThreadInput(currentThreadId, targetThreadId, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd przy przełączaniu fokusu: {ex.Message}");
            }
        }
        #endregion

        #region Startup Animation
        private void UpdateTextParticles(float dt)
        {
            for (int i = textParticles.Count - 1; i >= 0; i--)
            {
                var particle = textParticles[i];
                particle.Position += particle.Velocity;
                particle.Velocity *= 0.95f;
                particle.Life -= dt / particle.MaxLife;

                if (particle.Life <= 0)
                {
                    textParticles.RemoveAt(i);
                }
            }
        }

        private void CreateTextExplosion()
        {
            Vector2 center = new Vector2(screenSize.X / 2, screenSize.Y / 2);
            for (int i = 0; i < 50; i++)
            {
                float angle = (float)random.NextDouble() * MathF.PI * 2;
                float speed = 2 + (float)random.NextDouble() * 4;

                textParticles.Add(new Particle
                {
                    Position = center,
                    Velocity = new Vector2(MathF.Cos(angle) * speed, MathF.Sin(angle) * speed),
                    Color = new Vector4(
                        accentColor.X + (float)random.NextDouble() * 0.3f,
                        accentColor.Y + (float)random.NextDouble() * 0.3f,
                        accentColor.Z + (float)random.NextDouble() * 0.3f,
                        0.8f
                    ),
                    Size = 1 + (float)random.NextDouble() * 3,
                    Life = 1.0f + (float)random.NextDouble() * 0.5f,
                    MaxLife = 1.0f + (float)random.NextDouble() * 0.5f
                });
            }
        }

        private void RenderBackground(float alpha)
        {
            if (alpha <= 0f) return;

            var drawList = ImGui.GetBackgroundDrawList();
            Vector4 topColor = new Vector4(0f, 0f, 0f, alpha * 0.8f);
            Vector4 bottomColor = new Vector4(0.1f, 0.1f, 0.15f, alpha * 0.3f);

            drawList.AddRectFilledMultiColor(
                Vector2.Zero,
                screenSize,
                ImGui.ColorConvertFloat4ToU32(topColor),
                ImGui.ColorConvertFloat4ToU32(topColor),
                ImGui.ColorConvertFloat4ToU32(bottomColor),
                ImGui.ColorConvertFloat4ToU32(bottomColor)
            );
        }

        private void RenderStartupAnimation()
        {
            if (!showStartupAnimation) return;

            float dt = ImGui.GetIO().DeltaTime;
            startupAnimationTimer += dt;
            startupAnimationProgress = Math.Min(startupAnimationTimer / startupAnimationDuration, 1.0f);
            float t = startupAnimationProgress;

            UpdateTextParticles(dt);

            if (t < 0.12f)
            {
                float phase1 = t / 0.12f;
                tScaleProgress = EaseOutElastic(phase1 * 0.7f);
                logoAlpha = EaseOutCubic(phase1);

                if (phase1 > 0.3f && textParticles.Count < 20)
                {
                    Vector2 center = new Vector2(screenSize.X / 2, screenSize.Y / 2);
                    for (int i = 0; i < 5; i++)
                    {
                        textParticles.Add(new Particle
                        {
                            Position = center,
                            Velocity = new Vector2((float)(random.NextDouble() * 4 - 2), (float)(random.NextDouble() * 4 - 2)),
                            Color = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.8f),
                            Size = 2 + (float)random.NextDouble() * 3,
                            Life = 1.0f,
                            MaxLife = 1.0f
                        });
                    }
                }

                displayedText = "T";
                fullTextAlpha = 0f;
                textGlowAlpha = EaseOutQuad(phase1);
                float bgAlpha = 0.25f * EaseOutQuad(phase1);
                RenderBackground(bgAlpha);
            }
            else if (t < 0.32f)
            {
                float phase2 = (t - 0.12f) / 0.20f;
                tScaleProgress = 1.0f;
                logoAlpha = 1.0f;

                charRevealTimer += dt;
                if (charRevealTimer >= charRevealDelay && displayedText.Length < FULL_TEXT.Length)
                {
                    displayedText = FULL_TEXT.Substring(0, displayedText.Length + 1);
                    charRevealTimer = 0f;

                    if (displayedText.Length > 1)
                    {
                        Vector2 center = new Vector2(screenSize.X / 2, screenSize.Y / 2);
                        for (int i = 0; i < 3; i++)
                        {
                            textParticles.Add(new Particle
                            {
                                Position = center + new Vector2((displayedText.Length - 2) * 40, 0),
                                Velocity = new Vector2((float)(random.NextDouble() * 3 - 1.5f), (float)(random.NextDouble() * 3 - 1.5f)),
                                Color = new Vector4(1f, 1f, 1f, 0.6f),
                                Size = 1 + (float)random.NextDouble() * 2,
                                Life = 0.8f,
                                MaxLife = 0.8f
                            });
                        }
                    }
                }

                fullTextAlpha = EaseOutCubic(phase2);
                textGlowAlpha = 0.5f + 0.5f * (float)Math.Sin(phase2 * MathF.PI * 4);
                RenderBackground(0.25f);
            }
            else if (t < 0.40f)
            {
                float phase3 = (t - 0.32f) / 0.08f;
                displayedText = FULL_TEXT;
                fullTextAlpha = 1.0f;
                lineTopProgress = EaseOutBack(phase3);
                lineBottomProgress = 0f;
                glowIntensity = 0f;
                textGlowAlpha = 0.3f;
                RenderBackground(0.25f);
            }
            else if (t < 0.48f)
            {
                float phase4 = (t - 0.40f) / 0.08f;
                displayedText = FULL_TEXT;
                fullTextAlpha = 1.0f;
                lineTopProgress = 1.0f;
                lineBottomProgress = EaseOutBack(phase4);

                if (phase4 > 0.7f)
                {
                    glowIntensity = 0.6f * (phase4 - 0.7f) * 3.33f;
                }

                textGlowAlpha = 0.3f + 0.2f * (float)Math.Sin(phase4 * MathF.PI * 2);
                RenderBackground(0.25f);
            }
            else if (t < 0.58f)
            {
                float phase5 = (t - 0.48f) / 0.10f;
                displayedText = FULL_TEXT;
                fullTextAlpha = 1.0f;
                lineTopProgress = 1.0f;
                lineBottomProgress = 1.0f;
                glowIntensity = 0.6f * (0.5f + 0.5f * (float)Math.Sin(phase5 * MathF.PI * 4));
                textGlowAlpha = 0.3f + 0.2f * (float)Math.Sin(phase5 * MathF.PI * 3);
                RenderBackground(0.25f);
            }
            else if (t < 0.68f)
            {
                float phase6 = (t - 0.58f) / 0.10f;
                displayedText = FULL_TEXT;
                fullTextAlpha = 1.0f;

                lineTopProgress = 1.0f - EaseInCubic(phase6);

                float bottomPhase = Math.Max(0f, (phase6 - 0.15f) / 0.85f);
                lineBottomProgress = 1.0f - EaseInCubic(bottomPhase);

                glowIntensity = 0.6f * (1.0f - phase6);
                textGlowAlpha = 0.3f * (1.0f - phase6);
                fpsSlideProgress = EaseOutQuad(phase6);

                if (phase6 > 0.5f && particleExplosionProgress == 0f)
                {
                    CreateTextExplosion();
                    particleExplosionProgress = 1.0f;
                }

                RenderBackground(0.25f * (1.0f - phase6));
            }
            else if (t < 0.92f)
            {
                float phase7 = (t - 0.68f) / 0.24f;
                float easedPhase = EaseInOutQuint(phase7);
                displayedText = FULL_TEXT;

                Vector2 targetPos = new Vector2(screenSize.X - 120, 35);
                Vector2 startPos = new Vector2(screenSize.X / 2, screenSize.Y / 2);

                float arcHeight = 80f;
                Vector2 linearPos = Vector2.Lerp(startPos, targetPos, easedPhase);
                float arc = MathF.Sin(easedPhase * MathF.PI) * arcHeight;

                logoPosition = new Vector2(linearPos.X, linearPos.Y - arc);

                float scaleProgress = easedPhase;
                float squeeze = 1.0f - 0.2f * MathF.Sin(easedPhase * MathF.PI);
                logoScale = (1.0f - (scaleProgress * 0.70f)) * squeeze;

                fullTextAlpha = 1.0f - (easedPhase * 0.7f);

                float bgFadeProgress = 1f - EaseOutCubic(easedPhase);
                RenderBackground(0.25f * bgFadeProgress);

                fpsSlideProgress = 1f;
                lineTopProgress = 0f;
                lineBottomProgress = 0f;
                glowIntensity = 0f;
            }
            else if (t < 1.0f)
            {
                float phase8 = (t - 0.92f) / 0.08f;
                displayedText = FULL_TEXT;
                Vector2 finalPos = new Vector2(screenSize.X - 120, 35);

                float bounce = 5f * MathF.Sin(phase8 * MathF.PI * 2) * (1f - phase8);
                logoPosition = finalPos + new Vector2(0, bounce);

                logoScale = 0.3f;
                fullTextAlpha = 0.3f;
                RenderBackground(0f);
                fpsSlideProgress = 1f;

                if (phase8 > 0.5f && textParticles.Count < 10)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        textParticles.Add(new Particle
                        {
                            Position = logoPosition,
                            Velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)),
                            Color = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.5f),
                            Size = 1 + (float)random.NextDouble(),
                            Life = 0.5f,
                            MaxLife = 0.5f
                        });
                    }
                }
            }
            else
            {
                showStartupAnimation = false;
                CanOpenMenu = true;
                DrawWindow = false;

                lineTopProgress = 0f;
                lineBottomProgress = 0f;
                fpsSlideProgress = 0f;
                glowIntensity = 0f;
                displayedText = "";
                fullTextAlpha = 0f;
                textGlowAlpha = 0f;
                textParticles.Clear();
                particleExplosionProgress = 0f;
            }

            RenderAnimationElements();
        }

        private void RenderAnimationElements()
        {
            if (!IsTextFont60Loaded) return;

            var animDrawList = ImGui.GetBackgroundDrawList();

            if (logoPosition == Vector2.Zero)
            {
                logoPosition = new Vector2(screenSize.X / 2, screenSize.Y / 2);
            }

            ImGui.PushFont(TextFont60);
            float fontSize = 80f * logoScale;
            Vector2 textSize = ImGui.CalcTextSize(FULL_TEXT) * logoScale;
            Vector2 currentTextSize = ImGui.CalcTextSize(displayedText) * logoScale;

            Vector2 textPos = logoPosition - textSize * 0.5f;

            foreach (var particle in textParticles)
            {
                float alpha = particle.Life / particle.MaxLife;
                uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(
                    particle.Color.X,
                    particle.Color.Y,
                    particle.Color.Z,
                    particle.Color.W * alpha
                ));

                animDrawList.AddCircleFilled(particle.Position, particle.Size * alpha, color, 12);

                if (alpha > 0.3f)
                {
                    Vector2 trailPos = particle.Position - particle.Velocity * 0.2f;
                    uint trailColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                        particle.Color.X,
                        particle.Color.Y,
                        particle.Color.Z,
                        particle.Color.W * alpha * 0.3f
                    ));
                    animDrawList.AddLine(particle.Position, trailPos, trailColor, particle.Size * alpha * 0.5f);
                }
            }

            if (lineTopProgress > 0f || lineBottomProgress > 0f)
            {
                Vector2 center = new Vector2(screenSize.X / 2, screenSize.Y / 2);
                float maxLength = screenSize.X * 0.4f;
                float lineThickness = 4.0f;
                float lineOffset = 60f;

                if (lineTopProgress > 0f)
                {
                    float topLength = maxLength * lineTopProgress;
                    Vector2 lineStart = new Vector2(center.X - topLength, center.Y - lineOffset);
                    Vector2 lineEnd = new Vector2(center.X + topLength, center.Y - lineOffset);

                    uint startColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                        accentColor.X, accentColor.Y, accentColor.Z, lineTopProgress * 0.8f
                    ));
                    uint endColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                        accentColor.X * 0.7f, accentColor.Y * 0.7f, accentColor.Z * 0.7f, lineTopProgress * 0.6f
                    ));

                    animDrawList.AddLine(lineStart, lineEnd, startColor, lineThickness);

                    if (glowIntensity > 0f)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            float offset = i * 1.0f;
                            float alpha = lineTopProgress * glowIntensity * (1.0f - i * 0.2f);

                            uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                                accentColor.X, accentColor.Y, accentColor.Z, alpha
                            ));

                            animDrawList.AddLine(
                                new Vector2(lineStart.X, lineStart.Y - offset),
                                new Vector2(lineEnd.X, lineEnd.Y - offset),
                                glowColor,
                                lineThickness - i * 0.5f
                            );

                            animDrawList.AddLine(
                                new Vector2(lineStart.X, lineStart.Y + offset),
                                new Vector2(lineEnd.X, lineEnd.Y + offset),
                                glowColor,
                                lineThickness - i * 0.5f
                            );
                        }
                    }
                }

                if (lineBottomProgress > 0f)
                {
                    float bottomLength = maxLength * lineBottomProgress;
                    Vector2 lineStart = new Vector2(center.X - bottomLength, center.Y + lineOffset);
                    Vector2 lineEnd = new Vector2(center.X + bottomLength, center.Y + lineOffset);

                    uint lineColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                        accentColor.X, accentColor.Y, accentColor.Z, lineBottomProgress
                    ));
                    animDrawList.AddLine(lineStart, lineEnd, lineColor, lineThickness);

                    if (glowIntensity > 0f)
                    {
                        for (int i = 1; i <= 4; i++)
                        {
                            float offset = i * 1.0f;
                            float alpha = lineBottomProgress * glowIntensity * (1.0f - i * 0.2f);
                            uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                                accentColor.X, accentColor.Y, accentColor.Z, alpha
                            ));

                            animDrawList.AddLine(
                                new Vector2(lineStart.X, lineStart.Y - offset),
                                new Vector2(lineEnd.X, lineEnd.Y - offset),
                                glowColor,
                                lineThickness - i * 0.5f
                            );

                            animDrawList.AddLine(
                                new Vector2(lineStart.X, lineStart.Y + offset),
                                new Vector2(lineEnd.X, lineEnd.Y + offset),
                                glowColor,
                                lineThickness - i * 0.5f
                            );
                        }
                    }
                }
            }

            if (textGlowAlpha > 0f && fullTextAlpha > 0f)
            {
                for (int size = 12; size > 0; size -= 2)
                {
                    float alpha = textGlowAlpha * (size / 12f) * fullTextAlpha;

                    for (int i = -size; i <= size; i += 2)
                    {
                        for (int j = -size; j <= size; j += 2)
                        {
                            if (Math.Abs(i) + Math.Abs(j) > size * 0.8f) continue;

                            uint glowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                                accentColor.X, accentColor.Y, accentColor.Z,
                                alpha * 0.1f
                            ));

                            animDrawList.AddText(TextFont60, fontSize, textPos + new Vector2(i, j), glowColor, displayedText);
                        }
                    }
                }
            }

            if (fullTextAlpha > 0f)
            {
                for (int i = -4; i <= 4; i++)
                {
                    for (int j = -4; j <= 4; j++)
                    {
                        if (i == 0 && j == 0) continue;

                        float distance = MathF.Sqrt(i * i + j * j);
                        float alpha = fullTextAlpha * 0.3f / (distance * 0.7f);

                        uint shadowColor = ImGui.ColorConvertFloat4ToU32(new Vector4(
                            0f, 0f, 0f, alpha
                        ));

                        animDrawList.AddText(TextFont60, fontSize, textPos + new Vector2(i, j), shadowColor, displayedText);
                    }
                }
            }

            if (fullTextAlpha > 0f)
            {
                Vector4 textStartColor = new Vector4(1f, 1f, 1f, logoAlpha * fullTextAlpha);
                Vector4 textEndColor = new Vector4(
                    accentColor.X * 0.8f,
                    accentColor.Y * 0.8f,
                    accentColor.Z * 0.8f,
                    logoAlpha * fullTextAlpha
                );

                float step = 1f / (displayedText.Length - 1);
                Vector2 currentPos = textPos;

                for (int i = 0; i < displayedText.Length; i++)
                {
                    float t = i * step;
                    Vector4 color = Vector4.Lerp(textStartColor, textEndColor, t);

                    string charStr = displayedText[i].ToString();
                    Vector2 charSize = ImGui.CalcTextSize(charStr) * logoScale;

                    animDrawList.AddText(TextFont60, fontSize, currentPos, ImGui.ColorConvertFloat4ToU32(color), charStr);
                    currentPos.X += charSize.X;
                }
            }

            ImGui.PopFont();

            if (fpsSlideProgress > 0f)
            {
                ImGui.PushFont(TextFontBig);
                string fpsText = $"FPS: {CurrentFPS}";
                Vector2 fpsSize = ImGui.CalcTextSize(fpsText);

                float slideOffset = (1.0f - EaseOutBack(fpsSlideProgress)) * 40f;
                float fpsAlpha = EaseOutQuad(fpsSlideProgress) * logoAlpha * fullTextAlpha;

                Vector2 fpsPos = textPos + new Vector2(
                    (textSize.X - fpsSize.X) * 0.5f,
                    textSize.Y + 20 + slideOffset
                );

                animDrawList.AddText(
                    fpsPos + new Vector2(2, 2),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, fpsAlpha * 0.6f)),
                    fpsText
                );

                Vector4 fpsStartColor = new Vector4(TextCol.X, TextCol.Y, TextCol.Z, fpsAlpha);
                Vector4 fpsEndColor = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, fpsAlpha);

                float fpsStep = 1f / (fpsText.Length - 1);
                Vector2 currentFpsPos = fpsPos;

                for (int i = 0; i < fpsText.Length; i++)
                {
                    float t = i * fpsStep;
                    Vector4 color = Vector4.Lerp(fpsStartColor, fpsEndColor, t);

                    string charStr = fpsText[i].ToString();
                    Vector2 charSize = ImGui.CalcTextSize(charStr);

                    animDrawList.AddText(currentFpsPos, ImGui.ColorConvertFloat4ToU32(color), charStr);
                    currentFpsPos.X += charSize.X;
                }

                ImGui.PopFont();
            }
        }

        private float EaseOutElastic(float t)
        {
            float c4 = (2 * MathF.PI) / 3;
            return t == 0 ? 0 : t == 1 ? 1 : MathF.Pow(2, -10 * t) * MathF.Sin((t * 10 - 0.75f) * c4) + 1;
        }

        private float EaseOutBack(float t)
        {
            float c1 = 1.70158f;
            float c3 = c1 + 1f;
            return 1f + c3 * MathF.Pow(t - 1f, 3f) + c1 * MathF.Pow(t - 1f, 2f);
        }

        private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

        private float EaseInOutQuint(float t) => t < 0.5f ? 16f * t * t * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 5f) / 2f;

        private float EaseOutCubic(float t) => 1f - MathF.Pow(1f - t, 3f);

        private float EaseInCubic(float t) => t * t * t;

        public void StartAnimation()
        {
            showStartupAnimation = true;
            hasPlayedStartupAnimation = true;
            CanOpenMenu = false;
            DrawWindow = false;
            startupAnimationTimer = 0f;
            startupAnimationProgress = 0f;
            logoPosition = new Vector2(screenSize.X / 2, screenSize.Y / 2);
            logoScale = 1.0f;
            logoAlpha = 0f;
            lineTopProgress = 0f;
            lineBottomProgress = 0f;
            textFadeProgress = 0f;
            glowIntensity = 0f;
            fpsSlideProgress = 0f;
            displayedText = "";
            charRevealTimer = 0f;
            tScaleProgress = 0f;
            fullTextAlpha = 0f;
            textGlowAlpha = 0f;
            particleExplosionProgress = 0f;
            textParticles.Clear();
        }
        #endregion

        #region Render Loop
        protected override void Render()
        {
            try
            {
                if (!musicSystemInitialized)
                    InitializeMusicSystem();

                if (EnableFPSLimit && TargetFPS > 0)
                {
                    float targetFrameTime = 1000f / TargetFPS;
                    Thread.Sleep(Math.Max(1, (int)targetFrameTime - 1));
                }


                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                io.Framerate = 0;
                io.ConfigViewportsNoAutoMerge = true;
                io.ConfigViewportsNoTaskBarIcon = true;


                if ((DateTime.Now - lastSpectatorUpdate).TotalMilliseconds > 50)
                {
                    SpectatorList.Update();
                    lastSpectatorUpdate = DateTime.Now;
                }


                if (Modules.Visual.SoundESP.enabled)
                {
                    Modules.Visual.SoundESP.UpdateSoundESP();
                }
                UpdateFPSCounter();
                UpdateMusicVolume();
                UpdateColorTransitions();

                bool currentCS2State = IsCS2Active();
                if (currentCS2State && !wasCS2Active && !hasPlayedStartupAnimation)
                {
                    StartAnimation();
                }
                wasCS2Active = currentCS2State;

                if (showStartupAnimation)
                {
                    RenderStartupAnimation();
                }
                else
                {
                    if (CanOpenMenu)
                    {
                        CheckInsertKey();
                        NotificationManager.Render();
                        RenderESPOverlay();
                        RenderMainWindow();
                        RenderWaterMark();
                        BombTimerOverlay.TimeOverlay();
                        SpectatorList.Render();
                        Modules.Visual.ModelChanger.Update();
                    }
                    else
                    {
                        RenderESPOverlay();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Błąd w Render: {ex.Message}");
            }
        }

        private void UpdateFPSCounter()
        {
            frameCount++;
            frameTimeAccumulator += ImGui.GetIO().DeltaTime;

            if (frameTimeAccumulator >= 1.0f)
            {
                CurrentFPS = frameCount;
                frameCount = 0;
                frameTimeAccumulator = 0f;
            }
        }

        private void UpdateColorTransitions()
        {
            currentAccentColor = Vector4.Lerp(currentAccentColor, accentColor, colorTransitionSpeed);
            currentSidebarColor = Vector4.Lerp(currentSidebarColor, SidebarColor, colorTransitionSpeed);
            currentMainContentCol = Vector4.Lerp(currentMainContentCol, MainContentCol, colorTransitionSpeed);
            currentTextCol = Vector4.Lerp(currentTextCol, TextCol, colorTransitionSpeed);

            knobOn = currentTextCol;
            HeaderStartCol = currentTextCol;
        }

        private void CheckInsertKey()
        {
            if (!CanOpenMenu) return;

            bool isInsertPressed = (User32.GetAsyncKeyState((int)Keys.Insert) & 0x8000) != 0;

            if (isInsertPressed && !wasInsertPressed)
            {
                bool previousState = DrawWindow;
                DrawWindow = !DrawWindow;
                wasInsertPressed = true;

                if (DrawWindow && !previousState)
                {
                    SwitchFocus(true);
                    HandleMusicState(true);
                }
                else if (!DrawWindow && previousState)
                {
                    SwitchFocus(false);
                    HandleMusicState(false);
                }
            }
            else if (!isInsertPressed && wasInsertPressed)
            {
                wasInsertPressed = false;
            }

            if (ImGui.IsKeyPressed(OpenKey, false))
            {
                bool previousState = DrawWindow;
                DrawWindow = !DrawWindow;

                if (DrawWindow && !previousState)
                {
                    SwitchFocus(true);
                    HandleMusicState(true);
                }
                else if (!DrawWindow && previousState)
                {
                    SwitchFocus(false);
                    HandleMusicState(false);
                }
            }
        }
        #endregion

        #region Rendering Methods
        public void RenderWaterMark()
        {
            if (EnableWaterMark && IsTextFontBigLoaded)
            {
                ImGui.SetNextWindowSize(new(150, 60));
                ImGui.SetNextWindowPos(new(screenSize.X - 160, 10));
                ImGui.Begin("wm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoInputs);
                ImGui.PushFont(TextFontBig);
                var DrawList = ImGui.GetWindowDrawList();
                Vector2 TextPosition = ImGui.GetWindowPos() + new Vector2(10, 8);
                DrawList.AddText(TextPosition, ImGui.ColorConvertFloat4ToU32(currentAccentColor), "Tutamaka");

                timeSinceLastUpdate += ImGui.GetIO().DeltaTime;
                if (timeSinceLastUpdate >= fpsUpdateInterval)
                {
                    lastFPS = 1f / ImGui.GetIO().DeltaTime;
                    timeSinceLastUpdate = 0.0f;
                }

                DrawList.AddText(new(TextPosition.X, TextPosition.Y + 20f), ImGui.ColorConvertFloat4ToU32(currentTextCol), $"FPS: {Math.Round(lastFPS)}");
                ImGui.PopFont();
                ImGui.End();
            }
        }

        private void RenderESPOverlay()
        {
            ImGui.SetNextWindowSize(screenSize);
            ImGui.SetNextWindowPos(Vector2.Zero);
            ImGui.Begin("TitledOverlay",
                ImGuiWindowFlags.NoTitleBar |
                ImGuiWindowFlags.NoResize |
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse |
                ImGuiWindowFlags.NoCollapse |
                ImGuiWindowFlags.NoBackground |
                ImGuiWindowFlags.NoSavedSettings |
                ImGuiWindowFlags.NoInputs |
                ImGuiWindowFlags.NoMove
            );
            drawList = ImGui.GetWindowDrawList();
            ImGui.End();
        }

        private void RenderMainWindow()
        {
            BGdrawList = ImGui.GetBackgroundDrawList();
            if (DrawWindow)
            {
                BGdrawList.AddRectFilled(Vector2.Zero, screenSize, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.5f)));

                if (NumberOfParticles > 0)
                    DrawParticles(NumberOfParticles);

                ImGui.SetNextWindowPos(new Vector2((screenSize.X - 800) / 2f, (screenSize.Y - 600) / 2f), ImGuiCond.Always);
                ApplyCustomStyle();

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.Begin("", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking);
                ImGui.SetWindowSize(new(800, 600));

                Vector2 tabPos = ImGui.GetCursorScreenPos();
                tabSize = new(100, ImGui.GetContentRegionAvail().Y);
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(tabPos, tabPos + tabSize, ImGui.ColorConvertFloat4ToU32(currentSidebarColor), 12.0f, ImDrawFlags.RoundCornersLeft);

                ImGui.BeginChild("Sidebar", tabSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    RenderSidebar();
                }
                ImGui.EndChild();

                ImGui.SameLine(0f, 0f);

                Vector2 mainPos = ImGui.GetCursorScreenPos();
                Vector2 mainSize = ImGui.GetContentRegionAvail();

                drawList.AddRectFilled(mainPos, mainPos + mainSize, ImGui.ColorConvertFloat4ToU32(currentMainContentCol), 12.0f, ImDrawFlags.RoundCornersBottom);

                ImGui.BeginChild("MainContent", mainSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    ImGui.PopStyleVar();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12.0f);
                    RenderTitle("Tutamaka");
                    RenderTabContent();
                }
                ImGui.EndChild();

                ImGui.End();
                RunAllModules();
            }
            else
            {
                RunAllModules();
            }
        }

        private void RenderSidebar()
        {
            float LogoWidth = 120;
            float offset = (ImGui.GetContentRegionAvail().X - LogoWidth) * 0.5f;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

            string exeDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logoPath = Path.Combine(exeDirectory, "Resources", "MenuLogo.png");

            if (!File.Exists(logoPath))
                logoPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", "MenuLogo.png");
            if (!File.Exists(logoPath))
                logoPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location)!, "Resources", "MenuLogo.png");

            if (File.Exists(logoPath))
            {
                AddOrGetImagePointer(logoPath, true, out menuLogoTexture, out Width, out Height);
                ImGui.Image(menuLogoTexture, new(120, 120));
            }
            else
            {
                ImGui.Text("LOGO");
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            RenderTabButton("E", 0);
            RenderTabButton("D", 1);
            RenderTabButton("C", 2);
            RenderTabButton("B", 3);
            RenderTabButton("A", 4);
            RenderTabButton("\uEB54", 5);

            var availableHeight = ImGui.GetContentRegionAvail().Y;
            var cogButtonHeight = 35f;
            var spacingHeight = availableHeight - cogButtonHeight - 5f;

            if (spacingHeight > 0)
                ImGui.Dummy(new(0, spacingHeight));

            Vector2 cogPos = ImGui.GetCursorScreenPos();
            Vector2 cogSize = new(ImGui.GetContentRegionAvail().X, cogButtonHeight);

            if (ImGui.InvisibleButton("##SettingsGear", cogSize))
                selectedTab = 6;

            bool isHovered = ImGui.IsItemHovered();
            bool isSettingsSelected = selectedTab == 6;

            Vector2 gearCenter = new(cogPos.X + cogSize.X / 2, cogPos.Y + cogSize.Y / 2);

            uint gearColor;
            if (isSettingsSelected)
                gearColor = ImGui.ColorConvertFloat4ToU32(currentAccentColor);
            else if (isHovered)
                gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, 1));
            else
                gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 1));

            DrawGearIcon(gearCenter, gearColor);
        }

        private void RenderTabContent()
        {
            switch (selectedTab)
            {
                case 0: RenderLegitTab(); break;
                case 1: RenderRageTab(); break;
                case 2: RenderVisualsTab(); break;
                case 3: RenderHvHTab(); break;
                case 4: RenderSkinTab(); break;
                case 5: RenderConfigTab(); break;
                case 6: RenderSettingsTab(); break;
            }
        }

        private void RenderLegitTab()
        {
            ImGui.Columns(2, "Legit Columns", true);
            ImGui.BeginChild("LeftLegit");
            RenderBoolSettingWithWarning("Auto BHOP", ref Modules.Legit.Bhop.BhopEnable);
            RenderBoolSetting("Auto Strafe Enable", ref Bhop.AutoStrafeEnable);
            RenderBoolSetting("Hit Sound", ref HitStuff.Enabled);
            RenderFloatSlider("Hit Sound Volume", ref HitStuff.Volume, 0, 1);
            RenderIntCombo("Current Hit Sound", ref HitStuff.CurrentHitSound, HitStuff.HitSounds, HitStuff.HitSounds.Length);
            RenderBoolSettingWith1ColorPicker("Headshot Text", ref HitStuff.EnableHeadshotText, ref HitStuff.TextColor);
            RenderBoolSetting("Jump Shot", ref Modules.Legit.JumpHack.JumpHackEnabled);
            RenderKeybindChooser("Jump Shot Keybind", ref JumpHack.JumpHotkey);

            // W RenderVisualsTab() dodaj:
            // W RenderVisualsTab() dodaj:
            ImGui.EndChild();
            ImGui.NextColumn();
            ImGui.BeginChild("RightLegit");
            RenderCategoryHeader("Bullet Trail");
            RenderBoolSetting("Enable Bullet Trail", ref BulletTrailManager.EnableBulletTrails);

            if (BulletTrailManager.EnableBulletTrails)
            {
                RenderBoolSetting("Use Real Bullet Impact", ref BulletTrailManager.UseRealBulletImpact);


                RenderColorSetting("Start Color", ref BulletTrailManager.StartColor);
                RenderColorSetting("End Color", ref BulletTrailManager.EndColor);
                RenderFloatSlider("Trail Opacity", ref BulletTrailManager.TrailOpacity, 0.1f, 1.0f, "%.2f");
                RenderFloatSlider("Trail Thickness", ref BulletTrailManager.TrailThickness, 1.0f, 5.0f, "%.1f px");
                RenderFloatSlider("Trail Lifetime", ref BulletTrailManager.TrailLifetime, 0.5f, 7.0f, "%.2f s");
                RenderFloatSlider("Trail Length", ref BulletTrailManager.TrailLength, 500f, 3000f, "%.0f");
                RenderIntSlider("Max Trails", ref BulletTrailManager.MaxTrails, 10, 50);
                RenderIntSlider("Trail Segments", ref BulletTrailManager.TrailSegments, 8, 32);

                RenderBoolSetting("Show Glow Effect", ref BulletTrailManager.ShowGlowEffect);
                RenderFloatSlider("Glow Intensity", ref BulletTrailManager.GlowIntensity, 0.1f, 1.0f, "%.2f");
                RenderFloatSlider("Spark Size", ref BulletTrailManager.SparkSize, 0.0f, 5.0f, "%.1f");


                ImGui.Text($"Active Trails: {BulletTrailManager.GetActiveTrailCount()}");

            }
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            RenderCategoryHeader("Spectator List");
            RenderBoolSetting("Enable Spectator List", ref SpectatorList.Enabled);

            if (SpectatorList.Enabled)
            {

                RenderSettingsSection("Style Settings", () =>
                {
                    RenderColorSetting("Window Background", ref SpectatorList.WindowBgColor);
           
                    RenderColorSetting("Border Color", ref SpectatorList.BorderColor);
                    RenderColorSetting("Text Color", ref SpectatorList.TextColor);
                    RenderColorSetting("Accent Color", ref SpectatorList.AccentColor);
                });

                RenderSettingsSection("Size Settings", () =>
                {
                    RenderFloatSlider("Width", ref SpectatorList.Width, 150f, 300f, "%.0f px");
                    RenderFloatSlider("Item Height", ref SpectatorList.ItemHeight, 18f, 30f, "%.0f px");
                   
                    RenderFloatSlider("Rounding", ref SpectatorList.Rounding, 0f, 12f, "%.0f");
                });

                RenderSettingsSection("Position", () =>
                {
                    RenderFloatSlider("Position X", ref SpectatorList.Position.X, 0f, GameState.renderer.screenSize.X - 200, "%.0f");
                    RenderFloatSlider("Position Y", ref SpectatorList.Position.Y, 0f, GameState.renderer.screenSize.Y - 200, "%.0f");
                });
            }

            // W RenderVisualsTab() dodaj:

            ImGui.EndChild();
            ImGui.Columns(1);
        }

        private void RenderSkinTab()
        {
            ImGui.Columns(2, "Skins Columns", true);

            // LEFT COLUMN
            ImGui.BeginChild("SkinLeft");
            {
                RenderBoolSettingWithWarning("Skin Changer", ref Modules.Visual.SkinChanger.Enabled);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                // MODEL CHANGER
                RenderCategoryHeader("Player Model Changer");
                ModelChanger.RenderMenu();
            }
            ImGui.EndChild();

            ImGui.NextColumn();

            // RIGHT COLUMN - Preview
            ImGui.BeginChild("RightSkin");
            {
                if (ModelChanger.ShowPreview && ModelChanger.SelectedModelIndex >= 0)
                {
                    ModelChanger.RenderPreview();
                }
                else
                {
                    ImGui.TextColored(new Vector4(0.7f, 0.7f, 0.7f, 1), "Select a model to see preview");
                }
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private void RenderRageTab()
        {
            ImGui.Columns(2, "TriggerColumns", true);
            ImGui.BeginChild("LeftAim");
            RenderCategoryHeader("AimBot");
            RenderBoolSetting("Enable Aimbot", ref Modules.Rage.Aimbot.AimbotEnable);

            RenderSettingsSection("Aimbot Settings", () =>
            {
                RenderIntCombo("Aim Bone", ref Modules.Rage.Aimbot.CurrentBone, Modules.Rage.Aimbot.Bones, Modules.Rage.Aimbot.Bones.Length, 160f);
                RenderIntCombo("Aim Method", ref Modules.Rage.Aimbot.CurrentAimMethod, Modules.Rage.Aimbot.AimbotMethods, Modules.Rage.Aimbot.AimbotMethods.Length);
                RenderKeybindChooser("Aimbot Keybind", ref Modules.Rage.Aimbot.AimbotKey);
                RenderBoolSetting("Aim On Team", ref Modules.Rage.Aimbot.Team);
                RenderFloatSlider("Smoothing X", ref Modules.Rage.Aimbot.SmoothingX, 0, 20, "%.2f");
                RenderFloatSlider("Smoothing Y", ref Modules.Rage.Aimbot.SmoothingY, 0, 20, "%.2f");
                RenderBoolSetting("Draw FOV", ref Modules.Rage.Aimbot.DrawFov);
                RenderBoolSetting("Use FOV", ref Modules.Rage.Aimbot.UseFOV);
                RenderBoolSetting("Scoped Check", ref Modules.Rage.Aimbot.ScopedOnly);
                RenderIntSlider("FOV Size", ref Modules.Rage.Aimbot.FovSize, 10, 1000, "%d");
                RenderColorSetting("FOV Color", ref Modules.Rage.Aimbot.FovColor);
                RenderBoolSetting("Visibility Check", ref Modules.Rage.Aimbot.VisibilityCheck);
                RenderBoolSetting("Target Line", ref Modules.Rage.Aimbot.targetLine);

            });

            RenderCategoryHeader("RCS");
            RenderBoolSetting("RCS", ref RCS.enabled);
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("RightRage");
            RenderCategoryHeader("Trigger Bot");
            RenderBoolSetting("Triggerbot", ref Modules.Rage.TriggerBot.Enabled);
            RenderBoolSetting("Team Check", ref Modules.Rage.TriggerBot.TeamCheck);
            RenderKeybindChooser($"Trigger Bot Keybind", ref TriggerBot.TriggerKey);
            RenderIntSlider("Max Delay", ref Modules.Rage.TriggerBot.MaxDelay, 0, 1000, "%d");
            RenderIntSlider("Min Delay", ref Modules.Rage.TriggerBot.MinDelay, 0, 1000, "%d");
            RenderBoolSetting("Require Key bind", ref Modules.Rage.TriggerBot.RequireKeybind);




            ImGui.EndChild();

            ImGui.Columns(1);
        }

        private void RenderHvHTab()
        {
            ImGui.Columns(2, "HvH", true);
            ImGui.BeginChild("Silent Aim");
            RenderCategoryHeader("Silent Aim");
            RenderBoolSetting("Enable Silent aim", ref Modules.HvH.SilentAimManager.Enabled);
            if (Modules.HvH.SilentAimManager.Enabled)
            {
                RenderSettingsSection("SIlent AIm Settings", () =>
                {
                    RenderBoolSetting("team Check", ref Modules.HvH.SilentAimManager.TeamCheck);
                    RenderBoolSetting("Visibility Check", ref Modules.HvH.SilentAimManager.VisibilityCheck);

                });
            }
            RenderCategoryHeader("SpinBot");
            RenderBoolSetting("Enable SpinBot", ref Modules.HvH.SpinBot.SpinbotEnabled);
            if (Modules.HvH.SpinBot.SpinbotEnabled)
            {
                RenderSettingsSection("SpinBot Settings", () =>
                {
                    RenderIntCombo("SpinBot Method", ref Modules.HvH.SpinBot.currentSpinbotMode, Modules.HvH.SpinBot.SpinbotModes, Modules.HvH.SpinBot.SpinbotModes.Length);
                    RenderFloatSlider("Spin Speed", ref Modules.HvH.SpinBot.SpinbotSpeed, 0f, 100f);
                    RenderBoolSetting("Enable X", ref Modules.HvH.SpinBot.EnableX);
                    RenderBoolSetting("Enable Y", ref Modules.HvH.SpinBot.EnableY);
                    RenderBoolSetting("Enable Z", ref Modules.HvH.SpinBot.EnableZ);
                    RenderBoolSetting("Movment Fix", ref Modules.HvH.SpinBot.MovementFix);
                    RenderFloatSlider("Jiggle Intensity", ref Modules.HvH.SpinBot.JiggleIntensity, 0f, 100f);
                    RenderFloatSlider("Jiggle Frequency", ref Modules.HvH.SpinBot.JiggleFrequency, 0f, 100f);
                    RenderFloatSlider("Random Min", ref Modules.HvH.SpinBot.RandomMin, -360f, 360f);
                    RenderFloatSlider("Random Max", ref Modules.HvH.SpinBot.RandomMax, -360f, 360f);
                    RenderBoolSetting("Anti Aim", ref Modules.HvH.SpinBot.AntiAimEnabled);
                    RenderFloatSlider("Custom Angle X", ref Modules.HvH.SpinBot.CustomAngleX, -360f, 360f);
                    RenderFloatSlider("Custom Angle Y", ref Modules.HvH.SpinBot.CustomAngleY, -360f, 360f);
                    RenderFloatSlider("Custom Angle Z", ref Modules.HvH.SpinBot.CustomAngleZ, -360f, 360f);
                    RenderFloatSlider("Pitch Angle", ref Modules.HvH.SpinBot.PitchAngle, -89f, 89f);
                    RenderFloatSlider("Yaw Angle", ref Modules.HvH.SpinBot.YawAngle, 0f, 360f);
                });
            }
            ImGui.EndChild();

        }

        private void RenderVisualsTab()
        {
            ImGui.Columns(2, "VisualsColumns", true);

            ImGui.BeginChild("LeftVisuals");
            RenderBoolSetting("Enable ESP", ref BoxESP.EnableESP);
            RenderIntCombo("ESP Shape", ref BoxESP.CurrentShape, BoxESP.Shapes, BoxESP.Shapes.Length);
            RenderBoolSetting("Team Check", ref BoxESP.TeamCheck);
            RenderBoolSetting("Enable RGB", ref Colors.RGB);
            RenderBoolSettingWith2ColorPickers("Box Fill Gradient", ref BoxESP.BoxFillGradient, ref BoxESP.BoxFillGradientColorTop, ref BoxESP.BoxFillGradientBottom);
            RenderFloatSlider("Box Fill Opacity", ref BoxESP.BoxFillOpacity, 0.0f, 1.0f, "%.2f");
            RenderBoolSettingWith1ColorPicker("Inner Outline", ref BoxESP.InnerOutline, ref BoxESP.InnerOutlineColor);
            RenderFloatSlider("ESP Rounding", ref BoxESP.Rounding, 1f, 5f);
            RenderFloatSlider("ESP Glow", ref BoxESP.GlowAmount, 0f, 5f);
            RenderColorSetting("Team Color", ref Colors.TeamColor);
            RenderColorSetting("Enemy Color", ref Colors.EnemyColor);
            RenderBoolSettingWith2ColorPickers("Outer Outline", ref BoxESP.OuterOutline, ref BoxESP.OutlineEnemyColor, ref BoxESP.OutlineTeamColor);
            RenderBoolSetting("Flash Check", ref BoxESP.FlashCheck);
            RenderBoolSetting("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
            RenderBoolSetting("Enable Armor Bar", ref ArmorBar.EnableArmorhBar);
            RenderBoolSetting("Show Distance Text", ref BoxESP.EnableDistanceTracker);
            RenderBoolSetting("Enable Tracers", ref Tracers.enableTracers);
            RenderIntCombo("Tracer Start Position", ref Tracers.CurrentStartPos, Tracers.StartPositions, Tracers.StartPositions.Length);
            RenderIntCombo("Tracer End Position", ref Tracers.CurrentEndPos, Tracers.EndPositions, Tracers.EndPositions.Length);
            RenderFloatSlider("Tracer Thickness", ref Tracers.LineThickness, 0.05f, 5f);
            RenderBoolSetting("Show Name", ref NameDisplay.Enabled);
            RenderBoolSetting("Enable Bone ESP", ref Modules.Visual.BoneESP.EnableBoneESP);
            RenderIntCombo("Bone ESP Type", ref BoneESP.CurrentType, BoneESP.Types, BoneESP.Types.Length);
            RenderBoolSetting("Team Check", ref BoneESP.TeamCheck);
            RenderColorSetting("Bone Color", ref BoneESP.BoneColor);
            RenderBoolSetting("Enable RGB", ref Colors.RGB);
            RenderFloatSlider("Bone Glow", ref BoneESP.GlowAmount, 0, 1f);
            RenderBoolSetting("Eye Ray", ref EyeRay.Enabled);
            RenderBoolSettingWith1ColorPicker("Gun Icon", ref GunDisplay.Enabled, ref GunDisplay.TextColor);
            RenderBoolSetting("SoundEsp", ref Modules.Visual.SoundESP.enabled);
            RenderColorSetting("Sound Esp Color", ref Modules.Visual.SoundESP.color);
            RenderBoolSetting("Arrow ESP", ref Modules.Visual.ArrowESP.Enabled);

            // Jeśli ArrowESP jest włączony, pokazuj jego ustawienia
            if (Modules.Visual.ArrowESP.Enabled)
            {
                ImGui.Indent(16f); // Wcięcie dla pod-ustawień

                RenderBoolSetting("Show Team", ref Modules.Visual.ArrowESP.ShowTeam);
                RenderBoolSetting("Show Distance", ref Modules.Visual.ArrowESP.ShowDistanceText);

                if (Modules.Visual.ArrowESP.ShowDistanceText)
                {
                    RenderBoolSetting("Always Show Distance", ref Modules.Visual.ArrowESP.AlwaysShowDistance);
                }

                RenderBoolSetting("Enable Glow", ref Modules.Visual.ArrowESP.EnableGlow);

                if (Modules.Visual.ArrowESP.EnableGlow)
                {
                    RenderFloatSlider("Glow Amount", ref Modules.Visual.ArrowESP.GlowAmount, 0f, 20f, "%.1f");
                    RenderColorSetting("Glow Color", ref Modules.Visual.ArrowESP.GlowColor);
                }

                RenderFloatSlider("Arrow Size", ref Modules.Visual.ArrowESP.ArrowSize, 10f, 50f, "%.1f px");
                RenderBoolSetting("Dynamic Size", ref Modules.Visual.ArrowESP.UseDynamicSize);

                if (Modules.Visual.ArrowESP.UseDynamicSize)
                {
                    RenderFloatSlider("Min Size", ref Modules.Visual.ArrowESP.MinArrowSize, 5f, 30f, "%.1f px");
                    RenderFloatSlider("Max Size", ref Modules.Visual.ArrowESP.MaxArrowSize, 20f, 60f, "%.1f px");
                }

                RenderFloatSlider("Arrow Distance", ref Modules.Visual.ArrowESP.ArrowDistance, 50f, 200f, "%.0f px");
                RenderFloatSlider("Min Distance", ref Modules.Visual.ArrowESP.MinDistanceToShow, 0f, 500f, "%.0f");
                RenderFloatSlider("Max Distance", ref Modules.Visual.ArrowESP.MaxArrowDistance, 100f, 2000f, "%.0f");
                RenderFloatSlider("Arrow Thickness", ref Modules.Visual.ArrowESP.ArrowThickness, 1f, 5f, "%.1f px");
                RenderBoolSetting("Show Health Bar", ref Modules.Visual.ArrowESP.ShowHealthBar);

                if (Modules.Visual.ArrowESP.ShowHealthBar)
                {
                    RenderFloatSlider("Health Bar Height", ref Modules.Visual.ArrowESP.HealthBarHeight, 1f, 10f, "%.1f px");
                }

                RenderBoolSetting("Show Name", ref Modules.Visual.ArrowESP.ShowName);
                RenderBoolSetting("Fade By Distance", ref Modules.Visual.ArrowESP.FadeByDistance);
                RenderBoolSetting("Show Out Of View", ref Modules.Visual.ArrowESP.ShowOutOfView);

                RenderColorSetting("Team Arrow Color", ref Modules.Visual.ArrowESP.TeamArrowColor);
                RenderColorSetting("Enemy Arrow Color", ref Modules.Visual.ArrowESP.EnemyArrowColor);

                ImGui.Unindent(16f);
            }
            RenderCategoryHeader("Chams");
            RenderBoolSetting("Enable Chams", ref Modules.Visual.Chams.EnableChams);
            if (Modules.Visual.Chams.EnableChams)
            {
                RenderSettingsSection("Chams Settings", () =>
                {
                    RenderBoolSetting("Draw On Self", ref Chams.DrawOnSelf);
                    RenderBoolSetting("Draw On Self", ref Modules.Visual.Chams.DrawOnSelf);
                    RenderFloatSlider("Bone Thickness", ref Chams.BoneThickness, 1f, 20f, "%.1f");
                    RenderBoolSetting("Enable RGB", ref Colors.RGB);
                });
            }

            ImGui.EndChild();

            ImGui.NextColumn();
            ImGui.BeginChild("RightVisuals", ImGui.GetContentRegionAvail());

            float PreviewHeight = ImGui.GetContentRegionAvail().Y * 0.5f;
            ImGui.BeginChild("ESPPreviewSection", new(0, PreviewHeight));
            RenderCategoryHeader("ESP Preview");
            float Offset = -30f;
            Vector2 previewCenter = ImGui.GetCursorScreenPos() + new Vector2(ImGui.GetContentRegionAvail().X / 2, PreviewHeight / 2 + Offset);
            BoxESP.RenderESPPreview(previewCenter);
            ImGui.EndChild();

            ImGui.BeginChild("EtraVisuals", new(0, 0));
            RenderCategoryHeader("Other Visuals");
            RenderBoolSetting("Enable Bomb Timer", ref Modules.Visual.BombTimerOverlay.EnableTimeOverlay);
            RenderBoolSetting("Anti Flash", ref Modules.Visual.NoFlash.NoFlashEnable);
            RenderBoolSetting("FOV Changer", ref FovChanger.Enabled);
            RenderIntSlider("Desired FOV", ref FovChanger.FOV, 60, 160);
            RenderBoolSettingWith2ColorPickers("Radar", ref Radar.IsEnabled, ref Radar.EnemyPointColor, ref Radar.TeamPointColor);
            RenderBoolSetting("Draw Team", ref Radar.DrawOnTeam);
            RenderBoolSetting("Draw Cross", ref Radar.DrawCrossb);
            RenderBoolSettingWith1ColorPicker("C4 Box ESP", ref C4ESP.BoxEnabled, ref C4ESP.BoxColor);
            RenderBoolSettingWith1ColorPicker("C4 Text ESP", ref C4ESP.TextEnabled, ref C4ESP.TextColor);
            RenderBoolSettingWith1ColorPicker("Dropped Weapon ESP", ref WorldESP.DroppedWeaponESP, ref WorldESP.WeaponTextColor);
            RenderBoolSettingWith2ColorPickers("Dropped Hostage ESP", ref WorldESP.HostageESP, ref WorldESP.HostageTextColor, ref WorldESP.HostageBoxColor);
            RenderBoolSettingWith2ColorPickers("Chicken ESP", ref WorldESP.ChickenESP, ref WorldESP.ChickenTextColor, ref WorldESP.ChickenBoxColor);
            RenderBoolSettingWith1ColorPicker("Projectile ESP", ref WorldESP.ProjectileESP, ref WorldESP.ProjectileTextColor);
            RenderCategoryHeader("Smoke");
            RenderBoolSetting("Remove Smoke", ref SmokeManager.AntiSmokeEnabled);
            RenderBoolSetting("Smoke color", ref SmokeManager.SmokeColorChangerEnabled);
            

            ImGui.EndChild();

            ImGui.EndChild();
            ImGui.Columns(1);
        }

        private void RenderConfigTab()
        {
            ImGui.Columns(2, "ConfigColumn", true);

            // --- LEWA KOLUMNA ---
            ImGui.BeginChild("ConfigLeft");
            RenderCategoryHeader("Available Configs");

            ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1.0f);
            ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.2f, 0.2f, 0.2f, 1.0f));
            ImGui.BeginChild("ConfigList", new Vector2(0, 350));
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();

            {
                float spacing = 8f;

                foreach (var config in Configs.SavedConfigs.Keys)
                {
                    bool isSelected = Configs.SelectedConfig == config;

                    // ODSTĘP (oprócz pierwszego)
                    if (Configs.SavedConfigs.Keys.First() != config)
                    {
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + spacing);
                    }

                    // ROZMIAR PROSTOKĄTA
                    Vector2 rectSize = new Vector2(ImGui.GetContentRegionAvail().X - 10, 55);
                    Vector2 cursorPos = ImGui.GetCursorScreenPos();

                    // TWORZYMY CHILD WINDOW DLA KAŻDEGO CONFIGU (to naprawia klikalność)
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 5.0f);
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 0));
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

                    // TŁO CHILD WINDOW
                    Vector4 bgColor = isSelected ?
                        new Vector4(currentAccentColor.X * 0.3f, currentAccentColor.Y * 0.3f, currentAccentColor.Z * 0.3f, 0.5f) :
                        new Vector4(0.15f, 0.17f, 0.20f, 0.5f);

                    ImGui.PushStyleColor(ImGuiCol.ChildBg, bgColor);
                    ImGui.PushStyleColor(ImGuiCol.Border, new Vector4(0.1f, 0.1f, 0.1f, 0.8f));

                    ImGui.BeginChild($"##ConfigChild_{config}", rectSize,
                        ImGuiChildFlags.Border | ImGuiChildFlags.None,
                        ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);

                    ImGui.PopStyleColor(2);
                    ImGui.PopStyleVar(3);

                    // NAZWA NA GÓRZE (wyśrodkowana)
                    Vector2 textSize = ImGui.CalcTextSize(config);
                    float textX = (rectSize.X - textSize.X) / 2;
                    ImGui.SetCursorPosX(textX);
                    ImGui.SetCursorPosY(6);
                    ImGui.Text(config);

                    // PRZYCISKI NA DOLE
                    ImGui.SetCursorPosY(28);

                    float buttonWidth = (rectSize.X - 30) / 4;

                    // PRZYCISK LOAD
                    if (ImGui.Button("Load##" + config, new Vector2(buttonWidth, 22)))
                    {
                        Configs.LoadConfig(config);
                        Configs.SelectedConfig = config;
                        NotificationManager.AddNotification($"Loaded: {config}", NotificationType.Success);
                    }
                    ImGui.PopID();

                    ImGui.SameLine();

                    // PRZYCISK SAVE
                    ImGui.PushID($"Save_{config}");
                    if (ImGui.Button("Save", new Vector2(buttonWidth, 22)))
                    {
                        Configs.SaveConfig(config);
                        NotificationManager.AddNotification($"Saved: {config}", NotificationType.Success);
                    }
                    ImGui.PopID();

                    ImGui.SameLine();

                    // PRZYCISK EDIT
                    ImGui.PushID($"Edit_{config}");
                    if (ImGui.Button("Edit", new Vector2(buttonWidth, 22)))
                    {
                        Configs.EditingConfig = config;
                        Configs.EditConfigName = config.Replace(".json", "");
                        Configs.ShowEditPopup = true;
                    }
                    ImGui.PopID();

                    ImGui.SameLine();

                    // PRZYCISK DELETE
                    ImGui.PushID($"Delete_{config}");
                    if (ImGui.Button("Del", new Vector2(buttonWidth, 22)))
                    {
                        Configs.ConfigToDelete = config;
                        Configs.ShowDeletePopup = true;
                    }
                    ImGui.PopID();

                    ImGui.EndChild();

                    // KLIKNIĘCIE CAŁEGO PROSTOKĄTA (oprócz przycisków)
                    if (ImGui.IsItemHovered() && ImGui.IsMouseClicked(ImGuiMouseButton.Left))
                    {
                        // Sprawdzamy czy nie kliknięto w przycisk
                        bool clickedOnButton = false;
                        Vector2 mousePos = ImGui.GetMousePos();

                        // Sprawdzamy pozycje przycisków
                        float buttonY = cursorPos.Y + 28;
                        if (mousePos.Y >= buttonY && mousePos.Y <= buttonY + 22)
                        {
                            clickedOnButton = true;
                        }

                        if (!clickedOnButton)
                        {
                            Configs.SelectedConfig = config;
                        }
                    }
                }
            }
            ImGui.EndChild();

            ImGui.Spacing();
            ImGui.EndChild();

            // --- PRAWA KOLUMNA ---
            ImGui.NextColumn();

            ImGui.BeginChild("ConfigRight");
            RenderCategoryHeader("Config Management");

            if (ImGui.Button("Create New Config", new Vector2(ImGui.GetContentRegionAvail().X, 35)))
            {
                Configs.ShowCreatePopup = true;
                Configs.NewConfigName = $"config_{DateTime.Now:yyyyMMdd_HHmmss}";
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Load Shared Config", new Vector2(ImGui.GetContentRegionAvail().X, 35)))
            {
                Configs.ShowLoadSharedPopup = true;
                Configs.ShareCodeInput = "";
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (!string.IsNullOrEmpty(Configs.SelectedConfig))
            {
                ImGui.Text($"Selected: {Configs.SelectedConfig}");

                ImGui.Spacing();

                if (ImGui.Button("Share This Config", new Vector2(ImGui.GetContentRegionAvail().X, 35)))
                {
                    Configs.ShowSharePopup = true;
                    Configs.ShareConfigName = Configs.SelectedConfig;
                }
            }
            else
            {
                ImGui.TextWrapped("Select a config from the list");
            }

            ImGui.EndChild();

            ImGui.Columns(1);

            // --- POP-UP'y ---
            RenderConfigPopups();
        }

        private void RenderConfigPopups()
        {
            // 1. POP-UP: Create New Config
            if (Configs.ShowCreatePopup)
            {
                ImGui.SetNextWindowSize(new Vector2(400, 150));
                ImGui.SetNextWindowPos(screenSize / 2 - new Vector2(200, 75));

                ImGui.Begin("Create New Config", ref Configs.ShowCreatePopup,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                ImGui.Text("Config Name:");
                ImGui.InputText("##NewConfigName", ref Configs.NewConfigName, 50);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Columns(2, "create_buttons", false);

                if (ImGui.Button("Cancel", new Vector2(120, 30)))
                {
                    Configs.ShowCreatePopup = false;
                }

                ImGui.NextColumn();

                if (ImGui.Button("Create", new Vector2(120, 30)))
                {
                    if (!string.IsNullOrWhiteSpace(Configs.NewConfigName))
                    {
                        string configName = Configs.NewConfigName;
                        if (!configName.EndsWith(".json"))
                            configName += ".json";

                        Configs.SaveConfig(configName);
                        Configs.SavedConfigs.TryAdd(configName, true);
                        Configs.SelectedConfig = configName;
                        Configs.LastMessage = $"Created: {configName}";
                        Configs.ShowCreatePopup = false;
                    }
                }

                ImGui.Columns(1);

                ImGui.End();
            }

            // 2. POP-UP: Edit Config Name
            if (Configs.ShowEditPopup && !string.IsNullOrEmpty(Configs.EditingConfig))
            {
                ImGui.SetNextWindowSize(new Vector2(400, 150));
                ImGui.SetNextWindowPos(screenSize / 2 - new Vector2(200, 75));

                ImGui.Begin("Edit Config Name", ref Configs.ShowEditPopup,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                ImGui.Text($"Current: {Configs.EditingConfig}");
                ImGui.Text("New Name:");
                ImGui.InputText("##EditConfigName", ref Configs.EditConfigName, 50);

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Columns(2, "edit_buttons", false);

                if (ImGui.Button("Cancel", new Vector2(120, 30)))
                {
                    Configs.ShowEditPopup = false;
                    Configs.EditingConfig = null;
                }

                ImGui.NextColumn();

                if (ImGui.Button("Rename", new Vector2(120, 30)))
                {
                    if (!string.IsNullOrWhiteSpace(Configs.EditConfigName))
                    {
                        string newName = Configs.EditConfigName;
                        if (!newName.EndsWith(".json"))
                            newName += ".json";

                        Configs.RenameConfig(Configs.EditingConfig, newName);
                        Configs.LastMessage = $"Renamed: {Configs.EditingConfig} -> {newName}";
                        Configs.ShowEditPopup = false;
                        Configs.EditingConfig = null;
                    }
                }

                ImGui.Columns(1);

                ImGui.End();
            }

            // 3. POP-UP: Load Shared Config
            if (Configs.ShowLoadSharedPopup)
            {
                ImGui.SetNextWindowSize(new Vector2(400, 180));
                ImGui.SetNextWindowPos(screenSize / 2 - new Vector2(200, 90));

                ImGui.Begin("Load Shared Config", ref Configs.ShowLoadSharedPopup,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                ImGui.Text("Enter 9-digit share code:");
                ImGui.InputText("##ShareCodeInputPopup", ref Configs.ShareCodeInput, 50);

                ImGui.Spacing();

                if (ImGui.Button("Paste", new Vector2(80, 25)))
                {
                    Configs.ShareCodeInput = ImGui.GetClipboardText();
                }

                ImGui.SameLine();
                ImGui.Text("(Ctrl+V)");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Columns(2, "load_shared_buttons", false);

                if (ImGui.Button("Cancel", new Vector2(120, 30)))
                {
                    Configs.ShowLoadSharedPopup = false;
                }

                ImGui.NextColumn();

                if (ImGui.Button("Load", new Vector2(120, 30)))
                {
                    if (!string.IsNullOrEmpty(Configs.ShareCodeInput) && Configs.ShareCodeInput.Length == 9)
                    {
                        bool success = Configs.LoadSharedConfig(Configs.ShareCodeInput);
                        if (success)
                        {
                            Configs.LastMessage = "Config loaded successfully!";
                            Configs.ShowLoadSharedPopup = false;
                        }
                        else
                        {
                            Configs.LastMessage = "Invalid share code!";
                        }
                    }
                }

                ImGui.Columns(1);

                ImGui.End();
            }

            // 4. POP-UP: Share Config
            if (Configs.ShowSharePopup && !string.IsNullOrEmpty(Configs.ShareConfigName))
            {
                ImGui.SetNextWindowSize(new Vector2(400, 200));
                ImGui.SetNextWindowPos(screenSize / 2 - new Vector2(200, 100));

                ImGui.Begin("Share Config", ref Configs.ShowSharePopup,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                ImGui.Text($"Sharing: {Configs.ShareConfigName}");

                ImGui.Spacing();

                if (string.IsNullOrEmpty(Configs.GeneratedShareCode))
                {
                    if (ImGui.Button("Generate Share Code", new Vector2(ImGui.GetContentRegionAvail().X, 40)))
                    {
                        string shareCode = Configs.ShareConfig(Configs.ShareConfigName);
                        if (!shareCode.StartsWith("Error"))
                        {
                            Configs.GeneratedShareCode = shareCode;
                            Configs.LastMessage = "Share code generated!";
                        }
                        else
                        {
                            Configs.LastMessage = shareCode;
                        }
                    }
                }
                else
                {
                    ImGui.Text("Share Code:");
                    ImGui.InputText("##GeneratedCode", ref Configs.GeneratedShareCode, 50, ImGuiInputTextFlags.ReadOnly);

                    ImGui.Spacing();

                    if (ImGui.Button("Copy to Clipboard", new Vector2(ImGui.GetContentRegionAvail().X, 30)))
                    {
                        ImGui.SetClipboardText(Configs.GeneratedShareCode);
                        Configs.LastMessage = "Copied to clipboard!";
                    }
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                if (ImGui.Button("Close", new Vector2(ImGui.GetContentRegionAvail().X, 30)))
                {
                    Configs.ShowSharePopup = false;
                    Configs.GeneratedShareCode = "";
                }

                ImGui.End();
            }

            // 5. POP-UP: Delete Confirmation
            if (Configs.ShowDeletePopup && !string.IsNullOrEmpty(Configs.ConfigToDelete))
            {
                ImGui.SetNextWindowSize(new Vector2(350, 150));
                ImGui.SetNextWindowPos(screenSize / 2 - new Vector2(175, 75));

                ImGui.Begin("Delete Config", ref Configs.ShowDeletePopup,
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                ImGui.TextWrapped($"Delete config:\n{Configs.ConfigToDelete}?");

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                ImGui.Columns(2, "delete_buttons", false);

                if (ImGui.Button("Cancel", new Vector2(120, 30)))
                {
                    Configs.ShowDeletePopup = false;
                    Configs.ConfigToDelete = "";
                }

                ImGui.NextColumn();

                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.9f, 0.3f, 0.3f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(1.0f, 0.4f, 0.4f, 1f));

                if (ImGui.Button("DELETE", new Vector2(120, 30)))
                {
                    Configs.DeleteConfig(Configs.ConfigToDelete);
                    if (Configs.SelectedConfig == Configs.ConfigToDelete)
                    {
                        Configs.SelectedConfig = "";
                    }
                    Configs.LastMessage = $"Deleted: {Configs.ConfigToDelete}";
                    Configs.ShowDeletePopup = false;
                    Configs.ConfigToDelete = "";
                }

                ImGui.PopStyleColor(3);

                ImGui.Columns(1);

                ImGui.End();
            }
        }



        private void RenderSettingsTab()
        {
            ImGui.Columns(2, "SettingsColumn", true);

            ImGui.BeginChild("LeftColumn");

            RenderBoolSetting("Enable Watermark", ref EnableWaterMark);
            RenderCategoryHeader("GUI Settings");
            RenderFloatSlider("Window Alpha", ref windowAlpha, 0.1f, 1.0f, "%.2f");
            RenderColorSetting("Accent Color", ref accentColor);
            RenderColorSetting("Sidebar Color", ref SidebarColor);
            RenderColorSetting("Main Content Color", ref MainContentCol);
            RenderColorSetting("Text Color", ref TextCol);
            RenderFloatSlider("Animation Speed", ref animationSpeed, 0.01f, 1.0f, "%.2f");

            RenderCategoryHeader("FPS Settings");
            RenderBoolSetting("Enable FPS Limit", ref EnableFPSLimit);
            RenderIntSlider("Target FPS", ref TargetFPS, 30, 360, "%d");
            ImGui.Text($"Current FPS: {CurrentFPS}");

            RenderCategoryHeader("Particle Effects");
            RenderIntSlider("Particle Count", ref NumberOfParticles, 0, 200, "%d");
            RenderFloatSlider("Particle Speed", ref ParticleSpeed, 0, 10);
            RenderColorSetting("Particle Color", ref ParticleColor);
            RenderColorSetting("Line Color", ref LineColor);
            RenderFloatSlider("Particle Radius", ref ParticleRadius, 0.5f, 10f);
            RenderFloatSlider("Max Line Distance", ref MaxLineDistance, 50f, 500f);


            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();
            RenderBoolSetting("Auto Focus Management", ref isFocusManagementEnabled);
            ImGui.TextWrapped("Auto focus switches between CS2 and menu when pressing Insert");

            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("RightColumn");

            RenderCategoryHeader("Application");

            ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.12f, 0.12f, 0.14f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonHovered, new Vector4(0.18f, 0.18f, 0.20f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.ButtonActive, new Vector4(0.22f, 0.22f, 0.24f, 1.0f));
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.95f, 0.32f, 0.32f, 1.0f));

            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 8.0f);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(16, 12));

            if (ImGui.Button("EXIT APPLICATION", new Vector2(ImGui.GetContentRegionAvail().X, 45)))
            {
                
            }

            ImGui.PopStyleColor(3);

            ImGui.Spacing();
            ImGui.TextWrapped("Click to close Tutamaka completely.");
            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();


            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            RenderCategoryHeader("About");
            ImGui.Text($"Tutamaka GUI V{Configs.Version}");
            ImGui.Text("External Cheat.");
            ImGui.Spacing();
            ImGui.TextWrapped("Enhanced with smooth animations, music system, and customizable colors.");

            RenderCategoryHeader("Performance Tips");
            ImGui.TextWrapped("• Reduce particle count for better FPS");
            ImGui.TextWrapped("• Disable unnecessary visual effects");
            ImGui.TextWrapped("• Lower FPS limit if experiencing lag");
            ImGui.TextWrapped("• Close other applications for best performance");
            ImGui.TextWrapped("• Disable background music if needed");

            ImGui.Spacing();
            RenderCategoryHeader("Troubleshooting");
            ImGui.TextWrapped("• If fonts don't load, check Resources/fonts folder");
            ImGui.TextWrapped("• If focus doesn't switch, disable Auto Focus Management");
            ImGui.TextWrapped("• Press Insert to toggle menu");
            ImGui.TextWrapped("• Music files must be in Resources/music folder");
            ImGui.TextWrapped("• Supported formats: MP3, WAV");

            ImGui.EndChild();

            ImGui.Columns(1);
        }
        #endregion



        #region Particle System
        public void DrawParticles(int num)
        {
            if (num <= 0) return;

            while (Positions.Count < num || Velocities.Count < num)
            {
                Positions.Add(new Vector2(random.Next((int)screenSize.X), random.Next((int)screenSize.Y)));
                Velocities.Add(new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)));
            }

            for (int i = 0; i < num; i++)
            {
                Positions[i] += Velocities[i] * ParticleSpeed;

                if (Positions[i].X < 0 || Positions[i].X > screenSize.X || Positions[i].Y < 0 || Positions[i].Y > screenSize.Y)
                {
                    Positions[i] = new Vector2(random.Next((int)screenSize.X), random.Next((int)screenSize.Y));
                    Velocities[i] = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1));
                }

                DrawHelpers.DrawGlowCircleFilled(BGdrawList, Positions[i], ParticleRadius, ParticleColor, 1.1f);
            }

            if (num <= 100)
            {
                for (int i = 0; i < num; i++)
                {
                    for (int j = i + 1; j < num; j++)
                    {
                        float dist = Vector2.Distance(Positions[i], Positions[j]);
                        if (dist < MaxLineDistance)
                        {
                            float alpha = 1f - (dist / MaxLineDistance);
                            BGdrawList.AddLine(Positions[i], Positions[j], ImGui.ColorConvertFloat4ToU32(new Vector4(LineColor.X, LineColor.Y, LineColor.Z, LineColor.W * alpha)), 1f);
                        }
                    }
                }
            }
        }
        #endregion

        #region Module Execution
        public void RunAllModules()
        {
            try
            {

                HitStuff.CreateHitText();

                if (EyeRay.Enabled)
                    EyeRay.DrawEyeRay();

                if (NameDisplay.Enabled)
                {
                    foreach (var e in entities)
                    {
                        NameDisplay.DrawName(e, this);
                    }
                }
                if (Aimbot.DrawFov && Aimbot.AimbotEnable && Aimbot.UseFOV)
                    Aimbot.DrawCircle(Aimbot.FovSize, Aimbot.FovColor);

                if (Modules.Visual.BoneESP.EnableBoneESP)
                {
                    foreach (Data.Entity.Entity entity in entities)
                    {
                        if ((!BoneESP.TeamCheck || entity.Team == localPlayer.Team) && entity != localPlayer)
                        {
                            Modules.Visual.BoneESP.DrawBoneLines(entity, this);
                        }
                    }
                }
                C4ESP.DrawESP();
                if (Chams.EnableChams)
                {
                    foreach (Entity entity in entities)
                    {
                        Chams.DrawChams(entity);
                    }
                }
                foreach (var e in GameState.Entities)
                {
                    GunDisplay.Draw(e);
                }
                WorldESP.EntityESP();
                Radar.DrawRadar();

                if (Modules.Visual.BoxESP.EnableESP)
                {
                    foreach (var entity in entities)
                    {
                        BoxESP.DrawBoxESP(entity, localPlayer, this);
                    }
                }
                if (BoxESP.EnableDistanceTracker)
                {
                    foreach (var entity in entities)
                    {
                        DistanceTextThingy(entity);
                    }
                }
                if (Tracers.enableTracers)
                {
                    foreach (var entity in GameState.Entities)
                    {
                        if (!Modules.Visual.BoxESP.TeamCheck || (Modules.Visual.BoxESP.TeamCheck && entity.Team != GameState.LocalPlayer.Team))
                        {
                            Tracers.DrawTracers(entity, this);
                        }
                    }
                }

                // W głównym pliku gdzie masz pętlę (np. w Main.cs lub GameLoop.cs)
                SmokeManager.Update();

                BulletTrailManager.Render();

                if (SpectatorList.Enabled)
                {
                    SpectatorList.Update();
                    SpectatorList.Render();
                }

                if (ArrowESP.Enabled)
                {
                    ArrowESP.Render();
                }

                // W metodzie RunAllModules() dodaj na początku:
                if (Modules.Visual.SoundESP.enabled)
                {
                    foreach (var entity in GameState.Entities)
                    {
                        if (entity != null && entity.Health > 0)
                        {
                            Modules.Visual.SoundESP.DrawSoundESP(entity);
                        }
                    }
                }

                // Potem w pętli rysowania:
                if (Modules.Visual.SoundESP.enabled)
                {
                    foreach (var entity in GameState.Entities)
                    {
                        if (entity != null && entity.Health > 0)
                        {
                            Modules.Visual.SoundESP.DrawSoundESP(entity);
                        }
                    }
                }
                foreach (var entity in GameState.Entities)
                {
                    if (entity != null)
                    {
                        var rect = BoxESP.GetBoxRect(entity);
                        if (rect != null)
                        {
                            var (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle) = rect.Value;
                            Vector2 barTopLeft = new(topLeft.X - HealthBar.HealthBarWidth - 2, topLeft.Y);
                            float height = bottomRight.Y - topLeft.Y;

                            Modules.Visual.HealthBar.DrawHealthBar(entity.Health, 100, barTopLeft, height, entity);
                        }
                    }
                }
                if (ArmorBar.EnableArmorhBar)
                {
                    foreach (var entity in GameState.Entities)
                    {
                        if (entity != null)
                        {
                            var rect = BoxESP.GetBoxRect(entity);
                            if (rect != null)
                            {
                                var (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle) = rect.Value;
                                Vector2 barTopRight = new(topRight.X - HealthBar.HealthBarWidth + 8, topRight.Y);
                                float height = bottomRight.Y - topLeft.Y;

                                ArmorBar.DrawArmorBar(this, entity.Armor, 100, barTopRight, height, entity);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public static void DistanceTextThingy(Entity e)
        {
            if (e == null || (BoxESP.TeamCheck && e?.Team == GameState.LocalPlayer.Team) || e?.Health <= 0 || e?.PawnAddress == GameState.LocalPlayer.PawnAddress || e?.Distance == null || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed)) return;

            string distText = $"{(int)e.Distance / 100}m";
            Vector2 textPos = new(e.Position2D.X + 2, e.Position2D.Y);
            GameState.renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new(1f, 1f, 1f, 1f)), distText);
        }
        #endregion

        #region UI Helper Methods
        private static void DrawGearIcon(Vector2 center, uint color)
        {
            if (!IsIconFont1Loaded) return;

            ImGui.PushFont(IconFont1);
            Vector2 textSize = ImGui.CalcTextSize("\uEAF5");
            Vector2 textPos = new(center.X - textSize.X / 2, center.Y - textSize.Y / 2);
            ImGui.GetWindowDrawList().AddText(textPos, color, "\uEAF5");
            ImGui.PopFont();
        }

        public static void RenderTitle(string Text)
        {
            if (!IsTextFontBigLoaded) return;

            ImGui.PushFont(TextFontBig);
            Vector2 offsetPos = new(ImGui.GetCursorScreenPos().X + 4, ImGui.GetCursorScreenPos().Y + 2);
            ImGui.GetWindowDrawList().AddText(offsetPos, ImGui.ColorConvertFloat4ToU32(currentTextCol), Text);
            ImGui.PopFont();

            Vector2 textSize = ImGui.CalcTextSize(Text);
            ImGui.Dummy(new Vector2(0, textSize.Y + 8));

            Vector2 start = ImGui.GetCursorScreenPos();
            Vector2 end = new(start.X + ImGui.GetContentRegionAvail().X, start.Y);
            ImGui.GetWindowDrawList().AddLine(start, end, ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.17f, 0.20f, 1.0f)), 1.0f);

            ImGui.Dummy(new Vector2(0, 6));
        }

        private static void RenderCategoryHeader(string categoryName)
        {
            Vector2 textSize = ImGui.CalcTextSize(categoryName);
            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 childSize = ImGui.GetContentRegionAvail();

            Vector2 rectPos = new(cursorPos.X, cursorPos.Y);
            Vector2 rectSize = new(childSize.X / 2, textSize.Y + 8.3f);

            ImGui.GetWindowDrawList().AddRectFilledMultiColor(rectPos, rectPos + rectSize, ImGui.ColorConvertFloat4ToU32(currentTextCol), ImGui.ColorConvertFloat4ToU32(currentMainContentCol), ImGui.ColorConvertFloat4ToU32(currentMainContentCol), ImGui.ColorConvertFloat4ToU32(currentTextCol));

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1 / 2);

            if (Renderer.IsTextFontBigLoaded)
            {
                ImGui.PushFont(Renderer.TextFontBig);
                RenderGradientText(categoryName, new(0, 0, 0, 1f), new(0, 0, 0, 1f));
                ImGui.PopFont();
            }
            else
            {
                RenderGradientText(categoryName, new(1, 0, 0, 1), new(0, 1, 0, 1));
            }

            ImGui.Dummy(new Vector2(textSize.X, textSize.Y + 1));
            ImGui.Separator();
            ImGui.Spacing();
        }

        private static void RenderGradientText(string text, Vector4 startColor, Vector4 endColor)
        {
            var drawList = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetCursorScreenPos();
            float step = 1f / (text.Length - 1);

            for (int i = 0; i < text.Length; i++)
            {
                float t = i * step;
                Vector4 color = startColor + t * (endColor - startColor);
                drawList.AddText(pos, ImGui.ColorConvertFloat4ToU32(color), text[i].ToString());
                pos.X += ImGui.CalcTextSize(text[i].ToString()).X;
            }

            ImGui.Dummy(new Vector2(ImGui.CalcTextSize(text).X, 0));
        }

        private void RenderTabButton(string label, int tabIndex)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
            bool isSelected = selectedTab == tabIndex;

            if (IsIconFontLoaded)
            {
                ImGui.PushFont(IconFont);

                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                }
                ImGui.PopStyleColor(isSelected ? 4 : 1);
            }

            bool pressed;
            if (label == "\uEB54")
            {
                ImGui.PushFont(IconFont1);
                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                }
                pressed = ImGui.Button(label, new Vector2(tabSize.X, 40));
                if (pressed) selectedTab = tabIndex;
                ImGui.PopStyleColor(isSelected ? 4 : 1);
                ImGui.PopFont();
            }
            else
            {
                pressed = ImGui.Button(label, new Vector2(tabSize.X, 40));
                if (pressed) selectedTab = tabIndex;
            }

            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var borderColor = isSelected ? currentSidebarColor + new Vector4(0.02f, 0.01f, 0.01f, 1f) : currentSidebarColor;
            drawList.AddLine(new Vector2(min.X, min.Y), new Vector2(max.X, min.Y), ImGui.ColorConvertFloat4ToU32(borderColor), 1.5f);
            drawList.AddLine(new Vector2(min.X, max.Y), new Vector2(max.X, max.Y), ImGui.ColorConvertFloat4ToU32(borderColor), 1.5f);

            ImGui.PopFont();
            ImGui.PopStyleVar();
        }

        private static Dictionary<string, bool> openPopups = new Dictionary<string, bool>();

        private static Dictionary<string, bool> previousValues = new Dictionary<string, bool>();
        private static void RenderBoolSettingWithWarning(string label, ref bool value, Action? onChanged = null, float widgetWidth = 0f)
        {
            if (!openPopups.ContainsKey(label))
                openPopups[label] = false;

            if (!previousValues.ContainsKey(label))
                previousValues[label] = value;

            bool temp = value;
            RenderRowRightAligned(label, () =>
            {
                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;

                float colWidth = ImGui.GetColumnWidth();
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float posX = ImGui.GetCursorPosX() + colWidth - width - spacing;
                ImGui.SetCursorPosX(posX);

                Vector2 p = ImGui.GetCursorScreenPos();
                var drawList = ImGui.GetWindowDrawList();
                string strId = "##" + label;

                ImGui.InvisibleButton(strId, new Vector2(width, height));
                if (ImGui.IsItemClicked())
                {
                    temp = !temp;
                    if (!previousValues[label] && temp)
                        openPopups[label] = true;
                }

                float t = temp ? 1f : 0f;

                drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height),
                    ImGui.ColorConvertFloat4ToU32(trackCol), height);

                float knobX = p.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = p.Y + radius + 2f;

                Vector4 knobColor = temp ? knobOn : knobOff;

                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(knobColor), 36);

                drawList.AddCircle(new Vector2(knobX, knobY), radius,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);
            }, widgetWidth);

            string popupId = "warning##" + label;
            if (openPopups[label])
                ImGui.OpenPopup(popupId);
            var tempref = openPopups[label];
            ImGui.SetNextWindowPos(GameState.renderer.screenSize / 2);
            if (ImGui.BeginPopupModal(popupId, ref tempref, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove))
            {
                ImGui.Text("WARNING\nThis feature uses WPM and may be detected.\nUse at your own risk.");
                ImGui.Separator();
                if (ImGui.Button("OK", new Vector2(120, 0)))
                {
                    openPopups[label] = false;
                    ImGui.CloseCurrentPopup();
                }
                ImGui.EndPopup();
            }

            previousValues[label] = temp;

            if (temp != value)
            {
                value = temp;
                onChanged?.Invoke();
            }
        }
        private static void RenderRowRightAligned(string label, Action renderWidget, float widgetWidth = 0f)
        {
            ImGui.Columns(2, null, false);

            ImGui.Indent(LabelPadding);
            ImGui.Text(label);
            ImGui.Unindent(LabelPadding);
            ImGui.NextColumn();

            float colWidth = ImGui.GetColumnWidth();
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float desired = widgetWidth <= 0f ? 200f : widgetWidth;
            desired = Math.Min(desired, colWidth - spacing);

            float posX = ImGui.GetCursorPosX() + colWidth - desired - spacing;
            ImGui.SetCursorPosX(posX);

            ImGui.PushItemWidth(desired);
            renderWidget();
            ImGui.PopItemWidth();

            ImGui.NextColumn();
            ImGui.Columns(1);
        }

        private static void RenderBoolSetting(string label, ref bool value, Action? onChanged = null, float widgetWidth = 0f)
        {
            bool temp = value;
            RenderRowRightAligned(label, () =>
            {
                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;

                float colWidth = ImGui.GetColumnWidth();
                float spacing = ImGui.GetStyle().ItemSpacing.X;
                float posX = ImGui.GetCursorPosX() + colWidth - width - spacing;
                ImGui.SetCursorPosX(posX);

                Vector2 p = ImGui.GetCursorScreenPos();
                var drawList = ImGui.GetWindowDrawList();
                string strId = "##" + label;

                ImGui.InvisibleButton(strId, new Vector2(width, height));
                if (ImGui.IsItemClicked()) temp = !temp;

                float t = temp ? 1f : 0f;

                drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height), ImGui.ColorConvertFloat4ToU32(trackCol), height);

                float knobX = p.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = p.Y + radius + 2f;
                Vector4 knobColor = temp ? knobOn : knobOff;
                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius, ImGui.ColorConvertFloat4ToU32(knobColor), 36);
                drawList.AddCircle(new Vector2(knobX, knobY), radius, ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);
            }, widgetWidth);

            if (temp != value)
            {
                value = temp;
                onChanged?.Invoke();
            }
        }

        private void RenderBoolSettingWith2ColorPickers(string label, ref bool value, ref Vector4 color1, ref Vector4 color2)
        {
            ImGui.PushID(label);

            var tmp1 = color1;
            var tmp2 = color2;
            var tmpVal = value;

            RenderRowRightAligned(label, () =>
            {
                ImGui.ColorEdit4("##" + label + "col1", ref tmp1, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);
                ImGui.SameLine();

                ImGui.ColorEdit4("##" + label + "col2", ref tmp2, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);
                ImGui.SameLine();

                ImGui.Checkbox("##" + label + "checkmark", ref tmpVal);
            }, widgetWidth: 73f);

            if (!tmp1.Equals(color1))
            {
                color1 = tmp1;
            }
            if (!tmp2.Equals(color2))
            {
                color2 = tmp2;
            }
            if (tmpVal != value)
            {
                value = tmpVal;
            }

            ImGui.PopID();
        }

        private static void RenderBoolSettingWith1ColorPicker(string label, ref bool value, ref Vector4 color1)
        {
            ImGui.PushID(label);

            bool tmpVal = value;
            Vector4 tmpColor = color1;

            RenderRowRightAligned(label, () =>
            {
                Vector2 rowStart = ImGui.GetCursorScreenPos();
                float rowWidth = ImGui.GetColumnWidth();
                float paddingRight = 7f;

                ImGui.SetCursorScreenPos(rowStart + new Vector2(0, 0));
                ImGui.ColorEdit4("##" + label + "_col1", ref tmpColor, ImGuiColorEditFlags.NoInputs | ImGuiColorEditFlags.NoTooltip | ImGuiColorEditFlags.NoLabel);

                float height = ImGui.GetFrameHeight();
                float width = height * 1.7f;
                float radius = height / 2f - 2f;
                Vector2 knobPos = new(rowStart.X + rowWidth - width - paddingRight, rowStart.Y);

                var drawList = ImGui.GetWindowDrawList();
                ImGui.SetCursorScreenPos(knobPos);

                ImGui.InvisibleButton("##" + label + "_toggle", new Vector2(width, height));
                if (ImGui.IsItemClicked()) tmpVal = !tmpVal;

                float t = tmpVal ? 1f : 0f;
                drawList.AddRectFilled(knobPos, new Vector2(knobPos.X + width, knobPos.Y + height), ImGui.ColorConvertFloat4ToU32(trackCol), height);
                float knobX = knobPos.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = knobPos.Y + radius + 2f;
                drawList.AddCircleFilled(new Vector2(knobX, knobY), radius, ImGui.ColorConvertFloat4ToU32(tmpVal ? knobOn : knobOff), 36);
                drawList.AddCircle(new Vector2(knobX, knobY), radius, ImGui.ColorConvertFloat4ToU32(new Vector4(0.08f, 0.08f, 0.08f, 0.3f)), 36, 1f);
            });

            if (!tmpColor.Equals(color1)) color1 = tmpColor;
            value = tmpVal;

            ImGui.PopID();
        }

        private static void RenderIntCombo(string label, ref int current, string[] items, int itemCount, float widgetWidth = 160f)
        {
            int temp = current;

            RenderRowRightAligned(label, () =>
            {
                ImGui.Combo("##" + label, ref temp, items, items.Length);
            }, widgetWidth);

            if (temp != current)
            {
                current = temp;
            }
        }

        private static void RenderColorSetting(string label, ref Vector4 color, Action? onChanged = null, float widgetWidth = 160f)
        {
            Vector4 temp = color;
            RenderRowRightAligned(label, () =>
            {
                ImGui.ColorEdit4("##" + label, ref temp);
            }, widgetWidth);

            if (!temp.Equals(color))
            {
                color = temp;
                onChanged?.Invoke();
            }
        }

        private static void RenderFloatSlider(string label, ref float value, float min, float max, string format = "%.2f", float widgetWidth = 200f)
        {
            float temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.SliderFloat("##" + label, ref temp, min, max, format);
            }, widgetWidth);

            if (temp != value) value = temp;
        }

        private static void RenderIntSlider(string label, ref int value, int min, int max, string format = "%d", float widgetWidth = 200f)
        {
            int temp = value;
            RenderRowRightAligned(label, () =>
            {
                ImGui.SliderInt("##" + label, ref temp, min, max, format);
            }, widgetWidth);

            if (temp != value) value = temp;
        }

        public static void RenderKeybindChooser(string label, ref int key)
        {
            ImGui.PushID(label);

            if (!KeyBind.ContainsKey(label)) KeyBind[label] = false;

            string keyName = KeyBind[label] ? "Press Any Key..." : (key == (int)Keys.None ? "None" : Enum.GetName(typeof(Keys), key) ?? key.ToString());

            if (ImGui.Button(keyName, new Vector2(100, 0)))
            {
                KeyBind[label] = true;
            }

            if (KeyBind[label])
            {
                foreach (Keys k in Enum.GetValues<Keys>())
                {
                    if (k == Keys.None) continue;

                    short state = User32.GetAsyncKeyState((int)k);
                    bool pressed = (state & 0x8000) != 0;

                    if (pressed)
                    {
                        if (k == Keys.Escape)
                            key = (int)Keys.None;
                        else
                            key = (int)k;

                        KeyBind[label] = false;
                        break;
                    }
                }
            }

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
        }

        public static void RenderKeybindChooser(string Lable, ref ImGuiKey Key)
        {
            ImGui.PushID(Lable);

            if (!KeyBind.ContainsKey(Lable)) KeyBind[Lable] = false;

            string keyName = KeyBind[Lable] ? "Press Any Key..." : (Key == ImGuiKey.None ? "None" : Key.ToString());

            if (ImGui.Button(keyName, new Vector2(100, 0)))
            {
                KeyBind[Lable] = true;
            }

            if (KeyBind[Lable])
            {
                foreach (ImGuiKey imguiKey in Enum.GetValues<ImGuiKey>())
                {
                    if (ImGui.IsKeyPressed(imguiKey))
                    {
                        if (imguiKey == ImGuiKey.Escape)
                        {
                            Key = ImGuiKey.Insert;
                        }
                        else
                        {
                            Key = imguiKey;
                        }

                        KeyBind[Lable] = false;
                        break;
                    }
                }

                if (KeyBind[Lable])
                {
                    if (User32.GetAsyncKeyState((int)Keys.Menu) != 0 ||
                        User32.GetAsyncKeyState((int)Keys.LMenu) != 0 ||
                        User32.GetAsyncKeyState((int)Keys.RMenu) != 0)
                    {
                        Key = ImGuiKey.LeftAlt;
                        KeyBind[Lable] = false;
                    }
                }
            }

            ImGui.SameLine();
            ImGui.Text(Lable);

            ImGui.PopID();
        }

        public static void RenderSettingsSection(string label, Action content)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4));
            ImGui.Text(label);

            ImGui.Indent(16f);
            content();
            ImGui.Unindent(16f);

            ImGui.PopStyleVar();
        }

        private void ApplyCustomStyle()
        {
            ImGuiStylePtr style = ImGui.GetStyle();
            style.Alpha = windowAlpha;
            style.DisabledAlpha = 0.8f;
            style.WindowPadding = new Vector2(0.0f, 0.0f);
            style.WindowRounding = 12.0f;
            style.WindowBorderSize = 1.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 12f;
            style.ChildBorderSize = 1f;
            style.PopupRounding = 4f;
            style.PopupBorderSize = 1.0f;

            style.FramePadding = new Vector2(5.0f, 1.0f);
            style.FrameRounding = 5.0f;
            style.FrameBorderSize = 1.0f;
            style.ItemSpacing = new Vector2(6.0f, 4.0f);
            style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
            style.CellPadding = new Vector2(4.0f, 2.0f);
            style.IndentSpacing = 21f;
            style.ColumnsMinSpacing = 6f;
            style.ScrollbarSize = 10f;
            style.ScrollbarRounding = 4f;
            style.GrabMinSize = 20f;
            style.GrabRounding = 5f;
            style.TabRounding = 4f;
            style.TabBorderSize = 1f;
            style.TabMinWidthForCloseButton = 0;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

            style.Colors[(int)ImGuiCol.ScrollbarBg] = style.Colors[(int)ImGuiCol.WindowBg];
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.15f, 0.17f, 0.20f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.20f, 0.22f, 0.25f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.25f, 0.27f, 0.30f, 1.0f);
            style.Colors[(int)ImGuiCol.Text] = currentTextCol;
            style.Colors[(int)ImGuiCol.TextDisabled] = new(0.2745098173618317f, 0.3176470696926117f, 0.4501f, 1.0f);
            style.Colors[(int)ImGuiCol.WindowBg] = new(0.078f, 0.0862f, 0.101f, 1.0f);
            style.Colors[(int)ImGuiCol.ChildBg] = currentMainContentCol;
            style.Colors[(int)ImGuiCol.PopupBg] = new(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Border] = new(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.BorderShadow] = new(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1137254908680916f, 0.125490203499794f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.CheckMark] = currentAccentColor;
            style.Colors[(int)ImGuiCol.SliderGrab] = currentAccentColor;
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.6000000238418579f, 0.9647058844566345f, 0.0313725508749485f, 1.0f);
            style.Colors[(int)ImGuiCol.Button] = currentSidebarColor;
            style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);
            style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1529411822557449f, 0.1529411822557449f, 0.1529411822557449f, 1.0f);
            style.Colors[(int)ImGuiCol.Header] = new Vector4(0.1411764770746231f, 0.1647058874368668f, 0.2078431397676468f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.105882354080677f, 0.105882354080677f, 0.105882354080677f, 1.0f);
            style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1294117718935013f, 0.1490196138620377f, 0.1921568661928177f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = currentAccentColor;
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.125490203499794f, 0.2745098173618317f, 0.572549045085907f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.5215686559677124f, 0.6000000238418579f, 0.7019608020782471f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.03921568766236305f, 0.9803921580314636f, 0.9803921580314636f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.0313725508749485f, 0.9490196108818054f, 0.843137264251709f, 1.0f);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.0470588244497776f, 0.05490196123719215f, 0.07058823853731155f, 1.0f);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(0.09803921729326248f, 0.105882354080677f, 0.1215686276555061f, 1.0f);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.2666666805744171f, 0.2901960909366608f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(0.4980392158031464f, 0.5137255191802979f, 1.0f, 1.0f);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.196078434586525f, 0.1764705926179886f, 0.545f, 0.501f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.196078434586525f, 0.176f, 0.545f, 0.501f);
        }
        #endregion



        #region Cleanup
        protected override void Dispose(bool disposing)
        {
            isDisposing = true;

            // Zatrzymaj muzykę
            StopMusic();

            // Zatrzymaj wszystkie dźwięki
            StopAllSounds();




            base.Dispose(disposing);
        }

        ~Renderer()
        {
            isDisposing = true;
            StopMusic();
            StopAllSounds();
        }
        #endregion
    }
}
