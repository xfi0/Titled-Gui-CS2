using System.Runtime.InteropServices;
using static Titled_Gui.Classes.User32;

internal static class MoveMouse
{
    public static void MouseMove(int dx, int dy)
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].U.mi = new MOUSEINPUT
        {
            dx = dx,
            dy = dy,
            dwFlags = MOUSEEVENTF_MOVE
        };
        SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
    }
}
