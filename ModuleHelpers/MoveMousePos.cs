using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace Titled_Gui.ModuleHelpers
{
    internal class MoveMousePos
    {
        private const int MOUSEEVENTF_MOVE = 0x0001;
        private const int HC_ACTION = 0;
        private const int WH_MOUSE_LL = 14;

        [DllImport("user32.dll")]
        private static extern void mouse_event(int dwFlags, int dx, int dy, int dwData, int dwExtraInfo);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT
        {
            public int X;
            public int Y;
        }

        public static void MoveRelative(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return;

            mouse_event(MOUSEEVENTF_MOVE, dx, dy, 0, 0);
        }

        public static void MoveTo(int targetX, int targetY, float smoothness)
        {
            if (smoothness <= 0f)
            {
                MoveAbsolute(targetX, targetY);
                return;
            }

            if (!GetCursorPos(out var current))
                return;

            int deltaX = targetX - current.X;
            int deltaY = targetY - current.Y;

            if (Math.Abs(deltaX) <= 2 && Math.Abs(deltaY) <= 2)
            {
                MoveAbsolute(targetX, targetY);
                return;
            }

            float moveSpeed = Math.Max(0.1f, Math.Min(1.0f, smoothness));

            int moveX = (int)(deltaX * moveSpeed);
            int moveY = (int)(deltaY * moveSpeed);

            if (moveX == 0 && Math.Abs(deltaX) > 0)
                moveX = deltaX > 0 ? 1 : -1;
            if (moveY == 0 && Math.Abs(deltaY) > 0)
                moveY = deltaY > 0 ? 1 : -1;

            if (moveX != 0 || moveY != 0)
            {
                MoveRelative(moveX, moveY);
            }
        }

        public static void MoveToSmooth(int targetX, int targetY, float smoothness, int maxSteps = 20)
        {
            if (smoothness <= 0f)
            {
                MoveAbsolute(targetX, targetY);
                return;
            }

            if (!GetCursorPos(out var startPos))
                return;

            float startX = startPos.X;
            float startY = startPos.Y;

            float totalDistance = (float)Math.Sqrt(
                Math.Pow(targetX - startX, 2) + Math.Pow(targetY - startY, 2)
            );

            if (totalDistance <= 3f)
            {
                MoveAbsolute(targetX, targetY);
                return;
            }

            int steps = Math.Max(3, Math.Min(maxSteps, (int)(totalDistance / 10f * smoothness)));

            for (int i = 1; i <= steps; i++)
            {
                float t = (float)i / steps;

                float easedT = EaseOutCubic(t);

                int newX = (int)(startX + (targetX - startX) * easedT);
                int newY = (int)(startY + (targetY - startY) * easedT);

                MoveAbsolute(newX, newY);

                int sleepTime = Math.Max(1, (int)(5f / Math.Max(0.1f, smoothness)));
                Thread.Sleep(sleepTime);
            }

            MoveAbsolute(targetX, targetY);
        }

        private static float EaseOutCubic(float t)
        {
            return 1f - (float)Math.Pow(1f - t, 3);
        }

        public static void MoveAbsolute(int x, int y)
        {
            SetCursorPos(x, y);
        }
    }
}