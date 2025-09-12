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
        public static float Smoothing = 0.5f;
        private static float CurrentSmoothing { get; set; } = 3;

        public static void EnableAimbot()
        {
            try
            {
                if (!AimbotEnable || User32.GetAsyncKeyState(AimbotKey) >= 0 || Entities == null)
                    return;

                Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);

                foreach (var entity in Entities)
                {
                    if (entity.Position2D == new Vector2(-99, -99) || entity.Head2D == new Vector2(-99, -99) || entity.Health == 0 || !Team && entity.Team == GameState.localPlayer.Team)
                        continue;

                    float distToBody = Vector2.Distance(screenCenter, entity.Position2D);
                    float distToHead = Vector2.Distance(screenCenter, entity.Head2D);

                    if (distToBody > FovSize && distToHead > FovSize)
                        continue;

                    Vector3 playerView = localPlayer.Origin + localPlayer.View;
                    Vector2 newAngles;

                    bool useHeadPosition = distToHead <= FovSize && entity?.Bones?[CurrentBoneIndex] != Vector3.Zero;
                    if (useHeadPosition) //bone pos first
                    {
                        if (entity?.Bones?[CurrentBoneIndex] != null && entity.Bones.Count > 6 && CurrentBoneV3 != Vector3.Zero)
                        {
                            CurrentBoneV3 = entity!.Bones[CurrentBoneIndex]!;
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
                                    newAngles = Calculate.CalculateAngles(playerView, entity.Position);
                                }
                            }
                            catch
                            {
                                newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                            }
                        }
                        else
                        {
                            if (entity != null)
                                newAngles = Calculate.CalculateAngles(playerView, entity.Head);
                            else
                                newAngles = Calculate.CalculateAngles(playerView, entity.Position); // if all else fails
                        }
                    }
                    else //fallback that if youre at there body flick to whatever chosen bone 
                    {
                        newAngles = Calculate.CalculateAngles(playerView, CurrentBoneV3);
                    }

                    if (float.IsNaN(newAngles.X) || float.IsNaN(newAngles.Y))
                        continue;

                    switch (CurrentAimMethod)
                    {
                        case 0:
                           
                            break;
                        case 1:
                            Vector3 newAnglesVec3 = new Vector3(newAngles.X, newAngles.Y, 0.0f);
                            newAnglesVec3.X = Math.Clamp(newAnglesVec3.X, -89f, 89f);
                            newAnglesVec3.Y = NormalizeAngle(newAnglesVec3.Y);
                            GameState.swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3); // write the view angle todo make mose movement
                            break;
                    }
                    break; //one ent
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

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180.0f) angle -= 360.0f;
            while (angle < -180.0f) angle += 360.0f;
            return angle;
        }
        protected override void FrameAction()
        {
            EnableAimbot();
        }
    }
}