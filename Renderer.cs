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

        //entity copy  
        public ConcurrentQueue<Entity> entities = new ConcurrentQueue<Entity>();
        public Entity localPlayer = new Entity();
        private readonly object entityLock = new object();

        // Gui Variables  
        private bool tabWasPressed = false; //bool to fix tab holding

        // GUI Navigation
        private int selectedTab = 0; // 0 = Legit, 1 = Rage, 2 = Config

        //draw list  
        public ImDrawListPtr drawList;

        //mod bools
        public static bool DrawWindow = true;
        private bool enableFovChanger = false; // Checkbox state
        private FovChanger fovChanger = new FovChanger(); // FovChanger instance

        //entity methods
        public void UpdateEntities(IEnumerable<Entity> newEntities) // update entities
        {
            entities = new ConcurrentQueue<Entity>(newEntities);
        }
        public void UpdateLocalPlayer(Entity newEntity)
        {
            lock (entityLock)
            {
                localPlayer = newEntity;
            }
        } // update local player
        public Entity GetLocalPlayer()
        {
            lock (entityLock)
            {
                return localPlayer;
            }
        } // get localplayer

        protected override void Render() // esp overlay
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

            // settings overlay
            if (ImGui.IsKeyPressed(ImGuiKey.Tab, false))
            {
                DrawWindow = !DrawWindow;
                tabWasPressed = true;
            }

            if (DrawWindow)
            {
                // Set window rounding and colors
                var style = ImGui.GetStyle();
                style.WindowRounding = 4.0f; // Less rounded
                style.ChildRounding = 6.0f;
                style.FrameRounding = 4.0f;
                style.PopupRounding = 4.0f;
                style.ScrollbarRounding = 4.0f;
                style.GrabRounding = 4.0f;
                style.TabRounding = 4.0f;
                style.Colors[(int)ImGuiCol.WindowBg] = new Vector4(0.13f, 0.14f, 0.15f, 1.0f);
                style.Colors[(int)ImGuiCol.ChildBg] = new Vector4(0.16f, 0.18f, 0.20f, 1.0f);
                style.Colors[(int)ImGuiCol.TitleBg] = new Vector4(0.18f, 0.10f, 0.22f, 1.0f);
                style.Colors[(int)ImGuiCol.TitleBgActive] = new Vector4(0.35f, 0.0f, 0.45f, 1.0f);

                style.Colors[(int)ImGuiCol.Button] = new Vector4(0.20f, 0.22f, 0.24f, 1.0f);
                style.Colors[(int)ImGuiCol.ButtonHovered] = new Vector4(0.24f, 0.26f, 0.28f, 1.0f);
                style.Colors[(int)ImGuiCol.ButtonActive] = new Vector4(0.90f, 0.26f, 0.26f, 1.0f);

                ImGui.Begin("Titled Gui", ref DrawWindow, ImGuiWindowFlags.NoResize);

                ImGui.SetWindowSize(new Vector2(800, 600));

                ImGui.BeginChild("Sidebar", new Vector2(150, 0), ImGuiChildFlags.Border);
                {
                    ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(8, 8));
                    ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, new Vector2(0.0f, 0.5f));

                    Vector4 activeColor = new Vector4(0.90f, 0.26f, 0.26f, 1.0f);
                    Vector4 inactiveColor = new Vector4(0.20f, 0.22f, 0.24f, 1.0f);

                    if (selectedTab == 0)
                        ImGui.PushStyleColor(ImGuiCol.Button, activeColor);
                    else
                        ImGui.PushStyleColor(ImGuiCol.Button, inactiveColor);

                    if (ImGui.Button("Legit", new Vector2(-1, 40)))
                        selectedTab = 0;
                    ImGui.PopStyleColor();

                    if (selectedTab == 1)
                        ImGui.PushStyleColor(ImGuiCol.Button, activeColor);
                    else
                        ImGui.PushStyleColor(ImGuiCol.Button, inactiveColor);

                    if (ImGui.Button("Rage", new Vector2(-1, 40)))
                        selectedTab = 1;
                    ImGui.PopStyleColor();

                    if (selectedTab == 2)
                        ImGui.PushStyleColor(ImGuiCol.Button, activeColor);
                    else
                        ImGui.PushStyleColor(ImGuiCol.Button, inactiveColor);

                    if (ImGui.Button("Config", new Vector2(-1, 40)))
                        selectedTab = 2;
                    ImGui.PopStyleColor();

                    ImGui.PopStyleVar(2);
                }
                ImGui.EndChild();

                ImGui.SameLine();

                ImGui.BeginChild("MainContent", new Vector2(0, 0), ImGuiChildFlags.Border);
                {
                    switch (selectedTab)
                    {
                        case 0: // legit
                            ImGui.Text("Legit");
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Checkbox("Enable Box ESP", ref BoxESP.enableESP);
                            ImGui.Checkbox("Enable Bone ESP", ref Modules.Visual.BoneESP.EnableBoneESP);

                            if (BoxESP.enableESP)
                            {
                                ImGui.Spacing();
                                if (ImGui.CollapsingHeader("Box ESP Settings"))
                                {
                                    ImGui.Indent();
                                    ImGui.Checkbox("Enable Tracers", ref Tracers.enableTracers);
                                    ImGui.Checkbox("Draw On Self", ref Tracers.DrawOnSelf);
                                    ImGui.Checkbox("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
                                    ImGui.Checkbox("Draw On Self", ref Modules.Visual.HealthBar.DrawOnSelf);
                                    ImGui.Checkbox("Team Check", ref BoxESP.TeamCheck);
                                    ImGui.Checkbox("Show Distance Text", ref DistanceTracker.EnableDistanceTracker);
                                    ImGui.Checkbox("Enable RGB", ref Colors.RGB);
                                    ImGui.Unindent();
                                }
                            }
                            if (Modules.Visual.BoneESP.EnableBoneESP)
                            {
                                ImGui.Spacing();
                                if (ImGui.CollapsingHeader("Bone ESP Settings"))
                                {
                                    ImGui.Indent();
                                    ImGui.Checkbox("Enable Tracers", ref Tracers.enableTracers);
                                    ImGui.Checkbox("Draw On Self", ref Tracers.DrawOnSelf);
                                    ImGui.Checkbox("Enable Health Bar", ref Modules.Visual.HealthBar.EnableHealthBar);
                                    ImGui.Checkbox("Draw On Self", ref Modules.Visual.HealthBar.DrawOnSelf);
                                    ImGui.SliderFloat("Bone Thickness", ref BoneESP.BoneThickness, 1f, 10f);
                                    ImGui.ColorEdit4("Bone Color", ref Colors.BoneColor);
                                    ImGui.Checkbox("Enable RGB", ref Colors.RGB);
                                    ImGui.Unindent();
                                }
                            }
                            ImGui.Checkbox("Enable Bomb Timer", ref Modules.Visual.BombTimerOverlay.EnableTimeOverlay);
                            ImGui.Checkbox("Anti Flash", ref Modules.Visual.NoFlash.NoFlashEnable);
                            ImGui.Checkbox("Auto BHOP", ref Modules.Legit.Bhop.BhopEnable);
                            break;

                        case 1: // rage
                            ImGui.Text("Rage");
                            ImGui.Separator();
                            ImGui.Spacing();

                            ImGui.Checkbox("Enable Aimbot", ref Modules.Rage.Aimbot.AimbotEnable);
                            if (Modules.Rage.Aimbot.AimbotEnable)
                            {
                                Modules.Rage.Aimbot.EnableAimbot();
                                ImGui.Spacing();
                                if (ImGui.CollapsingHeader("Aimbot Settings"))
                                {
                                    ImGui.Indent();
                                    ImGui.Checkbox("Aim On Team", ref Modules.Rage.Aimbot.Team);
                                    ImGui.Checkbox("Draw FOV", ref Modules.Rage.Aimbot.DrawFOV);
                                    ImGui.SliderInt("FOV Size", ref Modules.Rage.Aimbot.FovSize, 10, 1000);
                                    ImGui.Spacing();
                                    ImGui.Text("FOV Color:");
                                    ImGui.ColorEdit4("##FOV Color", ref Modules.Rage.Aimbot.FovColor);
                                    ImGui.Unindent();
                                }
                            }
                            if (ImGui.Checkbox("CV Triggerbot", ref Modules.Rage.CVTriggerBot.Enabled))
                            {
                                if (Modules.Rage.CVTriggerBot.Enabled)
                                {
                                    Modules.Rage.CVTriggerBot.Start(this);
                                }
                            }

                            if (Modules.Rage.Aimbot.DrawFOV && Modules.Rage.Aimbot.AimbotEnable)
                            {
                                DrawCircle(Modules.Rage.Aimbot.FovSize, Modules.Rage.Aimbot.FovColor);
                            }
                            break;

                        case 2: // config
                            ImGui.Text("Configs");
                            ImGui.Separator();
                            ImGui.Spacing();

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
                                ImGui.Text("Default.cfg");
                                ImGui.Text("Legit.cfg");
                                ImGui.Text("Rage.cfg");
                            }
                            ImGui.EndChild();
                            break;
                    }
                }
                ImGui.EndChild();

                ImGui.End();

                if (Modules.Rage.Aimbot.DrawFOV && Modules.Rage.Aimbot.AimbotEnable)
                {
                    DrawCircle(Modules.Rage.Aimbot.FovSize, Modules.Rage.Aimbot.FovColor);
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
                            if (DistanceTracker.EnableDistanceTracker)
                            {
                                string distText = $"{(int)entity.distance}m";
                                Vector2 textPos = new Vector2(entity.position2D.X + 5, entity.position2D.Y);
                                drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), distText);
                            }
                        }
                    }
                }
                if( Modules.Legit.Bhop.BhopEnable)
                {
                    Modules.Legit.Bhop.AutoBhop();
                }
            }
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