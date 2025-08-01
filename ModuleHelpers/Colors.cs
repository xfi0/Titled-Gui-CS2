﻿using System;
using System.Numerics;

namespace Titled_Gui.ModuleHelpers
{
    public class Colors
    {
        public static Vector4 enemyColor = new Vector4(1, 0, 0, 1); // red
        public static Vector4 teamColor = new Vector4(0, 1, 0, 1);   // green 
        public static Vector4 BoneColor = new Vector4(1f, 1f, 1f, 1f); // color of the bones

        public static bool RGB = false; // toggle for RGB color
        public static Vector4 Rgb(float speed1)
        {
            float time = (float)DateTime.Now.TimeOfDay.TotalSeconds;
            float speed = (float)(Math.Sin(time * Math.PI) + 1) / 2; // ocelate or how ever you spell it

            float r, g, b;

            if (speed < 0.5f)
            {
                r = 1f - speed * 2f;
                g = speed * 2f;
                b = 0f;
            }
            else
            {
                r = 0f;
                g = 1f - (speed - 0.5f) * 2f;
                b = (speed - 0.5f) * 2f;
            }

            return new Vector4(r, g, b, 1f); //return the color
        }
    }
}