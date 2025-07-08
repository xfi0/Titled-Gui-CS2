using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.Data.EntityManager;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;

namespace Titled_Gui.Modules.Legit
{
    //flick and flick back idk
    public class SlientAimbot
    {
        public static bool AimbotEnable = false;
        public static bool Team = false;
        public static int FovSize = 100;
        public static Vector4 FovColor = new Vector4(1f, 0f, 0f, 1f);
        public static bool DrawFOV = true;

        static Swed swed = new Swed("cs2");
        public const int AIMBOT_KEY = 0x04;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        public static void EnableAimbot()
        {
        }
    }
}
