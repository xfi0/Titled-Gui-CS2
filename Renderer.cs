using ClickableTransparentOverlay;
using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Legit;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui
{
    public class Renderer : Overlay
    {
        public Vector2 screenSize = new(Screen.PrimaryScreen!.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        private IntPtr menuLogoTexture;
        private uint Width;
        private uint Height;

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
        public static Vector4 trackCol = new(0.18f, 0.18f, 0.20f, 1f);
        public static Vector4 knobOff = new(0.15f, 0.15f, 0.15f, 1f);
        public static Vector4 knobOn = new(0.2745f, 0.3176f, 0.4510f, 1.0f);
        public static bool IsIconFont1Loaded => !IconFont1.Equals(default(ImFontPtr));
        public static bool IsGunIconFontLoaded => !GunIconsFont.Equals(default(ImFontPtr));
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
        public static HashSet<Keys> keys =
        [
           Keys.ShiftKey, Keys.LShiftKey, Keys.RShiftKey,
           Keys.ControlKey, Keys.LControlKey, Keys.RControlKey,
           Keys.Menu, Keys.LMenu, Keys.RMenu
        ];
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
                        IconFont1 = io.Fonts.AddFontFromFileTTF("..\\..\\..\\..\\Resources\\fonts\\Lineicons.ttf", 36.0f, null, (IntPtr)pIcons);
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
                io.ConfigFlags |= ImGuiConfigFlags.NavEnableGamepad;  // gamepad nav
                io.ConfigFlags |= ImGuiConfigFlags.ViewportsEnable;
                io.Framerate = 0;
                io.ConfigViewportsNoAutoMerge = true;
                io.ConfigViewportsNoTaskBarIcon = true;

                RenderESPOverlay();
                RenderMainWindow();
                RenderWaterMark();
                BombTimerOverlay.TimeOverlay();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public void RenderWaterMark()
        {
            if (EnableWaterMark && IsTextFontBigLoaded)
            {
                ImGui.SetNextWindowSize(new(200, 80));
                ImGui.SetNextWindowPos(new(screenSize.X / 2, 0));
                ImGui.Begin("wm", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.NoTitleBar);
                ImGui.PushFont(TextFontBig);
                var Stlye = ImGui.GetStyle();
                Stlye.WindowRounding = 12f;
                var DrawList = ImGui.GetWindowDrawList();
                Vector2 TextPosition = ImGui.GetWindowPos() + new Vector2(20, 20);
                DrawList.AddText(ImGui.GetWindowPos() + new Vector2(20, 20), ImGui.ColorConvertFloat4ToU32(accentColor), "Titled");
                timeSinceLastUpdate += ImGui.GetIO().DeltaTime;

                if (timeSinceLastUpdate >= fpsUpdateInterval)
                {
                    lastFPS = 1f / ImGui.GetIO().DeltaTime;
                    timeSinceLastUpdate = 0.0f;
                }

                DrawList.AddText(new(TextPosition.X, TextPosition.Y + 20f), ImGui.ColorConvertFloat4ToU32(TextCol), $"FPS: {Math.Round(lastFPS)}");
                DrawList.AddText(ImGui.GetWindowPos() + new Vector2(20, 20), ImGui.ColorConvertFloat4ToU32(accentColor), "Titled");
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
            var io = ImGui.GetIO();
            io.Framerate = 0;
            io.ConfigViewportsNoAutoMerge = true;
            io.ConfigViewportsNoTaskBarIcon = true;

            //ImGui.ShowMetricsWindow();
            ImGui.End();
        }

        private void RenderMainWindow()
        {
            // toggle on tab
            if (ImGui.IsKeyPressed(OpenKey, false))
                DrawWindow = !DrawWindow;
            
            BGdrawList = ImGui.GetBackgroundDrawList();
            if (DrawWindow)
            {
                BGdrawList.AddRectFilled(Vector2.Zero, screenSize, ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 0f, 0f, 0.5f)));
                DrawParticles(NumberOfParticles);
                ImGui.SetNextWindowPos(new Vector2((screenSize.X - 800) / 2f, (screenSize.Y - 600) / 2f), ImGuiCond.Always);

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

                style.Colors[(int)ImGuiCol.ScrollbarBg] = style.Colors[(int)ImGuiCol.WindowBg];
                style.Colors[(int)ImGuiCol.ScrollbarGrab] = new Vector4(0.15f, 0.17f, 0.20f, 1.0f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new Vector4(0.20f, 0.22f, 0.25f, 1.0f);
                style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new Vector4(0.25f, 0.27f, 0.30f, 1.0f);
                style.Colors[(int)ImGuiCol.Text] = new(0.274f, 0.317f, 0.450f, 1.0f);
                style.Colors[(int)ImGuiCol.TextDisabled] = new(0.2745098173618317f, 0.3176470696926117f, 0.4501f, 1.0f);
                style.Colors[(int)ImGuiCol.WindowBg] = new(0.078f, 0.0862f, 0.101f, 1.0f);
                style.Colors[(int)ImGuiCol.ChildBg] = new(0.09411764889955521f, 0.1019607856869698f, 0.1176470592617989f, 1.0f);
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
                style.Colors[(int)ImGuiCol.CheckMark] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
                style.Colors[(int)ImGuiCol.SliderGrab] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
                style.Colors[(int)ImGuiCol.SliderGrabActive] = new Vector4(0.6000000238418579f, 0.9647058844566345f, 0.0313725508749485f, 1.0f);
                style.Colors[(int)ImGuiCol.Button] = SidebarColor;
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.1803921610116959f, 0.1882352977991104f, 0.196078434586525f, 1.0f);
                style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.1529411822557449f, 0.1529411822557449f, 0.1529411822557449f, 1.0f);
                style.Colors[(int)ImGuiCol.Header] = new Vector4(0.1411764770746231f, 0.1647058874368668f, 0.2078431397676468f, 1.0f);
                style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(0.105882354080677f, 0.105882354080677f, 0.105882354080677f, 1.0f);
                style.Colors[(int)ImGuiCol.HeaderActive] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
                style.Colors[(int)ImGuiCol.Separator] = new Vector4(0.1294117718935013f, 0.1490196138620377f, 0.1921568661928177f, 1.0f);
                style.Colors[(int)ImGuiCol.SeparatorHovered] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
                style.Colors[(int)ImGuiCol.SeparatorActive] = new Vector4(0.1568627506494522f, 0.1843137294054031f, 0.250980406999588f, 1.0f);
                style.Colors[(int)ImGuiCol.ResizeGrip] = new Vector4(0.1450980454683304f, 0.1450980454683304f, 0.1450980454683304f, 1.0f);
                style.Colors[(int)ImGuiCol.ResizeGripHovered] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4509803950786591f, 1.0f);
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

                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.Begin("", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoDocking);
                ImGui.SetWindowSize(new(800, 600));

                Vector2 tabPos = ImGui.GetCursorScreenPos();
                tabSize = new(100, ImGui.GetContentRegionAvail().Y);
                var drawList = ImGui.GetWindowDrawList();
                drawList.AddRectFilled(tabPos, tabPos + tabSize, ImGui.ColorConvertFloat4ToU32(SidebarColor), 12.0f, ImDrawFlags.RoundCornersLeft);

                ImGui.BeginChild("Sidebar", tabSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    float LogoWidth = 120;
                    float offset = (ImGui.GetContentRegionAvail().X - LogoWidth) * 0.5f;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

                    AddOrGetImagePointer("..\\..\\..\\..\\Resources\\MenuLogo.png", true, out menuLogoTexture, out Width, out Height);
                    ImGui.Image(menuLogoTexture, new(120, 120));
                    ImGui.Spacing();

                    ImGui.Separator();
                    ImGui.Spacing();

                    RenderTabButton("E", 0);
                    RenderTabButton("D", 1);
                    RenderTabButton("C", 2);
                    RenderTabButton("\uEB54", 3);

                    var availableHeight = ImGui.GetContentRegionAvail().Y;
                    var cogButtonHeight = 35f;
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

                drawList.AddRectFilled(mainPos, mainPos + mainSize, ImGui.ColorConvertFloat4ToU32(new(0.094f, 0.102f, 0.118f, 1.0f)), 12.0f, ImDrawFlags.RoundCornersBottom);

                ImGui.BeginChild("MainContent", mainSize, ImGuiChildFlags.None, ImGuiWindowFlags.NoBackground);
                {
                    ImGui.PopStyleVar();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
                    ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 12.0f);
                    RenderTitle("Titled");
                    switch (selectedTab)
                    {
                        case 0: // legit
                            ImGui.Columns(2, "Legit Columns", true);
                            ImGui.BeginChild("LeftLegit");
                            //RenderBoolSetting("Auto BHOP", ref Modules.Legit.Bhop.BhopEnable);
                            //RenderBoolSetting("Jump Shot", ref Modules.Legit.JumpHack.JumpHackEnabled);
                            RenderBoolSetting("Hit Sound", ref HitStuff.Enabled);
                            RenderFloatSlider("Hit Sound Volume", ref HitStuff.Volume, 0, 1);
                            RenderIntCombo("Current Hit Sound", ref HitStuff.CurrentHitSound, HitStuff.HitSounds, HitStuff.HitSounds.Length);
                            RenderBoolSettingWith1ColorPicker("Headshot Text", ref HitStuff.EnableHeadshotText, ref HitStuff.TextColor);

                            RenderBoolSetting("Jump Shot", ref Modules.Legit.JumpHack.JumpHackEnabled);
                            RenderKeybindChooser("Jump Shot Keybind", ref JumpHack.JumpHotkey);

                            ImGui.NextColumn();
                            ImGui.EndChild();

                            ImGui.BeginChild("RightLegit");
                            ImGui.EndChild();

                            ImGui.Columns(1);
                            break;

                        case 1: // aim
                            ImGui.Columns(2, "TriggerColumns", true);

                            ImGui.BeginChild("LeftAim");
                            RenderBoolSetting("Enable Aimbot", ref Modules.Rage.Aimbot.AimbotEnable);

                            RenderSettingsSection("Aimbot Settings", () =>
                            {
                                RenderIntCombo("Aim Bone", ref Aimbot.CurrentBone, Aimbot.Bones, Aimbot.Bones.Length, 160f); // bone that aimbot aims on
                                RenderIntCombo("Aim Method", ref Aimbot.CurrentAimMethod, Aimbot.AimbotMethods, Aimbot.AimbotMethods.Length);
                                RenderKeybindChooser("Aimbot Keybind", ref Aimbot.AimbotKey);
                                RenderBoolSetting("Aim On Team", ref Modules.Rage.Aimbot.Team);
                                RenderFloatSlider("Smoothing X", ref Aimbot.SmoothingX, 0, 20, "%.2f");
                                RenderFloatSlider("Smoothing Y", ref Aimbot.SmoothingY, 0, 20, "%.2f");
                                RenderBoolSetting("Draw FOV", ref Aimbot.DrawFov);
                                RenderBoolSetting("Use FOV", ref Aimbot.UseFOV);
                                RenderBoolSetting("Scoped Check", ref Aimbot.ScopedOnly);
                                RenderIntSlider("FOV Size", ref Modules.Rage.Aimbot.FovSize, 10, 1000, "%d");
                                RenderColorSetting("FOV Color", ref Modules.Rage.Aimbot.FovColor);
                                RenderBoolSetting("Visibility Check", ref Aimbot.VisibilityCheck);
                            });

                            RenderBoolSetting("RCS", ref RCS.Enabled);
                            ImGui.EndChild();

                            ImGui.NextColumn();

                            ImGui.BeginChild("RightRage");
                            RenderBoolSetting("Triggerbot", ref Modules.Rage.TriggerBot.Enabled);
                            RenderKeybindChooser($"Trigger Bot Keybind", ref TriggerBot.TriggerKey);
                            RenderIntSlider("Max Delay", ref Modules.Rage.TriggerBot.MaxDelay, 0, 1000, "%d");
                            RenderIntSlider("Min Delay", ref Modules.Rage.TriggerBot.MinDelay, 0, 1000, "%d");
                            RenderBoolSetting("Require Key bind", ref Modules.Rage.TriggerBot.RequireKeybind);
                            //RenderBoolSetting("Team Check", ref TriggerBot.TeamCheck);
                            ImGui.EndChild();

                            ImGui.Columns(1);
                            break;

                        case 2: // visuals
                            ImGui.Columns(2, "VisualsColumns", true);

                            ImGui.BeginChild("LeftVisuals");
                            RenderBoolSetting("Enable ESP", ref BoxESP.EnableESP);

                            //RenderSettingsSection("ESP Settings", () =>
                            //{
                            RenderIntCombo("ESP Shape", ref BoxESP.CurrentShape, BoxESP.Shapes, BoxESP.Shapes.Length); // shape dropdown
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
                            //});
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
                            //RenderFloatSlider("Bone Thickness", ref BoneESP.BoneThickness, 1f, 10f, "%.1f");
                            RenderColorSetting("Bone Color", ref BoneESP.BoneColor);
                            RenderBoolSetting("Enable RGB", ref Colors.RGB);
                            RenderFloatSlider("Bone Glow", ref BoneESP.GlowAmount, 0, 1f);
                            //RenderBoolSetting("Enable Chams", ref Modules.Visual.Chams.EnableChams);
                            //if (Modules.Visual.Chams.EnableChams)
                            //{
                            //    RenderSettingsSection("Chams Settings", () =>
                            //    {
                            //        RenderBoolSetting("Draw On Self", ref Chams.DrawOnSelf);
                            //        RenderBoolSetting("Draw On Self", ref Modules.Visual.Chams.DrawOnSelf);
                            //        RenderFloatSlider("Bone Thickness", ref Chams.BoneThickness, 1f, 20f, "%.1f");
                            //        RenderBoolSetting("Enable RGB", ref Colors.RGB);
                            //    });
                            //}
                            RenderBoolSetting("Eye Ray", ref EyeRay.Enabled);
                            RenderBoolSettingWith1ColorPicker("Gun Icon", ref GunDisplay.Enabled, ref GunDisplay.TextColor);
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
                            ImGui.EndChild();

                            ImGui.EndChild();

                            ImGui.Columns(1);
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
                                    {
                                        Configs.SelectedConfig = config;
                                    }
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


                        case 4: // settings
                            ImGui.Columns(2, "SettingsColumn", true);

                            ImGui.BeginChild("LeftColumn");

                            ImGui.Text("GUI Settings");
                            ImGui.Spacing();

                            RenderFloatSlider("Window Alpha", ref windowAlpha, 0.1f, 1.0f, "%.2f");
                            RenderColorSetting("Accent Color", ref accentColor);
                            RenderFloatSlider("Animation Speed", ref animationSpeed, 0.01f, 1.0f, "%.2f"); // TODO make like open and close anim maybe tab swtichs
                            RenderFloatSlider("Particle Speed", ref ParticleSpeed, 0, 10);
                            RenderColorSetting("Particle Color", ref ParticleColor);
                            RenderColorSetting("Line Color", ref LineColor);
                            ImGui.Text("Keybinds:");
                            RenderKeybindChooser("Open Keybind", ref OpenKey);
                            ImGui.EndChild();

                            ImGui.NextColumn();

                            ImGui.BeginChild("RightColumn");
                            ImGui.Spacing();
                            ImGui.Spacing();

                            ImGui.Text("About:");
                            ImGui.Text($"Titled GUI V{Configs.Version}");
                            ImGui.Text("External Cheat Made By xfi0 / domok.");

                            ImGui.EndChild();

                            ImGui.Columns(1);
                            break;
                    }
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
        public void DrawParticles(int num)
        {
            while (Positions.Count < num || Velocities.Count < num) // only add if there isnt eg 50 drawn
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
                        drawList.AddLine(Positions[i], Positions[j], ImGui.ColorConvertFloat4ToU32(new Vector4(LineColor.X, LineColor.Y, LineColor.Z, LineColor.W * alpha)), 1f);
                    }
                }
            }
        }



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
                        //Vector2 barPos = new(entity.Head2D.X);
                        //float EntityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;

                        //ArmorBar.DrawArmorBar(this, entity.Armor, 100, barPos, EntityHeight, entity);
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

            ImGui.GetWindowDrawList().AddText(offsetPos, ImGui.ColorConvertFloat4ToU32(TextCol), Text);

            ImGui.PopFont();

            Vector2 textSize = ImGui.CalcTextSize(Text);

            ImGui.Dummy(new Vector2(0, textSize.Y + 8));

            // sep line
            Vector2 start = ImGui.GetCursorScreenPos();
            Vector2 end = new(start.X + ImGui.GetContentRegionAvail().X, start.Y);
            ImGui.GetWindowDrawList().AddLine(start, end, ImGui.ColorConvertFloat4ToU32(new Vector4(0.15f, 0.17f, 0.20f, 1.0f)), 1.0f);

            ImGui.Dummy(new Vector2(0, 6)); // spacing below
        }





        private static void RenderCategoryHeader(string categoryName)
        {
            Vector2 textSize = ImGui.CalcTextSize(categoryName);

            Vector2 cursorPos = ImGui.GetCursorScreenPos();
            Vector2 childSize = ImGui.GetContentRegionAvail();

            Vector2 rectPos = new(cursorPos.X, cursorPos.Y);
            Vector2 rectSize = new(childSize.X / 2, textSize.Y + 8.3f); // half of the child

            ImGui.GetWindowDrawList().AddRectFilledMultiColor( rectPos, rectPos + rectSize, ImGui.ColorConvertFloat4ToU32(HeaderStartCol), ImGui.ColorConvertFloat4ToU32(MainContentCol), ImGui.ColorConvertFloat4ToU32(MainContentCol), ImGui.ColorConvertFloat4ToU32(HeaderStartCol));

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


        private static void RenderLeftRightLableThing(string label, Action renderWidget)
        {
            ImGui.Columns(2, null, false);

            // left
            ImGui.SetColumnWidth(0, 200);
            ImGui.Text(label);
            ImGui.NextColumn();

            // right
            float widgetWidth = ImGui.CalcItemWidth();
            float availWidth = ImGui.GetColumnWidth() - ImGui.GetStyle().ItemSpacing.X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + availWidth - widgetWidth);

            renderWidget();
            ImGui.NextColumn();

            ImGui.Columns(1);
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
            var borderColor =isSelected ? SidebarColor + new Vector4(0.02f, 0.01f, 0.01f, 1f) : SidebarColor;
            drawList.AddLine(new Vector2(min.X, min.Y), new Vector2(max.X, min.Y), ImGui.ColorConvertFloat4ToU32(borderColor), 1.5f); 
            drawList.AddLine(new Vector2(min.X, max.Y), new Vector2(max.X, max.Y), ImGui.ColorConvertFloat4ToU32(borderColor), 1.5f);

            ImGui.PopFont();
            ImGui.PopStyleVar(); // restore spacing
        }



        private static void RenderRow(string label, Action renderWidget, bool rightAlignWidget = false)
        {
            ImGui.Columns(2, null, false);

            ImGui.Indent(LabelPadding);
            ImGui.Text(label);
            ImGui.Unindent(LabelPadding);
            ImGui.NextColumn();

            float colWidth = ImGui.GetColumnWidth();
            float spacing = ImGui.GetStyle().ItemSpacing.X;
            float widgetWidth = WidgetColumnWidth;

            if (rightAlignWidget)
            {
                float posX = ImGui.GetCursorPosX() + colWidth - widgetWidth - spacing;
                ImGui.SetCursorPosX(posX);
            }

            ImGui.PushItemWidth(widgetWidth);
            renderWidget();
            ImGui.PopItemWidth();

            ImGui.NextColumn();
            ImGui.Columns(1);
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

                drawList.AddRectFilled(p, new Vector2(p.X + width, p.Y + height), ImGui.ColorConvertFloat4ToU32(trackCol), height); // track

                float knobX = p.X + radius + t * (width - radius * 2f) + (t == 0f ? 2f : -2f);
                float knobY = p.Y + radius + 2f;
                Vector4 knobColor = temp ? knobOn : knobOff;
                // knob
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

            if (ImGui.Button(KeyBind[label] ? "Press Any Key..." : (key == (int)Keys.None ? "None" : Enum.GetName(typeof(Keys), key) ?? key.ToString()), new Vector2(100, 0))) KeyBind[label] = true;

            if (KeyBind[label])
            {
                foreach (Keys k in Enum.GetValues<Keys>())
                {
                    if (k == Keys.None || k == Keys.Escape) continue;
                    if (keys.Contains(k)) continue;

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
                        if (imguiKey >= ImGuiKey.MouseLeft && imguiKey <= ImGuiKey.MouseWheelY) continue;

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
    }
}