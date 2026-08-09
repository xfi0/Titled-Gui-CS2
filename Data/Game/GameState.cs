using System.Diagnostics;
using Titled_Gui.Classes.Memory;
using Titled_Gui.Data.Entity;

namespace Titled_Gui.Data.Game
{
    public static class GameState
    {
        public static Memory? memory; // public swed instance to use all arround
        public static Renderer? renderer;
        public static IntPtr client; // public client
        public static IntPtr LocalPlayerPawn { get; set; } // local player pawn pointer
        public static IntPtr EntityList { get; set; } // entity list pointer
        public static IntPtr CameraServices { get; set; } // camera services pointer
        public static uint CurrentFov { get; set; } = 60; // default FOV
        public static List<Entity.Entity?> Entities { get; set; } = [];
        public static Entity.Entity? LocalPlayer { get; set; } // local player entity
        public static int crosshairEnt { get; set; }
        public static uint Fflag { get; set; }
        public static uint Standing = 65665;
        public static uint Crouching = 655667; // crouching state
        public static IntPtr MoneyServices { get; set; }
        public static uint WeaponServices { get; set; }
        public static IntPtr ActionTrackingServices { get; set; }
        public static bool IsScoped { get; set; }
        public static IntPtr LocalController { get; set; }
        public static int RoundHeadshots { get; set; }
        public static int roundKills { get; set; }
        public static int RoundDamage { get; set; }
        public static List<WorldEntity?> worldEntities { get; set; } = [];

        public static bool CS2Open() // i didnt know where to put this
        {
            return !(Process.GetProcessesByName("cs2").Length == 0);
        }
        public static Process[] CS2Processes = [];
        public static Process[] GetCS2Process()
        {
            if (CS2Processes.Length <= 0)
                CS2Processes = Process.GetProcessesByName("cs2");

            return CS2Processes;
        }
    }
}