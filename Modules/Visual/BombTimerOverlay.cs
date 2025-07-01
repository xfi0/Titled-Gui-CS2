using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Modules.Visual
{
    public class BombTimerOverlay
    {
        public static bool EnableTimeOverlay = false;
        public static void TimeOverlay(Renderer renderer)
        {
            uint color = 0xFF0000FF; //maybe make changeable later?  
            renderer.drawList.AddText(new Vector2(2560f, 1440f), color, "Hello World!");
        }
    }
}
