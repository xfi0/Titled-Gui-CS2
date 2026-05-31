using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes.Math
{
    internal class MathUtils // wouldve liked to just name math but system.math got in the way
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos) // this seemed slightly better or same idk
        {
            // calculate depth
            float view = matrix[12] * pos.X + matrix[13] * pos.Y + matrix[14] * pos.Z + matrix[15];

            // if entity is not visible
            if (view <= 0.01f)
                return new(-99, -99); // if entity is not visible


            // calculate screen x and y
            float screenX = matrix[0] * pos.X + matrix[1] * pos.Y + matrix[2] * pos.Z + matrix[3];
            float screenY = matrix[4] * pos.X + matrix[5] * pos.Y + matrix[6] * pos.Z + matrix[7];

            // perspective division 
            float halfW = GameState.renderer.ScreenSize.X * 0.5f;
            float halfH = GameState.renderer.ScreenSize.Y * 0.5f;

            float X = halfW + (screenX / view) * halfW;
            float Y = halfH - (screenY / view) * halfH;

            if (X < -halfW || X > halfW * 3 || Y < -halfH || Y > halfH * 3)
                return new(-99, -99);

            return new(X, Y);
        }

        public static Vector3 AngleToForward(Vector3 angles)
        {
            float pitch = angles.X * (MathF.PI / 180f);
            float yaw = angles.Y * (MathF.PI / 180f);

            return new Vector3(MathF.Cos(pitch) * MathF.Cos(yaw), MathF.Cos(pitch) * MathF.Sin(yaw), -MathF.Sin(pitch));
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }
        public static Vector3 RotateByQuaternion(float qX, float qY, float qZ, float qW, Vector3 v)
        {
            Vector3 u = new(qX, qY, qZ);
            float s = qW;

            return 2f * Vector3.Dot(u, v) * u + (s * s - Vector3.Dot(u, u)) * v + 2f * s * Vector3.Cross(u, v);
        }
        public static Vector3 RotateCorner(Vector3 origin, float x, float y, float z, Vector3 right, Vector3 forward, Vector3 up)
        {
            return origin + right * x + forward * +up * z;
        }
        public static float Cross(Vector2 o, Vector2 a, Vector2 b) => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);

        public static Vector3 Extend(Vector3 from, Vector3 to, float distance)
        {
            return from + Vector3.Normalize(to - from) * distance;
        }
    }
}
