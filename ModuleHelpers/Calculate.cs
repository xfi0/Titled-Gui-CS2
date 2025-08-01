﻿using Swed64;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Titled_Gui.Modules.Visual.BoneESP;

namespace Titled_Gui.ModuleHelpers
{
    public static class Calculate
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos, Vector2 windowSize)
        {
            // ccalculate w aka depth
            float screenW = matrix[12] * pos.X + matrix[13] * pos.Y + matrix[14] * pos.Z + matrix[15];

            // if entity is visible
            if (screenW > 0.001f)
            {
                //calculate screen x and y
                float screenX = matrix[0] * pos.X + matrix[1] * pos.Y + matrix[2] * pos.Z + matrix[3];
                float screenY = matrix[4] * pos.X + matrix[5] * pos.Y + matrix[6] * pos.Z + matrix[7];

                //prespective division 
                float X = windowSize.X / 2 + windowSize.X / 2 * screenX / screenW;
                float Y = windowSize.Y / 2 - windowSize.Y / 2 * screenY / screenW;

                //now return the Cordinates
                return new Vector2(X, Y);
            }
            else
            {
                // if entity is not visible, return a negative vector idk why but eveyone does it sao
               return new Vector2(-99, -99);
            }
        }

        public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
        {
            Vector3 delta = new Vector3(to.X - from.X, to.Y - from.Y, to.Z - from.Z);

            float distance = (float)Math.Sqrt(delta.X * delta.X + delta.Y * delta.Y);

            float pitch = (float)(-Math.Atan2(delta.Z, distance) * 180 / Math.PI);
            float yaw = (float)(Math.Atan2(delta.Y, delta.X) * 180 / Math.PI);

            pitch = NormalizeAngle(pitch);
            yaw = NormalizeAngle(yaw);

            return new Vector2(pitch, yaw);
        }

        private static float NormalizeAngle(float angle)
        {
            while (angle > 180)
                angle -= 360;
            while (angle < -180)
                angle += 360;

            return angle;
        }
        public static List<Vector3> ReadBones(nint boneAddress, Swed swed)
        {
            byte[] boneBytes = swed.ReadBytes(boneAddress, 27 * 32 + 16);
            List<Vector3> bones = new List<Vector3>();
            foreach (var boneId in Enum.GetValues(typeof(BoneIds)))
            {
                float x = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 0);
                float y = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 4);
                float z = BitConverter.ToSingle(boneBytes, (int)boneId * 32 + 8);
                Vector3 currentBone = new Vector3(x, y, z);
                bones.Add(currentBone);
            }
            return bones;
        }

        public static List<Vector2> ReadBones2D(List<Vector3> bones, float[] viewMatrix, Vector2 screenSize)
        {
            List<Vector2> bones2d = new List<Vector2>();
            foreach (Vector3 bone in bones)
            {
                Vector2 bone2d = WorldToScreen(viewMatrix, bone, new Vector2((int)screenSize.X, (int)screenSize.Y));
                bones2d.Add(bone2d);
            }
            return bones2d;
        }
    }
}
