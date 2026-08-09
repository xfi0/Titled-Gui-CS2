using ClickableTransparentOverlay.Win32;
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
        public static bool DrawFov = true;
        public static bool RandomChosen = false;
        public static bool ScopedOnly = false;
        public static bool UseFOV = true;
        public static bool VisibilityCheck = true;
        public static bool TargetLine = true;
        public static bool RGB = false;
        private static bool FlashCheck = false;
        public static float SmoothingX = 5f;
        public static float SmoothingY = 5f;
        public static Vector4 FovColor = new(1f, 0f, 0f, 1f);
        public static int FovSize = 100;
        public static int AimbotKey = 0x04; // mmb
        public static int CurrentBone = 0;
        public static int CurrentBoneIndex = 2;
        public static int CurrentAimMethod = 0;
        public static string[] Bones = ["Head", "Neck", "Right Shoulder", "Left Shoulder", "Waist", "Random"];
        public static Vector2 CurrentBone2D = Vector2.Zero;
        private static Vector2 remainder = Vector2.Zero;
        public static Random random = new();
        private static Entity? target = null;
        private static Entity? previousTarget = null;

        public static void EnableAimbot() // TODO: return to old pos setting #7
        {
            try
            {
                if (!AimbotEnable || Entities.Count == 0 || LocalPlayer == null || GameState.renderer == null || GameState.LocalPlayer.Health == 0 || (ScopedOnly && !GameState.LocalPlayer.IsScoped) || (FlashCheck && GameState.LocalPlayer.IsFlashed))
                {
                    RandomChosen = false;
                    return;
                }

                if (AimbotKey == 0 || (User32.GetAsyncKeyState(AimbotKey) & 0x8000) != 0) // 0 == none
                {
                    target = GetTarget();

                    if (target == null || target.Bones == null || target.Bones.Count <= 0 || target.Bones == null) return;

                    if (target != previousTarget)
                    {
                        RandomChosen = false;
                        previousTarget = target;
                    }
                    Vector2 screenCenter = new(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2);
                    Vector2 newAngles2D = Vector2.Zero;

                    switch (CurrentBone)
                    {
                        case 0: CurrentBoneIndex = (int)BoneESP.BoneIds.Head; break;
                        case 1: CurrentBoneIndex = (int)BoneESP.BoneIds.Neck; break;
                        case 2: CurrentBoneIndex = (int)BoneESP.BoneIds.RightShoulder; break;
                        case 3: CurrentBoneIndex = (int)BoneESP.BoneIds.LeftShoulder; break;
                        case 4: CurrentBoneIndex = (int)BoneESP.BoneIds.Pelvis; break;
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

                    bool useHeadPosition = target?.Bones?[CurrentBoneIndex].Position != Vector3.Zero;
                    if (useHeadPosition) //bone pos first
                    {
                        if (target?.Bones?[CurrentBoneIndex] != null && target.Bones.Count > CurrentBoneIndex)
                        {
                            try
                            {
                                CurrentBone2D = target.Bones[CurrentBoneIndex].Position2D;

                                if (CurrentBone2D != Vector2.Zero)
                                    newAngles2D = CurrentBone2D;
                                else
                                    newAngles2D = target.Position2D;
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }
                        else
                        {
                            if (target != null)
                                newAngles2D = target.Head2D;
                        }
                    }
                    else //fallback that if you're at their body flick to whatever chosen bone 
                    {
                        newAngles2D = CurrentBone2D;
                    }

                    if (float.IsNaN(newAngles2D.X) || float.IsNaN(newAngles2D.Y))
                        return;

                    float dx = newAngles2D.X - screenCenter.X;
                    float dy = newAngles2D.Y - screenCenter.Y;
                    MoveMousePos(dx, dy);
                }
                else
                {
                    remainder = Vector2.Zero;
                    target = null;
                }
            }
            catch (DivideByZeroException divideByZeroException)
            {
                Console.WriteLine("Aimbot divide by zero exception: " + divideByZeroException);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Aimbot exception: " + ex);
            }
        }
        private static void MoveMousePos(float dx, float dy)
        {
            dx = (SmoothingX > 0 ? dx / SmoothingX : dx) + remainder.X;
            dy = (SmoothingY > 0 ? dy / SmoothingY : dy) + remainder.Y;

            int ix = (int)dx;
            int iy = (int)dy;

            remainder.X = dx - ix;
            remainder.Y = dy - iy;

            User32.MouseMove(ix, iy);
        }

        public static void DrawFOVCircle(int size, Vector4 circleColor)
        {
            if (GameState.renderer == null)
                return;

            float radius = size;
            GameState.renderer.DrawList.AddCircle(new Vector2(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2), radius, ImGui.ColorConvertFloat4ToU32(circleColor), 32, 1.0f); // draw circle  
        }

        public static Entity? GetTarget() // aimbot function was getting long
        {
            try
            {
                if (Entities.Count == 0 || GameState.renderer == null)
                    return null;

                Vector2 screenCenter = new(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2);
                Entity? bestTarget = null;
                float closestDist = float.MaxValue;

                foreach (Entity? entity in Entities)
                {
                    if (entity == null || entity.Position2D == new Vector2(-99, -99) || entity.Head2D == new Vector2(-99, -99) || entity.Health == 0 || GameState.LocalPlayer == null || (!Team && entity.Team == GameState.LocalPlayer.Team) || (VisibilityCheck && !entity.Visible)) continue;

                    float distToBody = Vector2.Distance(screenCenter, entity.Position2D);
                    float distToHead = Vector2.Distance(screenCenter, entity.Head2D);

                    // skip if out of FOV or usefov is false
                    if (UseFOV && (distToBody > FovSize && distToHead > FovSize))
                        continue;

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
        public static void RenderTargetLine()
        {
            if (target == null || target.Bones == null || target.Bones == null || target.Bones.Count <= CurrentBoneIndex || GameState.renderer == null)
                return;

            GameState.renderer.DrawList.AddLine(new(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2), target.Bones[CurrentBoneIndex].Position2D, ImGui.ColorConvertFloat4ToU32(FovColor));
        }

        protected override void FrameAction()
        {
            EnableAimbot();
        }
    }
}