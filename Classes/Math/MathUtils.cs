using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes.Math
{
    internal class MathUtils
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos)
        {
            if (GameState.renderer == null)
                return new(-99, -99);

            float view = matrix[12] * pos.X + matrix[13] * pos.Y + matrix[14] * pos.Z + matrix[15];

            if (view <= 0.01f)
                return new(-99, -99);

            float screenX = matrix[0] * pos.X + matrix[1] * pos.Y + matrix[2] * pos.Z + matrix[3];
            float screenY = matrix[4] * pos.X + matrix[5] * pos.Y + matrix[6] * pos.Z + matrix[7];

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
        public static float[] MatFromPosQuat(Vector3 p, Quaternion q)
        {
            float x = q.X, y = q.Y, z = q.Z, w = q.W;
            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = w * x2, wy = w * y2, wz = w * z2;

            return
            [
                1f - (yy + zz),
                xy - wz, xz + wy,
                p.X,
                xy + wz,
                1f - (xx + zz),
                yz - wx,
                p.Y,
                xz - wy,
                yz + wx,
                1f - (xx + yy),
                p.Z,
            ];
        }

        public static float[] MatMul(float[] a, float[] b)
        {
            var r = new float[12];
            for (int i = 0; i < 3; i++)
            {
                int o = i * 4;
                r[o] = a[o] * b[0] + a[o + 1] * b[4] + a[o + 2] * b[8];
                r[o + 1] = a[o] * b[1] + a[o + 1] * b[5] + a[o + 2] * b[9];
                r[o + 2] = a[o] * b[2] + a[o + 1] * b[6] + a[o + 2] * b[10];
                r[o + 3] = a[o] * b[3] + a[o + 1] * b[7] + a[o + 2] * b[11] + a[o + 3];
            }
            return r;
        }

        public static float[] MatMul(float[] a, ReadOnlySpan<float> b)
        {
            var r = new float[12];
            for (int i = 0; i < 3; i++)
            {
                int o = i * 4;
                r[o] = a[o] * b[0] + a[o + 1] * b[4] + a[o + 2] * b[8];
                r[o + 1] = a[o] * b[1] + a[o + 1] * b[5] + a[o + 2] * b[9];
                r[o + 2] = a[o] * b[2] + a[o + 1] * b[6] + a[o + 2] * b[10];
                r[o + 3] = a[o] * b[3] + a[o + 1] * b[7] + a[o + 2] * b[11] + a[o + 3];
            }
            return r;
        }

        public static float[] MatInvertRigid(float[] m)
        {
            var inv = new float[12];
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    inv[i * 4 + j] = m[j * 4 + i];

            inv[3] = -(inv[0] * m[3] + inv[1] * m[7] + inv[2] * m[11]);
            inv[7] = -(inv[4] * m[3] + inv[5] * m[7] + inv[6] * m[11]);
            inv[11] = -(inv[8] * m[3] + inv[9] * m[7] + inv[10] * m[11]);
            return inv;
        }

        public static void MatFromPosQuat(Vector3 p, Quaternion q, float[] dest, int base_)
        {
            float x = q.X, y = q.Y, z = q.Z, w = q.W;
            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = w * x2, wy = w * y2, wz = w * z2;

            dest[base_ + 0] = 1f - (yy + zz); dest[base_ + 1] = xy - wz; dest[base_ + 2] = xz + wy; dest[base_ + 3] = p.X;
            dest[base_ + 4] = xy + wz; dest[base_ + 5] = 1f - (xx + zz); dest[base_ + 6] = yz - wx; dest[base_ + 7] = p.Y;
            dest[base_ + 8] = xz - wy; dest[base_ + 9] = yz + wx; dest[base_ + 10] = 1f - (xx + yy); dest[base_ + 11] = p.Z;
        }

        public static void MatMulTo(float[] a, int ia, float[] b, int ib, float[] out_, int oo)
        {
            for (int i = 0; i < 3; i++)
            {
                int o = oo + i * 4;
                int ra = ia + i * 4;
                int rb = ib;
                out_[o] = a[ra] * b[rb] + a[ra + 1] * b[rb + 4] + a[ra + 2] * b[rb + 8];
                out_[o + 1] = a[ra] * b[rb + 1] + a[ra + 1] * b[rb + 5] + a[ra + 2] * b[rb + 9];
                out_[o + 2] = a[ra] * b[rb + 2] + a[ra + 1] * b[rb + 6] + a[ra + 2] * b[rb + 10];
                out_[o + 3] = a[ra] * b[rb + 3] + a[ra + 1] * b[rb + 7] + a[ra + 2] * b[rb + 11] + a[ra + 3];
            }
        }
    }
}
