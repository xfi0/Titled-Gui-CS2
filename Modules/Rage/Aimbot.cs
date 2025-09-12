using ImGuiNET;
using Swed64;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Titled_Gui.Classes;
using Titled_Gui.Data;
using Titled_Gui.Modules.Visual;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
using Point = System.Drawing.Point;

namespace Titled_Gui.Modules.Rage 
{
    public class Aimbot : Classes.ThreadService
    {
        // aimbot variables
        public static bool AimbotEnable = false;
        public static bool Team = false;
        public static int FovSize = 100;
        public static Vector4 FovColor = new Vector4(1f, 0f, 0f, 1f);
        public static bool DrawFov = true;
        public const int AimbotKey = 0x04; // mmb TODO Make Changable
        public static string[] Bones = new string[] { "Head", "Neck", "Right Sholder", "Left Sholder", "Waist", "Random" };
        public static int CurrentBone = 0;
        public static int CurrentBoneIndex = 2;
        public static Vector3 CurrentBoneV3;
        public static bool RandomChosen = false;
        public static string[] AimbotMethods = ["Simulate Mouse Movement", "Memory Writing"];
        public static int CurrentAimMethod = 0;
        public static float Smoothing = 10f;
        private static float CurrentSmoothing { get; set; } = 3;

        public static void EnableAimbot()
        {
            try
            {
                if (!AimbotEnable || User32.GetAsyncKeyState(AimbotKey) >= 0 || Entities == null || GameState.localPlayer.Health == 0) { RandomChosen = false; return; }

                Entity? target = GetTarget();
                if (target == null)
                    return;

                Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);
                Vector3 playerView = localPlayer.Origin + localPlayer.View;
                Vector2 newAngles;

                bool useHeadPosition = target?.Bones?[CurrentBoneIndex] != Vector3.Zero;
                if (useHeadPosition) //bone pos first
                {
                    if (target?.Bones?[CurrentBoneIndex] != null && target.Bones.Count > 6 && CurrentBoneV3 != Vector3.Zero)
                    {
                        CurrentBoneV3 = target!.Bones[CurrentBoneIndex]!;
                        try
                        {
                            switch (CurrentBone)
                            {
                                case 0:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 2;
                                    break;
                                case 1:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 1;
                                    break;
                                case 2:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 6;
                                    break;
                                case 3:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 3;
                                    break;
                                case 4:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 0;
                                    break;
                                case 5:
                                    Random random = new();
                                    if (!RandomChosen)
                                    {
                                        CurrentBoneIndex = random.Next(12);
                                        RandomChosen = true;
                                    }
                                    break;
                                default:
                                    if (RandomChosen)
                                        RandomChosen = false;
                                    CurrentBoneIndex = 2;
                                    break;
                            }
                            if (CurrentBoneV3 != Vector3.Zero)
                            {
                                newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                            }
                            else
                            {
                                newAngles = Calculate.CalculateAngles(playerView, target.Position);
                            }
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
                            Vector2 newAngles2D = target?.Head2D != Vector2.Zero ? target!.Head2D : target.Position2D;
                            int dx = (int)(newAngles2D.X - screenCenter.X);
                            int dy = (int)(newAngles2D.Y - screenCenter.Y);

                            dx = (int)(dx / Smoothing);
                            dy = (int)(dy / Smoothing);

                            MoveMouse.MouseMove(dx, dy); // move the mouse
                        }
                        break;


                    case 1: // write
                        Vector3 newAnglesVec3 = new Vector3(newAngles.X, newAngles.Y, 0.0f);
                        newAnglesVec3.X = Math.Clamp(newAnglesVec3.X, -89f, 89f);
                        newAnglesVec3.Y = Calculate.NormalizeAngle(newAnglesVec3.Y);
                        GameState.swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3); // write the view angle
                        break;
                }
            }
            catch (Exception)
            {
            }
        }

        public static void DrawCircle(int size, Vector4 color) // draw fov circle 
        {
            Vector4 circleColor = color; // set the circle color based on the Team  
            float radius = size; // use the size to determine radius
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
                    if (entity.Position2D == new Vector2(-99, -99) || entity.Head2D == new Vector2(-99, -99) || entity.Health == 0 || (!Team && entity.Team == GameState.localPlayer.Team)) continue;

                    float distToBody = Vector2.Distance(screenCenter, entity.Position2D);
                    float distToHead = Vector2.Distance(screenCenter, entity.Head2D);

                    // skip if out of FOV
                    if (distToBody > FovSize && distToHead > FovSize)
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

        protected override void FrameAction()
        {
            EnableAimbot();
        }
    }
}