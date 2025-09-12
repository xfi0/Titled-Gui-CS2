using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{
    internal class Radar 
    {
        public static bool IsEnabled = false;
        public static float Size = 10f; // idk size
        public static void DrawRadar()
        {
            if (!IsEnabled) return;

            var DrawList = GameState.renderer.drawList;

            foreach (var e in GameState.Entities)
            {
                if (e != null && e.Health != 0 && e.Team != GameState.localPlayer.Team)
                {

                }
            }
        }
    }
}
