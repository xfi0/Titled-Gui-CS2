using ClickableTransparentOverlay;
using ImGuiNET;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Media;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.Pkcs;
using System.Text.Json.Nodes;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.VRF;
using Titled_Gui.ImGUI.Widgets;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using Titled_Gui.Notifications;
using Vortice.Mathematics;
using ZstdSharp.Unsafe;
using static System.Collections.Specialized.BitVector32;
using static Titled_Gui.Data.Game.MapParser.MapLoader;
using static Titled_Gui.ImGUI.Widgets.ColorPickers;
using static Titled_Gui.ImGUI.Widgets.Combos;
using static Titled_Gui.ImGUI.Widgets.Sliders;
using static Titled_Gui.ImGUI.Widgets.Toggles;
using Colors = Titled_Gui.Classes.Colors;
using Image = SixLabors.ImageSharp.Image;

namespace Titled_Gui
{
    public class Renderer : Overlay
    {
        public Vector2 ScreenSize = new(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        private IntPtr _menuLogoTexture;
        private uint _width;
        private uint _height;
        private const string menuImage = "iVBORw0KGgoAAAANSUhEUgAABAAAAAQACAMAAABIw9uxAAAAAXNSR0IB2cksfwAAAAlwSFlzAAALEwAACxMBAJqcGAAAAm1QTFRFAAAA0dDQz9DR1tXW19bV1tfW2NjY2dnX2NfX19nY19rZ2dja2drZ2dnY2NnZ2tjZ2NrZ2tvb2tnb2tra2dnZ2dnb2tnY2tnZ2dvZ2tva2NvZ2Nra2tza2trZ2tra2drZ29va2tvZ2tnY2tra2trY29rY2NrZ2dra2dva2tvb2tzb3NnZ2drZ2dvZ2tnc2Nna2drY29rb2tna29vb2tva2djZ2trY19vX19rY2NnY19jW19jW19fW2NnX19nX1tbX1dXW1tfV1NPT09XT0tHRy83NzczM1dfW2NjZ1tfZ1trY2dnZ////////2Nna2djZ19jY2NjY2dna2dfX////////29ra////19ja////////2dra////////2tjY////////////////////////////////////19jZ2NfZ////////////1tXV////////////////////////////////////////////////////////////////2Nra2tjY2drY2NjX2dnZ2dva2Nja2NjZ1tfX1tnZ1tjY19fZ2dja1tjZ1tjZ2Nna2Nva2dra2Nvb2NzZ19nZ19fX1tnY19bZ19jZ2NnZ1tfY2drb19jY19ra2NfY19nZ29jY2NjY1trZ2Nva2Nvad3d42NrY2NvY2Nrc29rZ2dnX2dvb19rZ2NnY2dvZ19na2NnZ1tja2NvZ2NbY2djY1tjX2NnW19vY2NnY1tfX1tnY1tjY19jX19XW19jX2NbX1tfX19bY1dbX1NfV1dXV1dfW1NXV1NXV0tTSzc7MbG1sYmBhXV5df4B/W1xbWFhYXV1dapVdogAAAM90Uk5TAAIBAQEBAQIBAQECAQEBAQEBAgIBAQEBAQEBAQEBAwIBAQIBAgECAwICAQIDAgEBAQEBAQICAQIBAQECAQEBAQIBAQEBAQECAgEBA/8OAgEBAgEBJSgCHwEGIwErMwI+PCkeHAQDGgEBARYUFwICBQc9OUJKRUQiLC43HSEqAgECAgIBAQECAgIBAQECAwICAgIBAQECAgMBAQIBAQMBAwIDAQEBAwEBAQEDAgMBAgECAQIBAgIEAQIBAwEBAgMB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";

        //entity copy  
        public List<Entity> entities = [];
        public Entity localPlayer = new();
        private readonly object entityLock = new();

        private int selectedTab = 0; // 0 = legit, 1 = rage, 2 = visuals, 3 = config, 4 = settings

        public ImDrawListPtr drawList;
        public ImDrawListPtr BGdrawList;
        public ImDrawListPtr FGdrawList;
        public static Vector2 tabSize;

        public static bool DrawWindow = false;
        public static float fpsUpdateInterval = 1.0f;
        public static float timeSinceLastUpdate = 0.0f;
        public static float lastFPS = 0.0f;
        public static Vector4 accentColor = new(0.26f, 0.59f, 0.98f, 1.00f);
        public static Vector4 MainContentCol = new(0.094f, 0.102f, 0.118f, 1.0f);
        public static Vector4 TextCol = new(0.274f, 0.317f, 0.450f, 1.0f);
        public static Vector4 HeaderStartCol = TextCol;
        public static Vector4 HeaderEndCol = new(1, 1, 1, 0);
        public static float windowAlpha = 1f;
        public static float animationSpeed = 0.15f;
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
        public static Vector4 ParticleColor = new(1f, 1f, 1f, 1f);
        public static Vector4 LineColor = new(1, 1, 1, 0.33f);
        public static float ParticleRadius = 2.5f;
        public static Vector2 BaseParticlePos = new();
        public static int NumberOfParticles = 50;
        public static Random random = new();
        public static float ParticleSpeed = 0.53f;
        public static List<Vector2> Positions = [];
        public static List<Vector2> Velocities = [];
        public static float MaxLineDistance = 300f;
        public static ImGuiKey OpenKey = ImGuiKey.Insert;

        public static HashSet<Keys> keys =
        [
            Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey,
            Keys.ControlKey, Keys.LControlKey, Keys.RControlKey,
            Keys.Menu, Keys.LMenu, Keys.RMenu
        ];

        public static bool menuSounds = true;
        public static float menuSoundsVolume = 0.8f;
        public static bool enableVsync = true;
        public static Vector2 MainWindowSize = new(860, 550);


        public void UpdateEntities(IEnumerable<Entity> newEntities) => entities = newEntities.ToList();

        public static void LoadFonts()
        {
            try
            {
                if (ImGui.GetCurrentContext() == IntPtr.Zero) ImGui.CreateContext();

                var io = ImGui.GetIO();

                string Base = Path.Combine(AppContext.BaseDirectory, "Resources", "fonts");

                TextFontNormal = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "NotoSans-Bold.ttf"), 18.0f);
                TextFontBig = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "NotoSans-Bold.ttf"), 24.0f);
                TextFont48 = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "NotoSans-Bold.ttf"), 48.0f);
                TextFont60 = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "NotoSans-Bold.ttf"), 60.0f);
                IconFont = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "glyph.ttf"), 18.0f);
                GunIconsFont = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "undefeated.ttf"), 24.0f);

                ushort[] icons = [0xEB54, 0xEB55, 0];
                unsafe
                {
                    fixed (ushort* pIcons = icons)
                        IconFont1 = io.Fonts.AddFontFromFileTTF(Path.Combine(Base, "Lineicons.ttf"), 36.0f, null,
                            (IntPtr)pIcons);
                }

                io.Fonts.Build();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        public void UpdateLocalPlayer(Entity newEntity) // update local player
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        protected override void Render()
        {
            try
            {
                var io = ImGui.GetIO();
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableKeyboard; // keyboard nav
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad; // gamepad nav
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                io.Framerate = 0;
                this.VSync = enableVsync;
                io.ConfigViewportsNoAutoMerge = true;
                io.ConfigViewportsNoTaskBarIcon = true;

                RenderESPOverlay();
                RenderMainWindow();
                RenderWaterMark();
                BombTimerOverlay.TimeOverlay();
                //Library.UpdateNotifications(io.DeltaTime);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void RenderWaterMark()
        {
            if (!EnableWaterMark && !IsTextFontBigLoaded)
                return;

            ImGui.SetNextWindowSize(new(200, 80));
            ImGui.SetNextWindowPos(new(ScreenSize.X / 2, 0));
            ImGui.Begin("wm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar);
            ImGui.PushFont(TextFontBig);
            var drawList = ImGui.GetWindowDrawList();
            Vector2 textPosition = ImGui.GetWindowPos() + new Vector2(20, 20);
            drawList.AddText(ImGui.GetWindowPos() + new Vector2(20, 20), ImGui.ColorConvertFloat4ToU32(accentColor),
                "Titled");
            timeSinceLastUpdate += ImGui.GetIO().DeltaTime;

            if (timeSinceLastUpdate >= fpsUpdateInterval)
            {
                lastFPS = 1f / ImGui.GetIO().DeltaTime;
                timeSinceLastUpdate = 0.0f;
            }

            drawList.AddText(new(textPosition.X, textPosition.Y + 20f), ImGui.ColorConvertFloat4ToU32(TextCol),
                $"FPS: {Math.Round(lastFPS)}");
            drawList.AddText(ImGui.GetWindowPos() + new Vector2(20, 20), ImGui.ColorConvertFloat4ToU32(accentColor),
                "Titled");
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
            drawList = ImGui.GetWindowDrawList();
            var io = ImGui.GetIO();
            io.Framerate = 0;
            io.ConfigViewportsNoAutoMerge = true;
            io.ConfigViewportsNoTaskBarIcon = true;

            //ImGui.ShowMetricsWindow();
            ImGui.End();
        }

        private void RenderMainWindow()
        {
            if (ImGui.IsKeyPressed(OpenKey, false))
                DrawWindow = !DrawWindow;

            BGdrawList = ImGui.GetBackgroundDrawList();
            if (DrawWindow)
            {
                //BGdrawList.AddRectFilled(Vector2.Zero, ScreenSize,
                //    ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f,
                //        0.5f))); // ts the dimmed background TODO: make a opacity changer
                DrawParticles(NumberOfParticles);
                ImGui.SetNextWindowPos(new Vector2((ScreenSize.X - 800) / 2f, (ScreenSize.Y - 600) / 2f),
                    ImGuiCond.Always);

                ImGuiStylePtr style = ImGui.GetStyle();
                style.Alpha = windowAlpha;
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

                //style.Colors[(int)ImGuiCol.ScrollbarBg] = style.Colors[(int)ImGuiCol.WindowBg];
                //style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.15f, 0.17f, 0.20f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.20f, 0.22f, 0.25f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.25f, 0.27f, 0.30f, windowAlpha);
                style.Colors[(int)ImGuiCol.Text] = new(0.274f, 0.317f, 0.450f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TextDisabled] = new(0.274f, 0.317f, 0.450f, windowAlpha);
                style.Colors[(int)ImGuiCol.WindowBg] = new(0.102f, 0.102f, 0.102f, windowAlpha);
                style.Colors[(int)ImGuiCol.ChildBg] = new(0.125f, 0.125f, 0.125f, windowAlpha);
                //style.Colors[(int)ImGuiCol.PopupBg] = new(0.0784f, 0.0862f, 0.101f, windowAlpha);
                ////style.Colors[(int)ImGuiCol.Border] = new(0.156f, 0.168f, 0.192f, windowAlpha);
                ////style.Colors[(int)ImGuiCol.BorderShadow] = new(0.0784f, 0.086f, 0.101f, windowAlpha);
                //style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.113f, 0.125f, 0.152f, windowAlpha);
                //style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.156f, 0.168f, 0.192f, windowAlpha);
                //style.Colors[(int)ImGuiCol.FrameBgActive] = new Vector4(0.156f, 0.168f, 0.192f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.0470f, 0.0549f, 0.0705f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.0470f, 0.0549f, 0.0705f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new Vector4(0.0784f, 0.086f, 0.101f, windowAlpha);
                //style.Colors[(int)ImGuiCol.MenuBarBg] = new Vector4(0.0980f, 0.105f, 0.121f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarBg] = new Vector4(0.047f, 0.054f, 0.070f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.117f, 0.133f, 0.149f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.156f, 0.168f, 0.192f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.117f, 0.133f, 0.149f, windowAlpha);
                //style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.274f, 0.3176f, 0.450f, windowAlpha);
                //style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.274f, 0.317f, 0.450f, windowAlpha);
                //style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.600f, 0.964f, 0.031f, windowAlpha);
                style.Colors[(int)ImGuiCol.Button] = new(0.125f, 0.125f, 0.125f, windowAlpha);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.180f, 0.1882f, 0.196f, windowAlpha);
                style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.152f, 0.152f, 0.152f, windowAlpha);
                //style.Colors[(int)ImGuiCol.Header] = new Vector4(0.141f, 0.164f, 0.207f, windowAlpha);
                //style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.105f, 0.105f, 0.105f, windowAlpha);
                //style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.078f, 0.086f, 0.101f, windowAlpha);
                //style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.129f, 0.149f, 0.192f, windowAlpha);
                //style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.156f, 0.184f, 0.250f, windowAlpha);
                //style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.156f, 0.184f, 0.250f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.145f, 0.145f, 0.145f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.274f, 0.317f, 0.450f, windowAlpha);
                //style.Colors[(int)ImGuiCol.ResizeGripActive] = new Vector4(windowAlpha, windowAlpha, windowAlpha, windowAlpha);
                //style.Colors[(int)ImGuiCol.Tab] = new Vector4(0.078f, 0.086f, 0.101f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TabHovered] = new Vector4(0.117f, 0.133f, 0.149f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TabActive] = new Vector4(0.117f, 0.133f, 0.149f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TabUnfocused] = new Vector4(0.078f, 0.086f, 0.101f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new Vector4(0.125f, 0.274f, 0.572f, windowAlpha);
                //style.Colors[(int)ImGuiCol.PlotLines] = new Vector4(0.521f, 0.600f, 0.701f, windowAlpha);
                //style.Colors[(int)ImGuiCol.PlotLinesHovered] = new Vector4(0.039f, 0.980f, 0.980f, windowAlpha);
                //style.Colors[(int)ImGuiCol.PlotHistogram] = new Vector4(0.031f, 0.949f, 0.843f, windowAlpha);
                //style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new Vector4(0.156f, 0.184f, 0.2509f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TableHeaderBg] = new Vector4(0.0470f, 0.054f, 0.0705f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TableBorderStrong] = new Vector4(0.047f, 0.054f, 0.0705f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TableBorderLight] = new Vector4(0.0f, 0.0f, 0.0f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TableRowBg] = new Vector4(0.117f, 0.133f, 0.149f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TableRowBgAlt] = new Vector4(0.098f, 0.105f, 0.121f, windowAlpha);
                //style.Colors[(int)ImGuiCol.TextSelectedBg] = new Vector4(0.180f, 0.188f, 0.196f, windowAlpha);
                //style.Colors[(int)ImGuiCol.DragDropTarget] = new Vector4(0.498f, 0.513f, windowAlpha, windowAlpha);
                //style.Colors[(int)ImGuiCol.NavHighlight] = new Vector4(0.266f, 0.290f, windowAlpha, windowAlpha);
                //style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new Vector4(0.498f, 0.513f, windowAlpha, windowAlpha);
                //style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new Vector4(0.196f, 0.176f, 0.545f, 0.501f);
                //style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new Vector4(0.196f, 0.176f, 0.545f, 0.501f);

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.Begin("",
                    ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar |
                    ImGuiWindowFlags.NoDocking);
                ImGui.SetWindowSize(MainWindowSize);

                Vector2 tabPos = ImGui.GetCursorScreenPos();
                var availableSpace = ImGui.GetContentRegionAvail();
                var availableHeight = availableSpace.Y;
                var availableWidth = availableSpace.X;

                tabSize = new(100, ImGui.GetContentRegionAvail().Y);
                drawList.AddRectFilled(tabPos, tabPos + tabSize, ImGui.ColorConvertFloat4ToU32(new(0.125f, 0.125f, 0.125f, windowAlpha)), 12.0f,
                    ImDrawFlags.RoundCornersLeft);

                ImGui.BeginChild("Sidebar", tabSize, ImGuiChildFlags.None);
                {
                    const float logoWidth = 120f;
                    float offset = (ImGui.GetContentRegionAvail().X - logoWidth) * 0.5f;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);
                    byte[] LogoBytes = Convert.FromBase64String(menuImage);
                    Image<Rgba32> LogoImage = Image.Load<Rgba32>(LogoBytes);

                    AddOrGetImagePointer("MenuLogo", LogoImage, true, out _menuLogoTexture);

                    ImGui.Image(_menuLogoTexture, new(120, 120));
                    ImGui.Spacing();

                    ImGui.Separator();
                    ImGui.Spacing();

                    RenderTabButton("E", 0);
                    RenderTabButton("D", 1);
                    RenderTabButton("C", 2);
                    RenderTabButton("\uEB54", 3);

                    const float cogButtonHeight = 35f;
                    var spacingHeight = availableHeight - cogButtonHeight - 5f;

                    if (spacingHeight > 0)
                        ImGui.Dummy(new(0, spacingHeight));


                    Vector2 cogPos = ImGui.GetCursorScreenPos();
                    Vector2 cogSize = new(ImGui.GetContentRegionAvail().X, cogButtonHeight);

                    if (ImGui.InvisibleButton("##SettingsGear", cogSize))
                        selectedTab = 4;


                    bool isHovered = ImGui.IsItemHovered();
                    bool isSettingsSelected = selectedTab == 4;

                    Vector2 gearCenter = new(cogPos.X + cogSize.X / 2, cogPos.Y + cogSize.Y / 2);

                    uint gearColor;
                    if (isSettingsSelected)
                        gearColor = ImGui.ColorConvertFloat4ToU32(accentColor);

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

                drawList.AddRectFilled(mainPos, mainPos + mainSize,
                    ImGui.ColorConvertFloat4ToU32(new(0.094f, 0.102f, 0.118f, windowAlpha)), 12.0f,
                    ImDrawFlags.RoundCornersBottom);

                ImGui.BeginChild("MainContent", mainSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    ImGui.PopStyleVar();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12.0f);
                    //RenderTitle("Titled");
                    switch (selectedTab)
                    {
                        case 0:
                        case 1:
                        case 2:
                        case 4:
                            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 4f);
                            float availW = ImGui.GetContentRegionAvail().X;
                            float sectionW = (availW - ImGui.GetStyle().ItemSpacing.X) / 2f - 6f;
                            int col = 0;
                            foreach (var s in Sections.sections)
                            {
                                if (s.tab != selectedTab) 
                                    continue;

                                if (col % 2 != 0)
                                    ImGui.SameLine();

                                else ImGui.Dummy(new Vector2(0, 4));
                                Sections.BeginSection(s.label, s.content, new Vector2(sectionW, 0));
                                col++;
                            }
                            break;

                        case 3: // config
                            ImGui.Columns(2, "ConfigColum", true);
                            ImGui.BeginChild("ConfigLeft");
                            ImGui.Text("Available Configs:");

                            ImGui.BeginChild("ConfigList", new(0, 200), ImGuiChildFlags.Border);
                            {
                                foreach (var config in Configs.SavedConfigs.Keys)
                                {
                                    if (ImGui.Selectable(config))
                                        Configs.SelectedConfig = config;
                                }
                            }
                            ImGui.EndChild();
                            ImGui.Spacing();

                            ImGui.EndChild();
                            ImGui.NextColumn();

                            ImGui.BeginChild("ConfigRight");
                            ImGui.InputText("Config Name", ref Configs.ConfigName, 24);
                            if (ImGui.Button("Save Config", new Vector2(120, 30)))
                            {
                                Configs.SaveConfig(Configs.ConfigName);
                                if (!Configs.SavedConfigs.ContainsKey(Configs.ConfigName))
                                {
                                    Configs.SavedConfigs.TryAdd(Configs.ConfigName, false);
                                }
                                else
                                {
                                    Console.WriteLine("Config Already Exists.");
                                }
                            }

                            ImGui.SameLine();
                            if (ImGui.Button("Load Config", new Vector2(120, 30)))
                            {
                                Configs.LoadConfig(Configs.SelectedConfig);
                            }

                            ImGui.EndChild();
                            ImGui.Columns(1);
                            break;
                    }
                }
                ImGui.EndChild();

                ImGui.End();
            }

            RunAllModules();
        }

        public void DrawParticles(int num)
        {
            while (Positions.Count < num || Velocities.Count < num) // only add if there isnt eg 50 drawn
            {
                Positions.Add(new Vector2(random.Next((int)ScreenSize.X), random.Next((int)ScreenSize.Y)));
                Velocities.Add(new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)));
            }

            for (int i = 0; i < num; i++)
            {
                Positions[i] += Velocities[i] * ParticleSpeed;

                if (Positions[i].X < 0 || Positions[i].X > ScreenSize.X || Positions[i].Y < 0 ||
                    Positions[i].Y > ScreenSize.Y)
                {
                    Positions[i] = new Vector2(random.Next((int)ScreenSize.X), random.Next((int)ScreenSize.Y));
                    Velocities[i] = new Vector2((float)(random.NextDouble() * 2 - 1),
                        (float)(random.NextDouble() * 2 - 1));
                }

                DrawHelpers.DrawGlowCircleFilled(drawList, Positions[i], ParticleRadius, ParticleColor, 1.1f);
            }

            for (int i = 0; i < num; i++) // lines
            {
                for (int j = i + 1; j < num; j++)
                {
                    float dist = Vector2.Distance(Positions[i], Positions[j]);
                    if (dist < MaxLineDistance)
                    {
                        float alpha = 1f - (dist / MaxLineDistance);
                        drawList.AddLine(Positions[i], Positions[j],
                            ImGui.ColorConvertFloat4ToU32(new Vector4(LineColor.X, LineColor.Y, LineColor.Z,
                                LineColor.W * alpha)), 1f);
                    }
                }
            }
        }

        public void RunAllModules()
        {
            try
            {
                if (Aimbot.TargetLine)
                    Aimbot.RenderTargetLine();
                HitStuff.CreateHitText();

                if (EyeRay.Enabled)
                    EyeRay.DrawEyeRay();


                foreach (var entity in entities)
                {
                    if (entity == null)
                        continue;

                    Modules.Visual.BoneESP.DrawBoneLines(entity, this);
                    NameDisplay.DrawName(entity, this);
                    PingDisplay.DrawPing(entity, this);
                    Chams.Draw(entity);
                    GunDisplay.Draw(entity);
                    BoxESP.DrawBoxESP(entity, this);
                    Titled_Gui.Modules.Visual.DistanceText.DrawDistance(entity);
                    Tracers.DrawTracers(entity, this);

                    var rect = BoxESP.GetBoxRect(entity);
                    if (rect == null)
                        continue;

                    var (topLeft, bottomRight, topRight, bottomLeft, bottomMiddle) = rect.Value;
                    Vector2 barTopLeft = new(topLeft.X - HealthBar.HealthBarWidth - 2, topLeft.Y);
                    float height = bottomRight.Y - topLeft.Y;

                    HealthBar.DrawHealthBar(entity, entity.Health, 100, barTopLeft, height);
                    ArmorBar.DrawArmorBar(entity, this, entity.Armor, 100);
                }


                if (Aimbot.DrawFov && Aimbot.AimbotEnable && Aimbot.UseFOV)
                    Aimbot.DrawCircle(Aimbot.FovSize, Aimbot.FovColor);

                if (C4ESP.BoxEnabled || C4ESP.TextEnabled)
                {
                    C4ESP.DrawESP();
                }

                WorldESP.EntityESP();
                Radar.DrawRadar();

                SoundESP.Draw();
                //GernadeHelper.DrawAllLineups();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void DrawGearIcon(Vector2 center, uint color)
        {
            if (!IsIconFont1Loaded) return;

            ImGui.PushFont(IconFont1);

            Vector2 textSize = ImGui.CalcTextSize("\uEAF5");
            Vector2 textPos = new(center.X - textSize.X / 2, center.Y - textSize.Y / 2);

            ImGui.GetWindowDrawList().AddText(textPos, color, "\uEAF5");

            ImGui.PopFont();
        }

        public static void RenderTitle(string text)
        {
            if (!IsTextFontBigLoaded) return;

            ImGui.PushFont(TextFontBig);

            Vector2 offsetPos = new(ImGui.GetCursorScreenPos().X + 4, ImGui.GetCursorScreenPos().Y + 2);

            ImGui.GetWindowDrawList().AddText(offsetPos, ImGui.ColorConvertFloat4ToU32(TextCol), text);

            ImGui.PopFont();

            Vector2 textSize = ImGui.CalcTextSize(text);

            ImGui.Dummy(new Vector2(0, textSize.Y + 8));

            // sep line
            Vector2 start = ImGui.GetCursorScreenPos();
            Vector2 end = new(start.X + ImGui.GetContentRegionAvail().X, start.Y);
            ImGui.GetWindowDrawList().AddLine(start, end,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.17f, 0.20f, windowAlpha)), 1.0f);

            ImGui.Dummy(new Vector2(0, 6)); // spacing below
        }





        private static void RenderCategoryHeader(string categoryName)
        {
            Vector2 textSize = ImGui.CalcTextSize(categoryName);

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 childSize = ImGui.GetContentRegionAvail();

            Vector2 rectPos = new(cursorPos.X, cursorPos.Y);
            Vector2 rectSize = new(childSize.X / 2, textSize.Y + 8.3f); // half of the child

            ImGui.GetWindowDrawList().AddRectFilledMultiColor(rectPos, rectPos + rectSize,
                ImGui.ColorConvertFloat4ToU32(HeaderStartCol), ImGui.ColorConvertFloat4ToU32(MainContentCol),
                ImGui.ColorConvertFloat4ToU32(MainContentCol), ImGui.ColorConvertFloat4ToU32(HeaderStartCol));

            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + 1 / 2);

            if (Renderer.IsTextFontBigLoaded)
            {
                ImGui.PushFont(Renderer.TextFontBig);
                RenderGradientText(categoryName, new(0, 0, 0, 1f), new(0, 0, 0, 1f));
                //RenderGradientText(categoryName, new(0.078f, 0.0862f, 0.101f, 1f), Renderer.accentColor);
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
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0)); // no spacing between tabs
            bool isSelected = selectedTab == tabIndex;

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

            bool pressed;
            if (label == "\uEB54")
            {
                ImGui.PushFont(IconFont1);
                if (isSelected)
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                    ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                }

                pressed = ImGui.Button(label, new Vector2(tabSize.X, 40));
                if (pressed)
                {
                    selectedTab = tabIndex;
                    if (menuSounds)
                        Classes.PlaySound.PlaySoundFile("ClickSounds/Creamy.wav", menuSoundsVolume);
                }

                ImGui.PopStyleColor(isSelected ? 4 : 1);
                ImGui.PopFont();
            }
            else
            {
                pressed = ImGui.Button(label, new Vector2(tabSize.X, 40));
                if (pressed)
                {
                    selectedTab = tabIndex;
                    if (menuSounds)
                        Classes.PlaySound.PlaySoundFile("ClickSounds/Creamy.wav", menuSoundsVolume);
                }
            }

            var min = ImGui.GetItemRectMin();
            var max = ImGui.GetItemRectMax();
            var borderColor = isSelected ? new Vector4(0.125f, 0.125f, 0.125f, windowAlpha) + new Vector4(0.02f, 0.01f, 0.01f, 1f) : new(0.125f, 0.125f, 0.125f, windowAlpha);


            ImGui.PopFont();
            ImGui.PopStyleVar(); // restore spacing
        }

        //private static void RenderIntCombo(string label, ref int current, string[] items, int itemCount, float widgetWidth = 160f)
        //{
        //    int temp = current;

        //    RenderRowRightAligned(label, () =>
        //    {
        //        ImGui.Combo("##" + label, ref temp, items, items.Length);
        //    }, widgetWidth);

        //    if (temp != current)
        //    {
        //        current = temp;
        //    }
        //}

        public static void RenderKeybindChooser(string label, ref int key)
        {
            ImGui.PushID(label);

            if (!KeyBind.ContainsKey(label)) KeyBind[label] = false;

            if (ImGui.Button(KeyBind[label] ? "Press Any Key..." : (key == (int)Keys.None ? "None" : Enum.GetName(typeof(Keys), key) ?? key.ToString()), new Vector2(100, 0))) KeyBind[label] = true;

            if (KeyBind[label])
            {
                foreach (Keys k in Enum.GetValues<Keys>())
                {
                    if (k == Keys.None || k == Keys.Escape) continue;

                    short state = User32.GetAsyncKeyState((int)k);
                    bool pressed = (state & 0x8000) != 0;

                    if (!pressed) continue;

                    if (k == Keys.Escape) key = (int)Keys.None;

                    else key = (int)k;

                    KeyBind[label] = false;
                    break;
                }
            }

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
        }


        public static Dictionary<string, bool> KeyBind = [];

        public static void RenderKeybindChooser(string label, ref ImGuiKey key)
        {
            ImGui.PushID(label);

            KeyBind.TryAdd(label, false);

            string keyName = KeyBind[label] ? "Press Any Key..." : (key == ImGuiKey.None ? "None" : key.ToString());

            if (ImGui.Button(keyName, new Vector2(100, 0)))
                KeyBind[label] = true;


            if (KeyBind[label])
            {
                foreach (ImGuiKey imguiKey in Enum.GetValues<ImGuiKey>())
                {
                    if (!ImGui.IsKeyPressed(imguiKey))
                        continue;
                    if (imguiKey >= ImGuiKey.MouseLeft && imguiKey <= ImGuiKey.MouseWheelY) continue;

                    if (imguiKey == ImGuiKey.Escape)
                        key = ImGuiKey.Insert;

                    else
                        key = imguiKey;


                    KeyBind[label] = false;
                    break;
                }
            }

            ImGui.SameLine();
            ImGui.Text(label);

            ImGui.PopID();
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
