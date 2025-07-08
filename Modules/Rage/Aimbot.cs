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
using Titled_Gui.Data;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
using System.Windows.Input;
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui.Modules.Rage
{
    public class Aimbot
    {
        // aimbot variables
        public static bool AimbotEnable = false;
        public static bool Team = false;
        public static int FovSize = 100;
        public static Vector4 FovColor = new Vector4(1f, 0f, 0f, 1f);
        public static bool DrawFOV = true;
        static Swed swed = new Swed("cs2");
        public const int AIMBOT_KEY = 0x04; // mmb

        Entity entity = new Entity();
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public static void EnableAimbot(Renderer renderer)
        {
            try
            {
                if (!AimbotEnable || GetAsyncKeyState(AIMBOT_KEY) >= 0)
                    return;

                if (Entities == null)
                    return;

                Vector2 screenCenter = new Vector2(renderer.screenSize.X / 2, renderer.screenSize.Y / 2); 

                foreach (var e in Entities)
                {
                    if (e.position2D == new Vector2(-99, -99) || e.head2D == new Vector2(-99, -99))
                        continue;

                    float distToBody = Vector2.Distance(screenCenter, e.position2D);
                    float distToHead = Vector2.Distance(screenCenter, e.head2D);

                    if (distToBody > FovSize && distToHead > FovSize)
                        continue;

                    Vector3 playerView = localPlayer.origin + localPlayer.view;
                    Vector2 newAngles;

                    bool useHeadPosition = distToHead <= FovSize && e.head != Vector3.Zero;

                    if (useHeadPosition) //bone pos first
                    {
                        if (e.bones != null && e.bones.Count > 6)
                        {
                            try
                            {
                                Vector3 headBone = e.bones[2];  // head
                                if (headBone != Vector3.Zero)
                                {
                                    newAngles = Calculate.CalculateAngles(playerView, headBone);
                                }
                                else
                                {
                                    newAngles = Calculate.CalculateAngles(playerView, e.head);
                                }
                            }
                            catch
                            {
                                newAngles = Calculate.CalculateAngles(playerView, e.head);
                            }
                        }
                        else
                        {
                            newAngles = Calculate.CalculateAngles(playerView, e.head);
                        }
                    }
                    else //fallback that if youre at there body flick to head
                    {
                        newAngles = Calculate.CalculateAngles(playerView, e.head);
                    }

                    if (float.IsNaN(newAngles.X) || float.IsNaN(newAngles.Y))
                        continue;

                    Vector3 newAnglesVec3 = new Vector3(newAngles.X, newAngles.Y, 0.0f);
                    newAnglesVec3.X = Math.Clamp(newAnglesVec3.X, -89f, 89f);
                    newAnglesVec3.Y = NormalizeAngle(newAnglesVec3.Y);

                    swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
                    break; //one ent
                }
            }
            catch (Exception )
            {
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180.0f) angle -= 360.0f;
            while (angle < -180.0f) angle += 360.0f;
            return angle;
        }
    }
}