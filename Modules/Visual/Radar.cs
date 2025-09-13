using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using ImGuiNET;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
  
    internal class Radar 
    {
        public static bool IsEnabled = true;
        public static Vector4 PointColor = new Vector4(1f, 1f, 1f, 1f);

        public static void DrawRadar()
        {
            if (!IsEnabled) return;
            DrawPoints();

        }
        public static void DrawPoints()
        {
            if (!IsEnabled) return;

            foreach (var e in GameState.Entities)
            {
            }
        }

    }
}