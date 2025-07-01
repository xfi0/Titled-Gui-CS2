using Swed64;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;
namespace Titled_Gui.Modules.Rage
{
    public class Aimbot
    {
        // aimbot variables
        public static bool AimbotEnable = false; // toggle for aimbot
        public static bool Team = false; // if true aimbot will target teammates, if false it will target both
        public static int FovSize = 100; // how big the fov circle is
        public static Vector4 FovColor = new Vector4(1f, 0f, 0f, 1f); // color of the fov circle
        public static bool DrawFOV = true;
        static Swed swed = new Swed("cs2");
        public const int AIMBOT_KEY = 0x46; // F key

        // other stuff
        Entity entity = new Entity();
        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        private static DateTime lastPrint = DateTime.MinValue;

        public static void EnableAimbot()
        {
            try
            {
                if (!AimbotEnable || GetAsyncKeyState(AIMBOT_KEY) >= 0)
                    return;

                if (localPlayer == null || Entities == null)
                {
                    Console.WriteLine("LocalPlayer or Entities is null");
                    return;
                }

                foreach (var e in Entities)
                {
                    if (e == null)
                    {
                        continue;
                    }

                    if (e.head2D == new Vector2(-99, -99))
                    {
                        continue;
                    }

                    float distToCrosshair = Vector2.Distance(new Vector2(1280, 720), e.head2D);
                    if (distToCrosshair > FovSize) continue;

                    Vector3 playerView = localPlayer.origin + localPlayer.view;
                    Vector2 newAngles;

                    if (e.bones != null && e.bones.Count > 6)
                    {
                        try
                        {
                            Vector3 headBone = e.bones[2];  // head
                            Vector3 neckBone = e.bones[1];  // neck
                            
                            if (headBone != Vector3.Zero && neckBone != Vector3.Zero)
                            {
                                Vector3 forward = Vector3.Normalize(headBone - neckBone);
                                Vector3 facePos = headBone + forward * 2.5f;
                                
                                newAngles = Calculate.CalculateAngles(playerView, facePos);
                            }
                            else
                            {
                                newAngles = Calculate.CalculateAngles(playerView, e.head);
                            }
                        }
                        catch (Exception boneEx)
                        {
                            newAngles = Calculate.CalculateAngles(playerView, e.head);
                        }
                    }
                    else
                    {
                        newAngles = Calculate.CalculateAngles(playerView, e.head);
                    }

                    if (float.IsNaN(newAngles.X) || float.IsNaN(newAngles.Y))
                    {
                        continue;
                    }

                    Vector3 newAnglesVec3 = new Vector3(newAngles.X, newAngles.Y, 0.0f);

                    newAnglesVec3.X = Math.Clamp(newAnglesVec3.X, -89f, 89f);
                    newAnglesVec3.Y = NormalizeAngle(newAnglesVec3.Y);
                    
                    swed.WriteVec(client, Offsets.dwViewAngles, newAnglesVec3);
                    break;
                }
            }
            catch (Exception ex)
            {
            }
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180.0f) angle -= 360.0f;
            while (angle < -180.0f) angle += 360.0f;
            return angle;
        }

        private static bool FovCheck(Vector2 screenCenter, Vector2 targetPos, float fovSize)
        {
            float distance = Vector2.Distance(screenCenter, targetPos);
            return distance <= fovSize;
        }
    }
}
