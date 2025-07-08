using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ClickableTransparentOverlay;
using ImGuiNET;
using Titled_Gui.Data;
using Titled_Gui.Modules.Legit;
using System.Windows.Input;
using Titled_Gui.Modules.Visual;
using Titled_Gui.Modules.Rage;
using System.Windows.Forms;
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui
{
    public class Renderer : Overlay
    {
        // renderer variables  
        public Vector2 screenSize = new Vector2(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height); // should automatically be the size of anyones screen
        private IntPtr menuLogoTexture;
        private IntPtr cogIconTexture;
        private uint Width;
        private uint Height;
        private uint CogWidth;
        private uint CogHeight;

        //entity copy  
        public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        public Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        // Gui Variables  
        private bool tabWasPressed = false; //bool to fix tab holding

        // GUI Navigation
        private int selectedTab = 0; // 0 = legit, 1 = rage, 2 = visuals, 3 = config, 4 = settings

        //draw list  
        public ImDrawListPtr drawList;

        //mod bools
        public static bool DrawWindow = true;
        private bool enableFovChanger = false;

        // UI variables
        private Vector4 accentColor = new Vector4(0.26f, 0.59f, 0.98f, 1.00f);
        private float windowAlpha = 1f; 
        private float animationSpeed = 0.15f; 

        //entity methods
        public void UpdateEntities(IEnumerable<Entity> newEntities) // update entities
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
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

        protected override void Render() // esp overlay
        {
            RenderESPOverlay();
            RenderMainWindow();
        }

        private void RenderESPOverlay()
        {
            // ESP overlay rendering
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

            // esp
            if (Modules.Visual.BoxESP.enableESP)
            {
                foreach (var entity in entities)
                {
                    if (entity != null && entity != localPlayer)
                    {
                        bool isEnemy = entity.team != localPlayer.team;

                        //draw team and entity if team check is false
                        if (!Modules.Visual.BoxESP.TeamCheck || (Modules.Visual.BoxESP.TeamCheck && isEnemy))
                        {
                            BoxESP.DrawBoxESP(entity, this);

                            if (DistanceTracker.EnableDistanceTracker)
                            {
                                string distText = $"{(int)entity.distance}m";
                                Vector2 textPos = new Vector2(entity.position2D.X + 5, entity.position2D.Y);
                                drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), distText);
                            }
                        }

                        if (Tracers.enableTracers && (!Tracers.DrawOnSelf || entity != localPlayer))
                        {
                            if (!Modules.Visual.BoxESP.TeamCheck || (Modules.Visual.BoxESP.TeamCheck && isEnemy))
                            {
                                Tracers.DrawTracers(entity, this);
                            }
                        }

                        if (Modules.Visual.HealthBar.EnableHealthBar && (!Modules.Visual.HealthBar.DrawOnSelf || entity != localPlayer))
                        {
                            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y;
                            float barWidth = 5f;
                            Vector2 rectTTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
                            Vector2 barTopLeft = new Vector2(rectTTop.X - barWidth - 2, rectTTop.Y);
                            float barHeight = entityHeight;

                            Modules.Visual.HealthBar.DrawHealthBar(this, entity.health, 100, barTopLeft, barHeight, barWidth);
                        }
                    }
                }
            }
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
                
                var style = ImGui.GetStyle();
                style.WindowRounding = 8.0f;
                style.ChildRounding = 8.0f;
                style.FrameRounding = 4.0f;
                style.PopupRounding = 8.0f;
                style.ScrollbarRounding = 4.0f;
                style.GrabRounding = 4.0f;
                style.TabRounding = 8.0f;
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.09f, 0.09f, 0.10f, windowAlpha);
                style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.11f, 0.11f, 0.12f, windowAlpha);
                style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.08f, 0.08f, 0.09f, windowAlpha);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.11f, 0.11f, 0.12f, windowAlpha);
                style.Colors[(int)ImGuiCol.Border] = new Vector4(0.15f, 0.15f, 0.16f, windowAlpha);
                style.Colors[(int)ImGuiCol.Button] = new Vector4(0.18f, 0.18f, 0.19f, windowAlpha);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.22f, 0.22f, 0.23f, windowAlpha);
                style.Colors[(int)ImGuiCol.ButtonActive] = accentColor;
                style.Colors[(int)ImGuiCol.Header] = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.4f);
                style.Colors[(int)ImGuiCol.HeaderHovered] = new Vector4(accentColor.X, accentColor.Y, accentColor.Z, 0.6f);
                style.Colors[(int)ImGuiCol.HeaderActive] = accentColor;

                ImGui.Begin("Titled Gui", ref DrawWindow, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);
                ImGui.SetWindowSize(new Vector2(800, 600));

                ImGui.BeginChild("Sidebar", new Vector2(120, 550), ImGuiChildFlags.Border | ImGuiChildFlags.AutoResizeY);
                {
                    AddOrGetImagePointer("C:\\Users\\papsb\\Downloads\\Titled Gui\\Resources\\MenuLogo.png", true, out menuLogoTexture, out Width, out Height);
                    ImGui.Image(menuLogoTexture, new Vector2(120, 120));
                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(12, 12));
                    ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4));
                    ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));

                    RenderTabButton("Legit", 0);
                    RenderTabButton("Rage", 1);
                    RenderTabButton("Visuals", 2);
                    RenderTabButton("Config", 3);

                    ImGui.PopStyleVar(3);

                    var availableHeight = ImGui.GetContentRegionAvail().Y;
                    var cogButtonHeight = 35f; 
                    var spacingHeight = availableHeight - cogButtonHeight - 5f; 

                    if (spacingHeight > 0)
                    {
                        ImGui.Dummy(new Vector2(0, spacingHeight));
                    }

                    Vector2 cogPos = ImGui.GetCursorScreenPos();
                    Vector2 cogSize = new Vector2(ImGui.GetContentRegionAvail().X, cogButtonHeight);

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

                ImGui.BeginChild("MainContent", new Vector2(0, 0), ImGuiChildFlags.Border);
                {
                    switch (selectedTab)
                    {
                        case 0: // leigt
                            RenderCategoryHeader("LEGIT");
                            RenderModuleToggle("Auto BHOP", ref Modules.Legit.Bhop.BhopEnable);
                            RenderModuleToggle("Jump Shot", ref Modules.Legit.JumpHack.JumpHackEnabled);
                            break;

                        case 1: // rage
                            RenderCategoryHeader("RAGE");

                            RenderModuleToggle("Enable Aimbot", ref Modules.Rage.Aimbot.AimbotEnable);
                            if (Modules.Rage.Aimbot.AimbotEnable)
                            {
                                Modules.Rage.Aimbot.EnableAimbot(this);
                                RenderSettingsSection("Aimbot Settings", () => {
                                    RenderModuleToggle("Aim On Team", ref Modules.Rage.Aimbot.Team);
                                    RenderModuleToggle("Draw FOV", ref Modules.Rage.Aimbot.DrawFOV);
                                    ImGui.SliderInt("FOV Size", ref Modules.Rage.Aimbot.FovSize, 10, 1000, "%d");
                                    ImGui.Spacing();
                                    ImGui.Text("FOV Color:");
                                    ImGui.ColorEdit4("##FOV Color", ref Modules.Rage.Aimbot.FovColor);
                                });
                            }

                            RenderModuleToggle("CV Triggerbot", ref Modules.Rage.CVTriggerBot.Enabled, () => {
                                if (Modules.Rage.CVTriggerBot.Enabled)
                                {
                                    Modules.Rage.CVTriggerBot.Start(this);
                                }
                            });

                            if (Modules.Rage.Aimbot.DrawFOV && Modules.Rage.Aimbot.AimbotEnable)
                            {
                                DrawCircle(Modules.Rage.Aimbot.FovSize, Modules.Rage.Aimbot.FovColor);
                            }
                            break;

                        case 2: // visuals
                            RenderCategoryHeader("VISUALS");

                            RenderModuleToggle("Enable Box ESP", ref BoxESP.enableESP);
                            if (BoxESP.enableESP)
                            {
                                RenderSettingsSection("Box ESP Settings", () => {
                                    RenderModuleToggle("Enable Tracers", ref Tracers.enableTracers);
                                    RenderModuleToggle("Draw On Self", ref Tracers.DrawOnSelf);
                                    RenderModuleToggle("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
                                    RenderModuleToggle("Draw On Self", ref Modules.Visual.HealthBar.DrawOnSelf);
                                    RenderModuleToggle("Team Check", ref BoxESP.TeamCheck);
                                    RenderModuleToggle("Show Distance Text", ref DistanceTracker.EnableDistanceTracker);
                                    RenderModuleToggle("Enable RGB", ref Colors.RGB);
                                });
                            }

                            RenderModuleToggle("Enable Bone ESP", ref Modules.Visual.BoneESP.EnableBoneESP);
                            if (Modules.Visual.BoneESP.EnableBoneESP)
                            {
                                RenderSettingsSection("Bone ESP Settings", () => {
                                    RenderModuleToggle("Enable Tracers", ref Tracers.enableTracers);
                                    RenderModuleToggle("Draw On Self", ref Tracers.DrawOnSelf);
                                    RenderModuleToggle("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
                                    RenderModuleToggle("Draw On Self", ref Modules.Visual.HealthBar.DrawOnSelf);
                                    ImGui.SliderFloat("Bone Thickness", ref BoneESP.BoneThickness, 1f, 10f, "%.1f");
                                    ImGui.ColorEdit4("Bone Color", ref Colors.BoneColor);
                                    RenderModuleToggle("Enable RGB", ref Colors.RGB);
                                });
                            }

                            RenderModuleToggle("Enable Bomb Timer", ref Modules.Visual.BombTimerOverlay.EnableTimeOverlay);
                            RenderModuleToggle("Anti Flash", ref Modules.Visual.NoFlash.NoFlashEnable);
                            break;

                        case 3: // config
                            RenderCategoryHeader("CONFIG");

                            ImGui.Text("Config management coming soon...");
                            ImGui.Spacing();

                            if (ImGui.Button("Save Config", new Vector2(120, 30)))
                            {
                            }
                            ImGui.SameLine();
                            if (ImGui.Button("Load Config", new Vector2(120, 30)))
                            {
                            }

                            ImGui.Spacing();
                            ImGui.Text("Available Configs:");
                            ImGui.BeginChild("ConfigList", new Vector2(0, 200), ImGuiChildFlags.Border);
                            {
                                ImGui.Selectable("Default.cfg");
                                ImGui.Selectable("Legit.cfg");
                                ImGui.Selectable("Rage.cfg");
                            }
                            ImGui.EndChild();
                            break;

                        case 4: // settings
                            RenderCategoryHeader("SETTINGS");

                            ImGui.Text("Application Settings");
                            ImGui.Spacing();

                            ImGui.SliderFloat("Window Alpha", ref windowAlpha, 0.1f, 1.0f, "%.2f");
                            ImGui.ColorEdit4("Accent Color", ref accentColor);
                            ImGui.SliderFloat("Animation Speed", ref animationSpeed, 0.01f, 1.0f, "%.2f");

                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Text("Keybinds:");
                            ImGui.Text("Toggle Menu: TAB");

                            ImGui.Spacing();
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Text("About");
                            ImGui.Text("Titled Gui v1.0");
                            ImGui.Text("External Cheat Made By domok.");
                            break;
                    }
                }
                ImGui.EndChild();

                ImGui.End();

                // run all the modules in a loop
                if (Modules.Rage.Aimbot.DrawFOV && Modules.Rage.Aimbot.AimbotEnable)
                {
                    DrawCircle(Modules.Rage.Aimbot.FovSize, Modules.Rage.Aimbot.FovColor);
                }
                if (Modules.Legit.SlientAimbot.AimbotEnable)
                {
                    SlientAimbot.EnableAimbot();
                }
                if (Modules.Visual.NoFlash.NoFlashEnable)
                {
                    Modules.Visual.NoFlash.RemoveFlash();
                }
                if (Modules.Visual.BombTimerOverlay.EnableTimeOverlay)
                {
                    Modules.Visual.BombTimerOverlay.TimeOverlay(this);
                }
                if (Modules.Visual.BoneESP.EnableBoneESP)
                {
                    foreach (Entity entity in entities)
                    {
                        if ((!Modules.Rage.Aimbot.Team || entity.team == localPlayer.team) && (!BoneESP.DrawOnSelf || entity != localPlayer))
                        {
                            Modules.Visual.BoneESP.DrawBoneLines(entity, this);
                        }
                    }
                }
                if (Titled_Gui.Modules.Rage.Aimbot.AimbotEnable)
                {
                    Titled_Gui.Modules.Rage.Aimbot.EnableAimbot(this);
                }
                if (Titled_Gui.Modules.Visual.BombTimerOverlay.EnableTimeOverlay)
                {
                    BombTimerOverlay.TimeOverlay(this);
                }
                if (Modules.Legit.JumpHack.JumpHackEnabled)
                {
                    Modules.Legit.JumpHack.JumpShot();
                }
                if (Modules.Visual.BoneESP.EnableBoneESP)
                {
                    foreach (Entity entity in entities)
                    {
                    }
                }
                if (Modules.Legit.Bhop.BhopEnable)
                {
                    Modules.Legit.Bhop.AutoBhop();
                }
            }
        }

        private void DrawGearIcon(Vector2 center, uint color)
        {
            const float radius = 8f; 
            const int teeth = 100;
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

        private void RenderCategoryHeader(string categoryName)
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

        private void RenderModuleToggle(string label, ref bool value, Action callback = null)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
            bool originalValue = value;
            if (ImGui.Checkbox(label, ref value))
            {
                callback?.Invoke();
            }
            ImGui.PopStyleVar();
        }

        private void RenderSettingsSection(string label, Action content)
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 4));
            if (ImGui.CollapsingHeader(label, ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Indent(16f);
                content();
                ImGui.Unindent(16f);
            }
            ImGui.PopStyleVar();
        }

        public void DrawCircle(int Size, Vector4 color) // draw fov circle 
        {
            Vector4 circleColor = color; // set the circle color based on the team  
            float radius = Size; // use the size to determine radius
            drawList.AddCircle(new Vector2(screenSize.X / 2, screenSize.Y / 2), radius, ImGui.ColorConvertFloat4ToU32(circleColor), 32, 1.0f); // draw circle  
        }

        public void DrawSolidBox(Entity entity, Vector4 color) // draw solid box around entity  
        {
            Vector4 boxColor = color; // set the box color based on the team  
            float entityHeight = entity.position2D.Y - entity.viewPosition2D.Y; // size of the box  
            Vector2 rectTTop = new Vector2(entity.viewPosition2D.X - entityHeight / 3, entity.viewPosition2D.Y);
            Vector2 rectBottom = new Vector2(entity.position2D.X + entityHeight / 3, entity.position2D.Y); // bottom of the box  
            drawList.AddRectFilled(rectTTop, rectBottom, ImGui.ColorConvertFloat4ToU32(boxColor));
        }

        //actually draw the overlay 
        void DrawOverlay(Vector2 screenSize) //overlay
        {
            ImGui.SetNextWindowSize(screenSize); // set the size to the screen size
            ImGui.SetNextWindowPos(Vector2.Zero); // start in middle of the screen
            ImGui.Begin("Titled Gui",
            ImGuiWindowFlags.NoTitleBar |
            ImGuiWindowFlags.NoResize |
            ImGuiWindowFlags.NoScrollbar |
            ImGuiWindowFlags.NoScrollWithMouse |
            ImGuiWindowFlags.NoCollapse |
            ImGuiWindowFlags.NoBackground |
            ImGuiWindowFlags.NoSavedSettings
            );
        }
    }
}