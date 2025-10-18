using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Rage
{
    public class Aimbot : Classes.ThreadService
    {
        public static bool AimbotEnable = false;
        public static bool Team = false;
        public static int FovSize = 100;
        public static Vector4 FovColor = new(1f, 0f, 0f, 1f);
        public static bool DrawFov = true;
        public static int AimbotKey = 0x04; // mmb
        public static string[] Bones = ["Head", "Neck", "Right Sholder", "Left Sholder", "Waist", "Random"];
        public static int CurrentBone = 0;
        public static int CurrentBoneIndex = 2;
        public static Vector3 CurrentBoneV3 = Vector3.Zero;
        public static bool RandomChosen = false;
        public static string[] AimbotMethods = ["Mouse Movement", "Memory Writing"];
        public static int CurrentAimMethod = 0;
        public static float SmoothingX = 5f;
        public static float SmoothingY = 5f;
        private static bool FlashCheck = false; 
        public static bool ScopedOnly = false;
        public static bool UseFOV = true;
        public static Random random = new();
        public static bool VisibilityCheck = true;
        public static void EnableAimbot()
        {
            try
            {
                if (!AimbotEnable || ((User32.GetAsyncKeyState(AimbotKey) & 0x8000) == 0) || Entities == null || GameState.LocalPlayer.Health == 0 || (ScopedOnly && !GameState.LocalPlayer.IsScoped) || (FlashCheck && GameState.LocalPlayer.IsFlashed)) { RandomChosen = false; return; }

                Entity? target = GetTarget();

                if (target == null) return;

                Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);
                Vector3 playerView = LocalPlayer.Origin + LocalPlayer.View;
                Vector2 newAngles;

                bool useHeadPosition = target?.Bones?[CurrentBoneIndex] != Vector3.Zero;
                if (useHeadPosition) //bone pos first
                {
                    if (target?.Bones?[CurrentBoneIndex] != null && target.Bones.Count > 6)
                    {
                        try
                        {
                            switch (CurrentBone)
                            {
                                case 0: CurrentBoneIndex = 2; break;
                                case 1: CurrentBoneIndex = 1; break;
                                case 2: CurrentBoneIndex = 6; break;
                                case 3: CurrentBoneIndex = 3; break;
                                case 4: CurrentBoneIndex = 0; break;
                                case 5:
                                    if (!RandomChosen && target.Bones != null && target.Bones.Count > 0)
                                    {
                                        CurrentBoneIndex = random.Next(target.Bones.Count);
                                        RandomChosen = true;
                                    }
                                    break;
                                default: CurrentBoneIndex = (int)BoneESP.BoneIds.Head; break;
                            }

                            if (CurrentBone != 5 && RandomChosen)
                                RandomChosen = false;

                            CurrentBoneV3 = target!.Bones![CurrentBoneIndex]!;

                            if (CurrentBoneV3 != Vector3.Zero)
                                newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                            else
                                newAngles = Calculate.CalculateAngles(playerView, target.Position);
                        }
                        catch (Exception e)
                        {
                            newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                            Console.WriteLine(e.Message);
                        }
                    }
                    else
                    {
                        if (target != null)
                            newAngles = Calculate.CalculateAngles(playerView, target.Head);
                        else
                            newAngles = new Vector2(GameState.swed.ReadVec(client, Offsets.dwViewAngles).X, GameState.swed.ReadVec(client, Offsets.dwViewAngles).Y); // if entity is null dont change anything
                    }
                }
                else //fallback that if youre at there body flick to whatever chosen bone 
                {
                    newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                }

                if (float.IsNaN(newAngles.X) || float.IsNaN(newAngles.Y))
                    return;

                switch (CurrentAimMethod)
                {
                    case 0: // mouse
                        {
                            Vector2 newAngles2D = (target != null && target.Bones2D != null  &&
                            CurrentBoneIndex < target.Bones2D.Count && target.Bones2D[CurrentBoneIndex] != Vector2.Zero) 
                            ? target.Bones2D[CurrentBoneIndex] : target?.Position2D ?? Vector2.Zero; // holy ts is long
                            
                            int dx = (int)(newAngles2D.X - screenCenter.X);
                            int dy = (int)(newAngles2D.Y - screenCenter.Y);

                            if (SmoothingX > 0)
                                dx = (int)(dx / SmoothingX);
                            else
                                dx = (int)(dx / 1);

                            if (SmoothingY > 0)
                                dy = (int)(dy / SmoothingY);
                            else
                                dy = (int)(dy / 1);

                            MoveMouse.MouseMove(dx, dy);
                        }
                        break;


                    case 1: // write
                        {
                            Vector3 currentAngles = GameState.swed.ReadVec(client, Offsets.dwViewAngles);

                            Vector3 targetAngles = new(newAngles.X, newAngles.Y, 0.0f);
                            targetAngles.X = Math.Clamp(targetAngles.X, -89f, 89f);
                            targetAngles.Y = Calculate.NormalizeAngle(targetAngles.Y);
                            Vector3 d = targetAngles - currentAngles;
                            d.Y = Calculate.NormalizeAngle(d.Y);

                            float sx = SmoothingX > 0 ? SmoothingX : 1f;
                            float sy = SmoothingY > 0 ? SmoothingY : 1f;

                            Vector3 step = new(d.X / sx, d.Y / sy, 0.0f);
                            Vector3 newAnglesVec3 = currentAngles + step;

                            GameState.swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);// write the view angle
                        }
                        break;

                }
            }
            catch (DivideByZeroException)
            {
            }
            catch (Exception)
            {

            }
        }

        public static void DrawCircle(int size, Vector4 color) 
        {
            Vector4 circleColor = color; 
            float radius = size; 
            GameState.renderer.drawList.AddCircle(new Vector2(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2), radius, ImGui.ColorConvertFloat4ToU32(circleColor), 32, 1.0f); // draw circle  
        }
        public static Entity? GetTarget() // aimbot function was getting long
        {
            try
            {
                if (Entities == null) return null;

                Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);
                Entity? bestTarget = null;
                float closestDist = float.MaxValue;

                foreach (var entity in Entities)
                {
                    if (entity.Position2D == new Vector2(-99, -99) || entity.Head2D == new Vector2(-99, -99) || entity.Health == 0 || (!Team && entity.Team == GameState.LocalPlayer.Team) || (VisibilityCheck && !entity.Visible)) continue;

                    float distToBody = Vector2.Distance(screenCenter, entity.Position2D);
                    float distToHead = Vector2.Distance(screenCenter, entity.Head2D);

                    // skip if out of FOV or usefov is false
                    if (UseFOV && (distToBody > FovSize && distToHead > FovSize)) continue;

                    // pick closest 
                    float dist = Math.Min(distToBody, distToHead);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        bestTarget = entity;
                    }
                }

                return bestTarget;
            }
            catch (Exception)
            {
                return null;
            }
        }

        protected override void FrameAction()
        {
            EnableAimbot();
        }
    }
}