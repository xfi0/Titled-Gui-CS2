using ClickableTransparentOverlay;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Numerics;
using System.Reflection;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Classes.Rendering.ChamsRenderer;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;
using Titled_Gui.ImGUI.Widgets;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using Vortice.Direct3D11;
using Image = SixLabors.ImageSharp.Image;

namespace Titled_Gui
{
    public class Renderer : Overlay
    {
        public static bool DrawWindow = false;
        public static bool EnableWatermark = true;
        public static bool IsTextFontNormalLoaded => !TextFontNormal.Equals(default(ImFontPtr));
        public static bool IsTextFont24Loaded => !TextFont24.Equals(default(ImFontPtr));
        public static bool IsTextFont48Loaded => !TextFont48.Equals(default(ImFontPtr));
        public static bool IsTextFont60Loaded => !TextFont60.Equals(default(ImFontPtr));
        public static bool IsIconFontLoaded => !IconFont.Equals(default(ImFontPtr));
        public static bool IsGunIconFontLoaded => !GunIconsFont.Equals(default(ImFontPtr));
        public static bool MenuSounds = true;
        public static bool EnableVsync = true;
        public bool ShouldDraw = false;
        public List<Entity> Entities = [];
        public Entity LocalPlayer = new();
        public ImDrawListPtr DrawList;
        public ImDrawListPtr BgDrawList;
        public ImDrawListPtr FgDrawList;
        private bool _logoLoaded = false;
        private int _selectedTab = 0;

        public static int NumberOfParticles = 50;
        public static int OverlayProcessId = 0;
        public static int CS2ProcessId = 0;

        public static float FpsUpdateInterval = 1.0f;
        public static float TimeSinceLastUpdate = 0.0f;
        public static float LastFPS = 0.0f;
        public static float WindowAlpha = 1f;
        public static float AnimationSpeed = 0.15f;
        public static float ParticleRadius = 2.5f;
        public static float ParticleSpeed = 0.53f;
        public static float MaxLineDistance = 300f;
        public static float MenuSoundsVolume = 0.8f;

        public Vector2 ScreenSize = new(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        public static Vector2 TabSize;
        public static Vector2 BaseParticlePos = new();
        public static Vector2 MainWindowSize = new(860, 550);
        public static Vector2 WatermarkSize = new(210, 40);

        private static Vector4 _secondaryColor = new(0.102f, 0.102f, 0.102f, 1);
        private static Vector4 _primaryColor = new(0.125f, 0.125f, 0.125f, 1);
        public static Vector4 TextCol = new(0.274f, 0.317f, 0.450f, 1.0f);
        public static Vector4 HeaderStartCol = TextCol;
        public static Vector4 HeaderEndCol = new(1, 1, 1, 0);
        private static Vector4 _particleColor = new(1f, 1f, 1f, 1f);
        private static Vector4 _lineColor = new(1, 1, 1, 0.33f);
        public static Colors MenuColors = new Colors(primaryColor: _primaryColor, secondaryColor: _secondaryColor);
        public static Colors BackgroundEffectColors = new Colors(primaryColor: _particleColor, secondaryColor: _lineColor);

        public static ImFontPtr TextFontNormal;
        public static ImFontPtr TextFont24;
        public static ImFontPtr TextFont48;
        public static ImFontPtr TextFont60;
        public static ImFontPtr IconFont;
        public static ImFontPtr GunIconsFont;

        public static int OpenKeyInt = 0x2D;

        public static Random Random = new();
        private static Chams _chamsRenderer = new();
        private static WorldESP.WorldChams _worldChamsRenderer = new();
        public static List<Vector2> Positions = [];
        public static List<Vector2> Velocities = [];
        public static HashSet<Keys> KeysSet =
        [
            Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey,
    Keys.ControlKey, Keys.LControlKey, Keys.RControlKey,
    Keys.Menu, Keys.LMenu, Keys.RMenu
        ];

        private IntPtr _menuLogoTexture;
        private readonly object _entityLock = new();
        private const string _menuImage = "iVBORw0KGgoAAAANSUhEUgAABAAAAAQACAMAAABIw9uxAAAAAXNSR0IB2cksfwAAAAlwSFlzAAALEwAACxMBAJqcGAAAAm1QTFRFAAAA0dDQz9DR1tXW19bV1tfW2NjY2dnX2NfX19nY19rZ2dja2drZ2dnY2NnZ2tjZ2NrZ2tvb2tnb2tra2dnZ2dnb2tnY2tnZ2dvZ2tva2NvZ2Nra2tza2trZ2tra2drZ29va2tvZ2tnY2tra2trY29rY2NrZ2dra2dva2tvb2tzb3NnZ2drZ2dvZ2tnc2Nna2drY29rb2tna29vb2tva2djZ2trY19vX19rY2NnY19jW19jW19fW2NnX19nX1tbX1dXW1tfV1NPT09XT0tHRy83NzczM1dfW2NjZ1tfZ1trY2dnZ////////2Nna2djZ19jY2NjY2dna2dfX////////29ra////19ja////////2dra////////2tjY////////////////////////////////////19jZ2NfZ////////////1tXV////////////////////////////////////////////////////////////////2Nra2tjY2drY2NjX2dnZ2dva2Nja2NjZ1tfX1tnZ1tjY19fZ2dja1tjZ1tjZ2Nna2Nva2dra2Nvb2NzZ19nZ19fX1tnY19bZ19jZ2NnZ1tfY2drb19jY19ra2NfY19nZ29jY2NjY1trZ2Nva2Nvad3d42NrY2NvY2Nrc29rZ2dnX2dvb19rZ2NnY2dvZ19na2NnZ1tja2NvZ2NbY2djY1tjX2NnW19vY2NnY1tfX1tnY1tjY19jX19XW19jX2NbX1tfX19bY1dbX1NfV1dXV1dfW1NXV1NXV0tTSzc7MbG1sYmBhXV5df4B/W1xbWFhYXV1dapVdogAAAM90Uk5TAAIBAQEBAQIBAQECAQEBAQEBAgIBAQEBAQEBAQEBAwIBAQIBAgECAwICAQIDAgEBAQEBAQICAQIBAQECAQEBAQIBAQEBAQECAgEBA/8OAgEBAgEBJSgCHwEGIwErMwI+PCkeHAQDGgEBARYUFwICBQc9OUJKRUQiLC43HSEqAgECAgIBAQECAgIBAQECAwICAgIBAQECAgMBAQIBAQMBAwIDAQEBAwEBAQEDAgMBAgECAQIBAgIEAQIBAwEBAgMB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";

        public static ID3D11Device? GetDevice()
        {
            Renderer? overlay = GameState.renderer;
            if (overlay == null)
                return null;

            Type? baseType = typeof(Overlay);
            if (baseType == null)
                return null;

            return (ID3D11Device?)baseType.GetField("device", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(overlay);
        }

        public static ID3D11DeviceContext? GetDeviceContext()
        {
            Renderer? overlay = GameState.renderer;
            if (overlay == null)
                return null;

            Type? baseType = typeof(Overlay);
            if (baseType == null)
                return null;

            return (ID3D11DeviceContext?)baseType.GetField("deviceContext", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(overlay);
        }

        public static System.Collections.IDictionary? GetTextureResources()
        {
            Renderer? overlay = GameState.renderer;
            if (overlay == null)
                return null;

            Type? baseType = typeof(Overlay);
            if (baseType == null)
                return null;

            Object? renderer = baseType.GetField("renderer", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(overlay);
            if (renderer == null)
                return null;

            return (System.Collections.IDictionary?)renderer.GetType().GetField("textureResources", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(renderer);
        }

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            lock (_entityLock)
                Entities = (List<Entity>)newEntities ?? newEntities.ToList();
        }

        public static void LoadFonts()
        {
            try
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero) ImGui.CreateContext();

                var io = ImGui.GetIO();

                ushort[] ranges = { 0x0020, 0x00FF, 0xE000, 0xF8FF, 0 };
                unsafe
                {
                    byte[] iconFontData = LoadFont("NotoSans-BoldIcons.ttf");

                    fixed (ushort* pRanges = ranges)
                    {
                        byte[] d1 = (byte[])iconFontData.Clone();
                        byte[] d2 = (byte[])iconFontData.Clone();
                        byte[] d3 = (byte[])iconFontData.Clone();
                        byte[] d4 = (byte[])iconFontData.Clone();
                        byte[] d5 = (byte[])iconFontData.Clone();

                        fixed (byte* p2 = d2) TextFontNormal = io.Fonts.AddFontFromMemoryTTF((IntPtr)p2, d2.Length, 18.0f, null, (IntPtr)pRanges);
                        fixed (byte* p1 = d1) IconFont = io.Fonts.AddFontFromMemoryTTF((IntPtr)p1, d1.Length, 24.0f, null, (IntPtr)pRanges);
                        fixed (byte* p3 = d3) TextFont24 = io.Fonts.AddFontFromMemoryTTF((IntPtr)p3, d3.Length, 24.0f, null, (IntPtr)pRanges);
                        fixed (byte* p4 = d4) TextFont48 = io.Fonts.AddFontFromMemoryTTF((IntPtr)p4, d4.Length, 48.0f, null, (IntPtr)pRanges);
                        fixed (byte* p5 = d5) TextFont60 = io.Fonts.AddFontFromMemoryTTF((IntPtr)p5, d5.Length, 60.0f, null, (IntPtr)pRanges);
                    }
                }

                unsafe
                {
                    byte[] gunIconsData = LoadFont("undefeated.ttf");

                    fixed (byte* gunFontData = gunIconsData)
                        GunIconsFont = io.Fonts.AddFontFromMemoryTTF((IntPtr)gunFontData, gunIconsData.Length, 24.0f);
                }

                io.Fonts.Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static byte[] LoadFont(string fileName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();

            Stream? stream = asm.GetManifestResourceStream("Titled_Gui.Resources.fonts." + fileName) ?? throw new Exception("Font was not found");
            byte[] fontData = new byte[stream.Length];
            stream.ReadExactly(fontData);

            return fontData;
        }

        public void UpdateLocalPlayer(Entity newEntity) // update local player
        {
            lock (_entityLock)
            {
                LocalPlayer = newEntity;
            }
        }

        protected override Task PostInitialized()
        {
            var io = ImGui.GetIO();
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard;
            io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;
            io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
            io.ConfigViewportsNoAutoMerge = true;
            io.ConfigViewportsNoTaskBarIcon = true;
            // this makes cpu useage about 2x but makes the overlay like 2ms lat
            //int disabled = 1;
            //DwmSetWindowAttribute(Process.GetCurrentProcess().Handle, 3, ref disabled, sizeof(int));
            //timeBeginPeriod(1);
            //Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            var style = ImGui.GetStyle();
            ApplyStyles(style);

            return Task.CompletedTask;
        }

        protected override void Render()
        {
            try
            {
                this.VSync = EnableVsync;

                RenderESPOverlay();
                RenderMainWindow();
                RenderWaterMark();
                Keybind.RenderKeybindMenu();
                SpectatorList.DrawMenu();
                BombTimerOverlay.TimeOverlay();
                //Library.UpdateNotifications(io.DeltaTime);
                Toggles.LoopAllActions();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }


        public void RenderWaterMark()
        {
            if (!EnableWatermark)
                return;

            ImGui.PushFont(TextFontNormal);
            ImGui.SetNextWindowSize(WatermarkSize);
            ImGui.SetNextWindowPos(new(ScreenSize.X - WatermarkSize.X, 0));
            ImGui.Begin("wm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar);
            var drawList = ImGui.GetWindowDrawList();
            Vector2 textPosition = ImGui.GetWindowPos() + new Vector2(10, 10);
            TimeSinceLastUpdate += ImGui.GetIO().DeltaTime;

            if (TimeSinceLastUpdate >= FpsUpdateInterval)
            {
                LastFPS = 1f / ImGui.GetIO().DeltaTime;
                TimeSinceLastUpdate = 0.0f;
            }

            drawList.AddText(new(textPosition.X, textPosition.Y), ImGui.ColorConvertFloat4ToU32(new(1, 1, 1, 1)), $"Titled | FPS: {Math.Round(LastFPS)} | V-{Configs.Version} | {DateTime.Now.ToLocalTime().ToShortTimeString()}");
            ImGui.PopFont();
            ImGui.End();

        }
        private void RenderESPOverlay()
        {
            ImGui.SetNextWindowSize(ScreenSize);
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
            DrawList = ImGui.GetWindowDrawList();
            var io = ImGui.GetIO();
            io.Framerate = 0;
            io.ConfigViewportsNoAutoMerge = true;
            io.ConfigViewportsNoTaskBarIcon = true;

            //ImGui.ShowMetricsWindow();
            ImGui.End();
        }

        public static void ApplyColors()
        {
            var style = ImGui.GetStyle();

            var windowPrimaryColor = MenuColors.PrimaryRGB ? Colors.Rgb(WindowAlpha) : MenuColors.PrimaryColor;
            var windowSecondaryColor = MenuColors.SecondaryRGB ? Colors.Rgb(WindowAlpha) : MenuColors.SecondaryColor;

            style.Colors[(int)ImGuiCol.Text] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TextDisabled] = new(0.45f, 0.45f, 0.45f, WindowAlpha);
            style.Colors[(int)ImGuiCol.WindowBg] = new(windowPrimaryColor.X, windowPrimaryColor.Y, windowPrimaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.ChildBg] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.PopupBg] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.Border] = new(0.22f, 0.22f, 0.22f, WindowAlpha);
            style.Colors[(int)ImGuiCol.BorderShadow] = new(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.FrameBg] = new(0.152f, 0.152f, 0.152f, WindowAlpha);
            style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.180f, 0.188f, 0.196f, WindowAlpha);
            style.Colors[(int)ImGuiCol.FrameBgActive] = new(0.200f, 0.208f, 0.216f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TitleBg] = new(0.085f, 0.085f, 0.085f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TitleBgActive] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.085f, 0.085f, 0.085f, WindowAlpha);
            style.Colors[(int)ImGuiCol.MenuBarBg] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.ScrollbarBg] = new(0.102f, 0.102f, 0.102f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ScrollbarGrab] = new(0.22f, 0.22f, 0.22f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(0.28f, 0.28f, 0.28f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new(0.32f, 0.32f, 0.32f, WindowAlpha);
            style.Colors[(int)ImGuiCol.CheckMark] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.SliderGrab] = new(0.28f, 0.28f, 0.28f, WindowAlpha);
            style.Colors[(int)ImGuiCol.SliderGrabActive] = new(0.36f, 0.36f, 0.36f, WindowAlpha);
            style.Colors[(int)ImGuiCol.Button] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.ButtonHovered] = new(0.180f, 0.188f, 0.196f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ButtonActive] = new(0.152f, 0.152f, 0.152f, WindowAlpha);
            style.Colors[(int)ImGuiCol.Header] = new(0.152f, 0.152f, 0.152f, WindowAlpha);
            style.Colors[(int)ImGuiCol.HeaderHovered] = new(0.180f, 0.188f, 0.196f, WindowAlpha);
            style.Colors[(int)ImGuiCol.HeaderActive] = new(0.200f, 0.208f, 0.216f, WindowAlpha);
            style.Colors[(int)ImGuiCol.Separator] = new(0.22f, 0.22f, 0.22f, WindowAlpha);
            style.Colors[(int)ImGuiCol.SeparatorHovered] = new(0.32f, 0.32f, 0.32f, WindowAlpha);
            style.Colors[(int)ImGuiCol.SeparatorActive] = new(0.40f, 0.40f, 0.40f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ResizeGrip] = new(0.22f, 0.22f, 0.22f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ResizeGripHovered] = new(0.32f, 0.32f, 0.32f, WindowAlpha);
            style.Colors[(int)ImGuiCol.ResizeGripActive] = new(0.40f, 0.40f, 0.40f, WindowAlpha);
            style.Colors[(int)ImGuiCol.Tab] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.TabHovered] = new(0.180f, 0.188f, 0.196f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TabActive] = new(0.152f, 0.152f, 0.152f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TabUnfocused] = new(0.102f, 0.102f, 0.102f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.PlotLines] = new(0.60f, 0.60f, 0.60f, WindowAlpha);
            style.Colors[(int)ImGuiCol.PlotLinesHovered] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.PlotHistogram] = new(0.50f, 0.50f, 0.50f, WindowAlpha);
            style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new(0.70f, 0.70f, 0.70f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TableHeaderBg] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.TableBorderStrong] = new(0.22f, 0.22f, 0.22f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TableBorderLight] = new(0.16f, 0.16f, 0.16f, WindowAlpha);
            style.Colors[(int)ImGuiCol.TableRowBg] = new(0.0f, 0.0f, 0.0f, 0.0f);
            style.Colors[(int)ImGuiCol.TableRowBgAlt] = new(windowSecondaryColor.X, windowSecondaryColor.Y, windowSecondaryColor.Z, WindowAlpha);
            style.Colors[(int)ImGuiCol.TextSelectedBg] = new(0.28f, 0.28f, 0.28f, WindowAlpha);
            style.Colors[(int)ImGuiCol.DragDropTarget] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.NavHighlight] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new(0.84f, 0.84f, 0.84f, WindowAlpha);
            style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.0f, 0.0f, 0.0f, 0.4f);
            style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.0f, 0.0f, 0.0f, 0.4f);
        }

        public static void ApplyStyles()
        {
            var style = ImGui.GetStyle();
            ApplyStyles(style);
        }

        public static void ApplyStyles(ImGuiStylePtr style)
        {
            style.Alpha = WindowAlpha;
            style.DisabledAlpha = 0.8f;
            style.WindowPadding = new Vector2(0.0f, 0.0f);
            style.WindowRounding = 6.0f;
            style.WindowBorderSize = 2.0f;
            style.WindowMinSize = new Vector2(32.0f, 32.0f);
            style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
            style.WindowMenuButtonPosition = ImGuiDir.Left;
            style.ChildRounding = 0f;
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
            style.ScrollbarSize = 13f;
            style.ScrollbarRounding = 16f;
            style.GrabMinSize = 20f;
            style.GrabRounding = 5f;
            style.TabRounding = 4f;
            style.TabBorderSize = 1f;
            style.TabMinWidthForCloseButton = 0;
            style.ColorButtonPosition = ImGuiDir.Right;
            style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
            style.SelectableTextAlign = new Vector2(0.0f, 0.0f);
            style.ScrollbarSize = 10f;
            style.ScrollbarRounding = 4f;

            ApplyColors();
        }

        private void RenderMainWindow()
        {
            if (User32.GetKeyPressed(OpenKeyInt))
                DrawWindow = !DrawWindow;

            BgDrawList = ImGui.GetBackgroundDrawList();
            if (DrawWindow)
            {
                ImGUI.Widgets.Preview.DrawWindow();
                BgDrawList.AddRectFilled(Vector2.Zero, ScreenSize, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.5f))); // ts the dimmed background TODO: make a opacity changer
                DrawParticles(NumberOfParticles);
                ImGui.SetNextWindowPos(new Vector2((ScreenSize.X - 800) / 2f, (ScreenSize.Y - 600) / 2f),
                    ImGuiCond.Always);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.Begin("",
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoDocking);
                ImGui.SetWindowSize(MainWindowSize);

                Vector2 tabPos = ImGui.GetCursorScreenPos();
                var availableSpace = ImGui.GetContentRegionAvail();
                var availableHeight = availableSpace.Y;
                var availableWidth = availableSpace.X;

                TabSize = new(140, ImGui.GetContentRegionAvail().Y);
                DrawList.AddRectFilled(tabPos, tabPos + TabSize, ImGui.ColorConvertFloat4ToU32(new(0.125f, 0.125f, 0.125f, WindowAlpha)), 12.0f,
                    ImDrawFlags.RoundCornersLeft);

                ImGui.BeginChild("Sidebar", TabSize, ImGuiChildFlags.None);
                {
                    const float logoWidth = 120f;
                    float offset = (ImGui.GetContentRegionAvail().X - logoWidth) * 0.5f;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

                    if (!_logoLoaded)
                    {
                        byte[] LogoBytes = Convert.FromBase64String(_menuImage);
                        Image<Rgba32> LogoImage = Image.Load<Rgba32>(LogoBytes);
                        AddOrGetImagePointer("MenuLogo", LogoImage, true, out _menuLogoTexture);
                        _logoLoaded = true;
                    }

                    ImGui.Image(_menuLogoTexture, new(120, 120));
                    ImGui.Spacing();

                    ImGui.Separator();
                    ImGui.Spacing();

                    RenderTabButton("\uF53B", "Legit", 0);
                    RenderTabButton("\uF15E", "Visuals", 2);
                    RenderTabButton("\uF1BC", "Aim", 1);
                    RenderTabButton("\uF35A", "Configs", 3);

                    const float cogButtonHeight = 35f;
                    var spacingHeight = ImGui.GetContentRegionAvail().Y - cogButtonHeight - 5f;

                    if (spacingHeight > 0)
                        ImGui.Dummy(new(0, spacingHeight));


                    Vector2 cogPos = ImGui.GetCursorScreenPos();
                    Vector2 cogSize = new(ImGui.GetContentRegionAvail().X, cogButtonHeight);

                    if (ImGui.InvisibleButton("##SettingsGear", cogSize))
                        _selectedTab = 4;


                    bool isHovered = ImGui.IsItemHovered();
                    bool isSettingsSelected = _selectedTab == 4;
                    Vector2 gearCenter = new(cogPos.X + cogSize.X / 2, cogPos.Y + cogSize.Y / 2);

                    uint gearColor;
                    if (isSettingsSelected)
                        gearColor = ImGui.ColorConvertFloat4ToU32(MenuColors.SecondaryRGB ? Colors.Rgb(WindowAlpha) : Renderer.MenuColors.SecondaryColor);

                    else if (isHovered)
                        gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, 1));

                    else
                        gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 1));


                    DrawGearIcon(gearCenter, gearColor);
                }
                ImGui.EndChild();

                ImGui.SameLine(0f, 0f);

                Vector2 mainPos = ImGui.GetCursorScreenPos();
                Vector2 mainSize = ImGui.GetContentRegionAvail();

                DrawList.AddRectFilled(mainPos, mainPos + mainSize,
                    ImGui.ColorConvertFloat4ToU32(new(0.094f, 0.102f, 0.118f, WindowAlpha)), 12.0f,
                    ImDrawFlags.RoundCornersBottom);

                ImGui.BeginChild("MainContent", mainSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    ImGui.PopStyleVar();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12.0f);
                    //RenderTitle("Titled");
                    switch (_selectedTab)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 4:
                            ImGui.Dummy(new Vector2(0, 4));
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY());
                            float availW = ImGui.GetContentRegionAvail().X;
                            float sectionW = (availW - ImGui.GetStyle().ItemSpacing.X) / 2f - 4f;
                            float leftX = ImGui.GetCursorPosX() + 4f;
                            float rightX = leftX + sectionW + ImGui.GetStyle().ItemSpacing.X;
                            float startY = ImGui.GetCursorPosY();

                            var tabSections = Sections.sections.Where(s => s.tab == _selectedTab).ToList();
                            var leftSections = tabSections.Where((_, i) => i % 2 == 0).ToList();
                            var rightSections = tabSections.Where((_, i) => i % 2 != 0).ToList();

                            float curY = startY;
                            foreach (var s in leftSections)
                            {
                                ImGui.SetCursorPos(new Vector2(leftX, curY));
                                Sections.BeginSection(s.label, s.content, new Vector2(sectionW, 0));
                                curY = ImGui.GetCursorPosY() + 4f;
                            }
                            float leftColBottom = curY;

                            curY = startY;
                            foreach (var s in rightSections)
                            {
                                ImGui.SetCursorPos(new Vector2(rightX, curY));
                                Sections.BeginSection(s.label, s.content, new Vector2(sectionW, 0));
                                curY = ImGui.GetCursorPosY() + 4f;
                            }
                            float rightColBottom = curY;

                            ImGui.SetCursorPosY(Math.Max(leftColBottom, rightColBottom));
                            break;

                        case 3: // config
                            float availWidth = ImGui.GetContentRegionAvail().X;
                            float wiodthy = (availWidth - ImGui.GetStyle().ItemSpacing.X) / 2f - 6f;
                            ImGui.Dummy(new Vector2(0, 4));
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 4f);

                            Sections.BeginSection("ConfigList", () =>
                            {
                                foreach (var config in Configs.SavedConfigs.Keys)
                                {
                                    if (ImGui.Selectable(config, Configs.SelectedConfig == config))
                                        Configs.SelectedConfig = config;
                                }
                            }, new Vector2(wiodthy, 200));

                            ImGui.SameLine();

                            Sections.BeginSection("ConfigOptions", () =>
                            {
                                ImGui.SetNextItemWidth(-1);
                                ImGui.InputText("##ConfigName", ref Configs.SelectedConfig, 24);
                                ImGui.Dummy(new Vector2(0, 4));

                                if (ImGui.Button("Save Config", new Vector2(-1, 30)))
                                {
                                    if (!Configs.SavedConfigs.ContainsKey(Configs.SelectedConfig))
                                    {
                                        Configs.SaveConfig(Configs.SelectedConfig);
                                        Configs.SavedConfigs.TryAdd(Configs.SelectedConfig, false);
                                    }
                                    else
                                    {
                                        Console.WriteLine("Config already exsists, overwriting.");
                                        Configs.SavedConfigs.Remove(Configs.SelectedConfig, out bool value);
                                        Configs.SaveConfig(Configs.SelectedConfig);
                                        Configs.SavedConfigs.TryAdd(Configs.SelectedConfig, false);
                                    }
                                }

                                if (ImGui.Button("Load Config", new Vector2(-1, 30)))
                                {
                                    if (!string.IsNullOrEmpty(Configs.SelectedConfig))
                                        Configs.LoadConfig(Configs.SelectedConfig);

                                    Sections.sections.Clear();
                                    Sections.sections = Sections.InitializeSections(); // we re-init sections because all the refs would be pointing to the old values.
                                }
                            }, new Vector2(wiodthy, 200));

                            break;
                    }
                    ImGui.PopStyleVar(2);
                }
                ImGui.EndChild();

                ImGui.End();
            }

            bool cs2Focused = User32.IsWindowFocused(CS2ProcessId);
            bool overlayFocused = OverlayProcessId != 0 && User32.IsWindowFocused(OverlayProcessId);

            if (cs2Focused || overlayFocused)
                RunAllModules();
        }

        public void DrawParticles(int num)
        {
            var circleColor = BackgroundEffectColors.PrimaryRGB ? Colors.Rgb(BackgroundEffectColors.PrimaryColor.W) : BackgroundEffectColors.PrimaryColor;
            var lineColor = BackgroundEffectColors.SecondaryRGB ? Colors.Rgb(BackgroundEffectColors.SecondaryColor.W) : BackgroundEffectColors.SecondaryColor;
            while (Positions.Count < num || Velocities.Count < num) // only add if there isnt eg 50 drawn
            {
                Positions.Add(new Vector2(Random.Next((int)ScreenSize.X), Random.Next((int)ScreenSize.Y)));
                Velocities.Add(new Vector2((float)(Random.NextDouble() * 2 - 1), (float)(Random.NextDouble() * 2 - 1)));
            }

            for (int i = 0; i < num; i++)
            {
                Positions[i] += Velocities[i] * ParticleSpeed;

                if (Positions[i].X < 0 || Positions[i].X > ScreenSize.X || Positions[i].Y < 0 ||
                    Positions[i].Y > ScreenSize.Y)
                {
                    Positions[i] = new Vector2(Random.Next((int)ScreenSize.X), Random.Next((int)ScreenSize.Y));
                    Velocities[i] = new Vector2((float)(Random.NextDouble() * 2 - 1),
                        (float)(Random.NextDouble() * 2 - 1));
                }

                GlowRenderer.DrawGlowCircleFilled(DrawList, Positions[i], ParticleRadius, circleColor, 1.1f);
            }

            for (int i = 0; i < num; i++) // lines
            {
                for (int j = i + 1; j < num; j++)
                {
                    float dist = Vector2.Distance(Positions[i], Positions[j]);
                    if (dist < MaxLineDistance)
                    {
                        float alpha = 1f - (dist / MaxLineDistance);
                        DrawList.AddLine(Positions[i], Positions[j],
                            ImGui.ColorConvertFloat4ToU32(new Vector4(lineColor.X, lineColor.Y, lineColor.Z,
                                lineColor.W * alpha)), 1f);
                    }
                }
            }
        }

        public void RunAllModules()
        {
            try
            {
                if (Aimbot.AimbotEnable && Aimbot.TargetLine)
                    Aimbot.RenderTargetLine();

                _chamsRenderer.RenderFrame();
                _worldChamsRenderer.RenderFrame();
                HitStuff.CreateHitText();

                if (EyeRay.Enabled)
                    EyeRay.DrawEyeRay();

                List<Entity> snapshot;
                lock (_entityLock)
                    snapshot = Entities;

                foreach (var entity in snapshot)
                {
                    if (entity == null)
                        continue;

                    Modules.Visual.BoneESP.DrawBoneLines(entity, this);
                    NameDisplay.DrawName(entity, this);
                    PingDisplay.DrawPing(entity, this);
                    BoxESP.DrawBoxESP(entity);
                    Titled_Gui.Modules.Visual.DistanceText.DrawDistance(entity);
                    Tracers.DrawTracers(entity, this);

                    var rect = BoxESP.GetBoxRect(entity);
                    if (rect == null)
                        continue;

                    Vector2 barTopLeft = new(rect.TopLeft.X - HealthBar.HealthBarWidth - 2, rect.TopLeft.Y);
                    float height = rect.BottomRight.Y - rect.TopLeft.Y;

                    HealthBar.DrawHealthBar(entity, entity.Health, 100, barTopLeft, height);
                    ArmorBar.DrawArmorBar(entity, this, entity.Armor, 100);
                    Flags.DrawFlags(entity);
                }

                if (GernadeLineup.Enabled)
                    GernadeLineup.DrawAllLineups();

                if (Aimbot.DrawFov && Aimbot.AimbotEnable && Aimbot.UseFOV)
                    Aimbot.DrawFOVCircle(Aimbot.FovSize, Aimbot.RGB ? Colors.Rgb(Aimbot.FovColor.W) : Aimbot.FovColor);

                if (C4ESP.Enabled)
                {
                    C4ESP.DrawESP();
                }

                WorldESP.EntityESP();
                Radar.DrawRadar();

                SoundESP.Draw();

                if (MenuColors.PrimaryRGB || MenuColors.SecondaryRGB)
                    ApplyColors();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void DrawGearIcon(Vector2 center, uint color)
        {

            ImGui.PushFont(IconFont);

            Vector2 textSize = ImGui.CalcTextSize("\uF3DC");
            Vector2 textPos = new(center.X - textSize.X / 2, center.Y - textSize.Y / 2);

            ImGui.GetWindowDrawList().AddText(textPos, color, "\uF3DC");

            ImGui.PopFont();
        }


        private void RenderTabButton(string icon, string label, int tabIndex)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 5)); // slight space between tabs
            bool isSelected = _selectedTab == tabIndex;
            ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
            if (IsIconFontLoaded)
            {
                ImGui.PushFont(IconFont);

                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero); // transparent background
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                    //Library.SendNotification("Tab", "Switched to tab");
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                }

                ImGui.PopStyleColor(isSelected ? 4 : 1);
            }

            bool pressed = ImGui.InvisibleButton(label, new Vector2(TabSize.X, 40));
            int paddingLeft = 8;
            Vector2 pos = ImGui.GetItemRectMin();
            Vector2 size = ImGui.GetItemRectSize();

            var iconSize = ImGui.CalcTextSize(icon);
            var labelSize = ImGui.CalcTextSize(label);
            var borderPadding = new Vector2(5, 0);
            var offset = new Vector2(10, 0);

            windowDrawList.AddRect(pos + borderPadding, pos + size - borderPadding - offset, ImGui.GetColorU32(ImGuiCol.Border), 6.0f);
            windowDrawList.AddText(new Vector2(pos.X + paddingLeft, pos.Y + (size.Y - iconSize.Y) * 0.5f), ImGui.GetColorU32(ImGuiCol.Text), icon);
            windowDrawList.AddText(new Vector2(pos.X + paddingLeft * 2 + 20, pos.Y + (size.Y - labelSize.Y) * 0.5f - 2), ImGui.GetColorU32(ImGuiCol.Text), label);

            if (pressed)
            {
                _selectedTab = tabIndex;
                PlayTabClickSound();
            }

            ImGui.PopFont();
            ImGui.PopStyleVar(); // restore spacing
        }

        public static void PlayTabClickSound()
        {
            if (!MenuSounds)
                return;

            Classes.PlaySound.PlaySoundFileEmbedded("Creamy.wav", "ClickSounds.", MenuSoundsVolume);
        }

        public static void RenderSettingsSection(string label, Action content)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4));
            ImGui.Indent(5f);
            ImGui.Text(label);

            ImGui.Indent(21f);
            content();
            ImGui.Unindent(16f);

            ImGui.PopStyleVar();
        }
    }
}
