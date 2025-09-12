using System;
using System.Runtime.InteropServices;
using System.Threading;
using Titled_Gui.Classes;
using static Titled_Gui.Classes.User32;

internal static class MoveMouse
{
    public static void MouseMove(int dx, int dy)
    {
        var inputs = new Input[1];
        inputs[0].type = INPUT_MOUSE;
        inputs[0].mi = new MouseInput
        {
            dx = dx,
            dy = dy,
            dwFlags = MOUSEEVENTF_MOVE
        };
        SendInput(1, inputs, Marshal.SizeOf(typeof(Input)));
    }
}
