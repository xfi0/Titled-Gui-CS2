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
    }
}