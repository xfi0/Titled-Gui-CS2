using ClickableTransparentOverlay;
using ImGuiNET;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Titled_Gui.Data;
using Titled_Gui.Classes;
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
        public List<Entity> entities = new List<Entity>();
        public Entity localPlayer = new();
        private readonly object entityLock = new();

        private bool tabWasPressed = false;
        private int selectedTab = 0; // 0 = legit, 1 = rage, 2 = visuals, 3 = config, 4 = settings

        public ImDrawListPtr drawList;

        public static bool DrawWindow = true;
        private static bool isWaitingForKey = false;
        private static string keybindLabel = $"Keybind: {Modules.Rage.TriggerBot.TriggerKey}";

        public static Vector4 accentColor = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        public static float windowAlpha = 1f;
        private float animationSpeed = 0.15f;

        public void UpdateEntities(IEnumerable<Entity> newEntities)
        {
            entities = newEntities.ToList();
        }
        public static class FontManager
        {
            public static ImFontPtr BoldFont;
            public static ImFontPtr IconFont;

            public static bool IsBoldFontLoaded => !BoldFont.Equals(default(ImFontPtr));
            public static bool IsIconFontLoaded => !IconFont.Equals(default(ImFontPtr));

            public static void LoadFonts() // TODO make a font changer in settings
            {
                var io = ImGui.GetIO();
                BoldFont = io.Fonts.AddFontFromFileTTF("C:\\Windows\\Fonts\\arialbd.ttf", 18.0f);
                IconFont = io.Fonts.AddFontFromFileTTF("z", 18.0f);
                io.Fonts.Build();
            }
        }

        public void UpdateLocalPlayer(Entity newEntity) // update local player
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        }

        public Entity GetLocalPlayer() // get localplayer
        {
            lock (entityLock)
            {
                return localPlayer;
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

                //uncap fps
                io.Framerate = 0;

                //no vsync maybe should be on idk
                io.ConfigViewportsNoAutoMerge = true;
                io.ConfigViewportsNoTaskBarIcon = true;
                RenderESPOverlay();
                RenderMainWindow();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
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
            //ImGui.ShowMetricsWindow();
            ImGui.End();
        }

        private void RenderMainWindow()
        {
            // toggle on tab
            if (ImGui.IsKeyPressed(ImGuiKey.Tab, false))
            {
                DrawWindow = !DrawWindow;
                tabWasPressed = true;
            }

            if (DrawWindow)
            {
                ImGuiStylePtr style = ImGui.GetStyle();
                style.Alpha = 1.0f;
                style.DisabledAlpha = 0.8f;
                style.WindowPadding = new Vector2(0.0f, 0.0f);
                style.WindowRounding = 12.0f;
                style.WindowBorderSize = 1.0f;
                style.WindowMinSize = new Vector2(32.0f, 32.0f);
                style.WindowTitleAlign = new Vector2(0.5f, 0.5f);
                style.WindowMenuButtonPosition = ImGuiDir.Left;
                style.ChildRounding = 12.0f;
                style.ChildBorderSize = 1.0f;
                style.PopupRounding = 4.0f;
                style.PopupBorderSize = 1.0f;
                style.FramePadding = new Vector2(5.0f, 1.0f);
                style.FrameRounding = 5.0f;
                style.FrameBorderSize = 1.0f;
                style.ItemSpacing = new Vector2(6.0f, 4.0f);
                style.ItemInnerSpacing = new Vector2(4.0f, 4.0f);
                style.CellPadding = new Vector2(4.0f, 2.0f);
                style.IndentSpacing = 21.0f;
                style.ColumnsMinSpacing = 6.0f;
                style.ScrollbarSize = 13.0f;
                style.ScrollbarRounding = 16.0f;
                style.GrabMinSize = 20.0f;
                style.GrabRounding = 5.0f;
                style.TabRounding = 4.0f;
                style.TabBorderSize = 1.0f;
                style.TabMinWidthForCloseButton = 0.0f;
                style.ColorButtonPosition = ImGuiDir.Right;
                style.ButtonTextAlign = new Vector2(0.5f, 0.5f);
                style.SelectableTextAlign = new Vector2(0.0f, 0.0f);

                style.Colors[(int)ImGuiCol.Text] = new Vector4(0.274f, 0.317f, 0.450f, 1.0f);
                style.Colors[(int)ImGuiCol.TextDisabled] = new Vector4(0.2745098173618317f, 0.3176470696926117f, 0.4501f, 1.0f);
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.078f, 0.0862f, 0.101f, 1.0f);
                style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.09411764889955521f, 0.1019607856869698f, 0.1176470592617989f, 1.0f);
                style.Colors[(int)ImGuiCol.PopupBg] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
                style.Colors[(int)ImGuiCol.Border] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
                style.Colors[(int)ImGuiCol.BorderShadow] = new Vector4(0.0784313753247261f, 0.08627451211214066f, 0.1019607856869698f, 1.0f);
                style.Colors[(int)ImGuiCol.FrameBg] = new Vector4(0.1137254908680916f, 0.125490203499794f, 0.1529411822557449f, 1.0f);
                style.Colors[(int)ImGuiCol.FrameBgHovered] = new Vector4(0.1568627506494522f, 0.168627455830574f, 0.1921568661928177f, 1.0f);
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
                style.Colors[(int)ImGuiCol.Button] = new Vector4(0.1176470592617989f, 0.1333333402872086f, 0.1490196138620377f, 1.0f);
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
                ImGui.SetWindowSize(new Vector2(800, 600));

                ImGui.BeginChild("Sidebar", new Vector2(110, -1), ImGuiChildFlags.Border);
                {
                    float LogoWidth = 120;
                    float offset = (ImGui.GetContentRegionAvail().X - LogoWidth) * 0.5f;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + offset);

                    AddOrGetImagePointer("..\\..\\..\\..\\Resources\\MenuLogo.png", true, out menuLogoTexture, out Width, out Height);
                    ImGui.Image(menuLogoTexture, new Vector2(120, 120));
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    RenderTabButton("Legit", 0);
                    RenderTabButton("Rage", 1);
                    RenderTabButton("Visuals", 2);
                    RenderTabButton("Config", 3);

                    var availableHeight = ImGui.GetContentRegionAvail().Y;
                    var cogButtonHeight = 35f;
                    var spacingHeight = availableHeight - cogButtonHeight - 5f;

                    if (spacingHeight > 0)
                    {
                        ImGui.Dummy(new Vector2(0, spacingHeight));
                    }

                    Vector2 cogPos = ImGui.GetCursorScreenPos();
                    Vector2 cogSize = new(ImGui.GetContentRegionAvail().X, cogButtonHeight);

                    if (ImGui.InvisibleButton("##SettingsGear", cogSize))
                    {
                        selectedTab = 4;
                    }

                    bool isHovered = ImGui.IsItemHovered();
                    bool isSettingsSelected = selectedTab == 4;

                    Vector2 gearCenter = new Vector2(
                        cogPos.X + cogSize.X / 2,
                        cogPos.Y + cogSize.Y / 2
                    );

                    uint gearColor;
                    if (isSettingsSelected)
                    {
                        gearColor = ImGui.ColorConvertFloat4ToU32(accentColor);
                    }
                    else if (isHovered)
                    {
                        gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.9f, 0.9f, 0.9f, 1));
                    }
                    else
                    {
                        gearColor = ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 1));
                    }

                    DrawGearIcon(gearCenter, gearColor);
                }
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("MainContent", new Vector2(0, 0), ImGuiChildFlags.None);
                {
                    ImGui.PopStyleVar();
                    ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));

                    switch (selectedTab)
                    {
                        case 0: // legit
                            RenderCategoryHeader("LEGIT");
                            RenderModuleToggle("Auto BHOP", ref Modules.Legit.Bhop.BhopEnable);
                            RenderModuleToggle("Bomb Overlay", ref BombTimerOverlay.EnableTimeOverlay);
                            RenderModuleToggle("Jump Shot", ref Modules.Legit.JumpHack.JumpHackEnabled);
                            break;

                        case 1: // rage
                            RenderCategoryHeader("Aim");

                            RenderModuleToggle("Enable Aimbot", ref Modules.Rage.Aimbot.AimbotEnable);
                            if (Modules.Rage.Aimbot.AimbotEnable)
                            {
                                Modules.Rage.Aimbot.EnableAimbot();
                                RenderSettingsSection("Aimbot Settings", () =>
                                {
                                    if (ImGui.Combo("##Aim Bone", ref Aimbot.CurrentBone, Aimbot.Bones, Aimbot.Bones.Length)) // bone that aimbot aims on
                                    {
                                    }
                                    if (ImGui.Combo("##Aim Method", ref Aimbot.CurrentAimMethod, Aimbot.AimbotMethods, Aimbot.AimbotMethods.Length))
                                    {
                                    }
                                    RenderModuleToggle("Aim On Team", ref Modules.Rage.Aimbot.Team);
                                    RenderModuleToggle("Draw FOV", ref Modules.Rage.Aimbot.DrawFov);
                                    ImGui.SliderInt("FOV Size", ref Modules.Rage.Aimbot.FovSize, 10, 1000, "%d");
                                    ImGui.Spacing();
                                    ImGui.Text("FOV Color:");
                                    ImGui.ColorEdit4("##FOV Color", ref Modules.Rage.Aimbot.FovColor);
                                });
                            }

                            RenderModuleToggle("Triggerbot", ref Modules.Rage.TriggerBot.Enabled);

                            if (Modules.Rage.TriggerBot.RequireKeybind)
                            {
                                if (isWaitingForKey)
                                {
                                    ImGui.Text("Press any key...");
                                    if (ImGui.Button("Cancel"))
                                    {
                                        isWaitingForKey = false;
                                    }

                                    foreach (Keys key in Enum.GetValues(typeof(Keys)))
                                    {
                                        if ((User32.GetAsyncKeyState((int)key) & 0x8000) != 0 && key != Keys.Escape)
                                        {
                                            Modules.Rage.TriggerBot.TriggerKey = key;
                                            keybindLabel = $"Keybind: {key}";
                                            isWaitingForKey = false;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    if (ImGui.Button(keybindLabel))
                                    {
                                        isWaitingForKey = true;
                                    }
                                }
                            }

                            RenderSettingsSection("Trigger Bot Settings", () =>
                            {
                                ImGui.SliderInt("Delay", ref Modules.Rage.TriggerBot.Delay, 0, 1000, "%d");
                                ImGui.Checkbox("Require Keybind", ref Modules.Rage.TriggerBot.RequireKeybind);
                                ImGui.Checkbox("Shoot At Team", ref Modules.Rage.TriggerBot.ShootAtTeam);
                            });


                            break;

                        case 2: // visuals
                            RenderCategoryHeader("VISUALS");

                            ImGui.Columns(2, "VisualsColumns", true);

                            ImGui.BeginChild("LeftVisuals");
                            RenderModuleToggle("Enable ESP", ref BoxESP.enableESP);

                            RenderSettingsSection("ESP Settings", () =>
                            {
                                if (ImGui.Combo("##ESPShape", ref BoxESP.CurrentShape, BoxESP.Shapes, BoxESP.Shapes.Length)) // shape dropdown
                                {
                                }
                                RenderModuleToggle("Draw On Self", ref BoxESP.DrawOnSelf);
                                RenderModuleToggle("Team Check", ref BoxESP.TeamCheck);
                                RenderModuleToggle("Enable RGB", ref Colors.RGB);
                                ImGui.SliderFloat("Box Fill Opacity", ref BoxESP.BoxFillOpacity, 0.0f, 1.0f, "%.2f");
                                RenderModuleToggle("Outline", ref BoxESP.Outline);
                                ImGui.SliderFloat("ESP Rounding", ref BoxESP.Rounding, 1f, 5f);
                            });

                            RenderModuleToggle("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
                            RenderModuleToggle("Enable Armor Bar", ref ArmorBar.EnableArmorhBar);
                            RenderModuleToggle("Show Distance Text", ref BoxESP.EnableDistanceTracker);

                            RenderModuleToggle("Enable Tracers", ref Tracers.enableTracers);
                            ImGui.SliderFloat("Tracer Thickness", ref Tracers.LineThickness, 0.05f, 5f);

                            RenderModuleToggle("Show Name", ref NameDisplay.Enabled);

                            RenderModuleToggle("Enable Bone ESP", ref Modules.Visual.BoneESP.EnableBoneESP);
                            RenderSettingsSection("Bone ESP Settings", () =>
                            {
                                RenderModuleToggle("Draw On Self", ref BoneESP.DrawOnSelf);
                                RenderModuleToggle("Team Check", ref BoneESP.TeamCheck);
                                ImGui.SliderFloat("Bone Thickness", ref BoneESP.BoneThickness, 1f, 10f, "%.1f");
                                ImGui.ColorEdit4("Bone Color", ref BoneESP.BoneColor);
                                RenderModuleToggle("Enable RGB", ref Colors.RGB);
                            });

                            RenderModuleToggle("Enable Chams", ref Modules.Visual.Chams.EnableChams);
                            if (Modules.Visual.Chams.EnableChams)
                            {
                                RenderSettingsSection("Chams Settings", () =>
                                {
                                    RenderModuleToggle("Draw On Self", ref Chams.DrawOnSelf);
                                    RenderModuleToggle("Draw On Self", ref Modules.Visual.Chams.DrawOnSelf);
                                    ImGui.SliderFloat("Bone Thickness", ref Chams.BoneThickness, 1f, 20f, "%.1f");
                                    RenderModuleToggle("Enable RGB", ref Colors.RGB);
                                });
                            }
                            ImGui.EndChild();

                            ImGui.NextColumn();

                            ImGui.BeginChild("RightVisuals");
                            RenderModuleToggle("Enable Bomb Timer", ref Modules.Visual.BombTimerOverlay.EnableTimeOverlay);
                            RenderModuleToggle("Anti Flash", ref Modules.Visual.NoFlash.NoFlashEnable);
                            RenderModuleToggle("Fov Changer", ref FovChanger.Enabled);
                            ImGui.SliderInt("Fov", ref FovChanger.FOV, 60, 160);
                            ImGui.EndChild();

                            ImGui.Columns(1);
                            break;

                        case 3: // config
                            RenderCategoryHeader("CONFIG");
                            ImGui.Columns(2, "ConfigColum", true);
                            ImGui.BeginChild("ConfigLeft");
                            ImGui.Text("Available Configs:");

                            ImGui.BeginChild("ConfigList", new Vector2(0, 200), ImGuiChildFlags.Border);
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
                                Configs.LoadConfig(Configs.SelectedConfig); // load 
                            }
                            ImGui.EndChild();
                            ImGui.Columns(1);
                            break;


                        case 4: // settings
                            RenderCategoryHeader("SETTINGS");

                            ImGui.Text("GUI Settings");
                            ImGui.Spacing();

                            ImGui.SliderFloat("Window Alpha", ref windowAlpha, 0.1f, 1.0f, "%.2f");
                            ImGui.ColorEdit4("Accent Color", ref accentColor);
                            ImGui.SliderFloat("Animation Speed", ref animationSpeed, 0.01f, 1.0f, "%.2f"); // TODO make like open and close anim maybe tab swtichs

                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Text("Keybinds:");
                            ImGui.Text("Toggle Menu: TAB");

                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Text("About:");
                            ImGui.Text($"Titled Gui v{Configs.Version}");
                            ImGui.Text("External Cheat Made By xfi0 / domok.");
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
        public void RunAllModules()
        {
            // run all modules that access the renderrer on the renderer thread prevents flickering
            if (NameDisplay.Enabled)
            {
                foreach (var e in entities)
                {
                    NameDisplay.DrawName(e, this);
                }
            }
            if (Aimbot.DrawFov && Aimbot.AimbotEnable)
                Aimbot.DrawCircle(Aimbot.FovSize, Aimbot.FovColor);
            if (Modules.Visual.BoneESP.EnableBoneESP)
            {
                foreach (Entity entity in entities)
                {
                    if ((!BoneESP.TeamCheck || entity.Team == localPlayer.Team) && (!BoneESP.DrawOnSelf || entity != localPlayer))
                    {
                        Modules.Visual.BoneESP.DrawBoneLines(entity, this);
                    }
                }
            }
            EyeRay.DrawEyeRay();
            if (Modules.Rage.TriggerBot.Enabled)
            {
                Modules.Rage.TriggerBot.Start();
            }
            if (Modules.Visual.Chams.EnableChams)
            {
                foreach (Entity entity in entities)
                {
                    Chams.DrawChams(entity);
                }
            }
            if (ArmorBar.EnableArmorhBar)
            {
                foreach (var entity in GameState.Entities)
                {
                    float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                    Vector2 rectTTop = new(entity.ViewPosition2D.X - entityHeight / 3, entity.ViewPosition2D.Y);
                    Vector2 barTopLeft = new(rectTTop.X = ArmorBar.ArmorBarWidth + 2f, entity.ViewPosition2D.Y);

                    float barHeight = entityHeight;
                    ArmorBar.DrawArmorBar(this, entity.Armor, 100, barTopLeft, barHeight, entity);
                }
            }

            if (Modules.Visual.BoxESP.enableESP)
            {
                foreach (var entity in entities)
                {
                    if (entity != null)
                    {
                        bool isEnemy = entity.Team != localPlayer.Team;

                        // skip if its local and we should draw on self
                        if (entity.PawnAddress == localPlayer.PawnAddress && !BoxESP.DrawOnSelf)
                            continue;

                        //draw Team and entity if Team check is false
                        if (!Modules.Visual.BoxESP.TeamCheck || (Modules.Visual.BoxESP.TeamCheck && isEnemy))
                        {
                            BoxESP.DrawBoxESP(entity, localPlayer, this);
                        }
                        if (BoxESP.EnableDistanceTracker)
                        {
                            string distText = $"{(int)entity.Distance / 100}m";
                            Vector2 textPos = new Vector2(entity.Position2D.X + 2, entity.Position2D.Y);
                            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), distText);
                        }
                    }
                }
            }
            if (Tracers.enableTracers)
            {
                foreach (var entity in GameState.Entities)
                {
                    if (!Modules.Visual.BoxESP.TeamCheck || (Modules.Visual.BoxESP.TeamCheck && entity.Team != GameState.localPlayer.Team))
                    {
                        Tracers.DrawTracers(entity, this);
                    }
                }
            }
            foreach (var entity in GameState.Entities)
            {
                float entityHeight = entity.Position2D.Y - entity.ViewPosition2D.Y;
                Vector2 rectTTop = new(entity.ViewPosition2D.X - entityHeight / 3, entity.ViewPosition2D.Y);
                Vector2 barTopLeft = new(rectTTop.X - HealthBar.HealthBarWidth - 2, rectTTop.Y);
                float barHeight = entityHeight;
                Modules.Visual.HealthBar.DrawHealthBar(this, entity.Health, 100, barTopLeft, barHeight, entity);
            }
        }
        private static void DrawGearIcon(Vector2 center, uint color)
        {
            const float radius = 8f;
            const int teeth = 8;
            const float innerRadius = radius * 0.6f;
            const float toothHeight = radius * 0.3f;
            const float toothWidth = 0.3f;

            var currentDrawList = ImGui.GetWindowDrawList();

            for (int i = 0; i < teeth; i++)
            {
                float angle1 = (float)(i * 2 * Math.PI / teeth - toothWidth / 2);
                float angle2 = (float)(i * 2 * Math.PI / teeth + toothWidth / 2);

                Vector2 innerP1 = new Vector2(
                    center.X + (float)Math.Cos(angle1) * innerRadius,
                    center.Y + (float)Math.Sin(angle1) * innerRadius
                );
                Vector2 innerP2 = new Vector2(
                    center.X + (float)Math.Cos(angle2) * innerRadius,
                    center.Y + (float)Math.Sin(angle2) * innerRadius
                );
                Vector2 outerP1 = new Vector2(
                    center.X + (float)Math.Cos(angle1) * (radius + toothHeight),
                    center.Y + (float)Math.Sin(angle1) * (radius + toothHeight)
                );
                Vector2 outerP2 = new Vector2(
                    center.X + (float)Math.Cos(angle2) * (radius + toothHeight),
                    center.Y + (float)Math.Sin(angle2) * (radius + toothHeight)
                );

                currentDrawList.AddQuadFilled(innerP1, innerP2, outerP2, outerP1, color);
            }

            currentDrawList.AddCircleFilled(center, radius, color);

            currentDrawList.AddCircleFilled(center, radius * 0.25f, ImGui.ColorConvertFloat4ToU32(new Vector4(0.09f, 0.09f, 0.10f, 1.0f)));
        }

        private static void RenderCategoryHeader(string categoryName)
        {
            if (FontManager.IsBoldFontLoaded)
            {
                ImGui.PushFont(FontManager.BoldFont);
                ImGui.TextColored(accentColor, categoryName);
                ImGui.PopFont();
            }
            else
            {
                ImGui.TextColored(accentColor, categoryName);
            }

            ImGui.Separator();
            ImGui.Spacing();
        }

        private void RenderTabButton(string label, int tabIndex)
        {
            bool isSelected = selectedTab == tabIndex;
            if (isSelected)
            {
                ImGui.PushStyleColor(ImGuiCol.Button, accentColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, accentColor);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, accentColor);
                ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 1, 1, 1));
            }
            else
            {
                ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0, 0, 0, 0));
            }

            if (ImGui.Button(label, new Vector2(-1, 40)))
                selectedTab = tabIndex;

            ImGui.PopStyleColor(isSelected ? 4 : 1);
        }

        private static void RenderModuleToggle(string? label, ref bool value, Action? callback = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
            bool OriginalValue = value;
            if (ImGui.Checkbox(label, ref value))
            {
                callback?.Invoke();
            }
            ImGui.PopStyleVar();
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