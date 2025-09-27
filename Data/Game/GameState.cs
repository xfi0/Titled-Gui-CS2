using ImGuiNET;
using Swed64;

namespace Titled_Gui.Data.Game
{
    public static class GameState
    {
        public static Swed swed = new("cs2"); // public swed instance to use all arround
        public static Renderer renderer = new();
        public static IntPtr client = swed.GetModuleBase("client.dll"); // public client
        public static IntPtr LocalPlayerPawn { get; set; } // local player pawn pointer
        public static IntPtr EntityList { get; set; } // entity list pointer
        public static IntPtr CameraServices { get; set; } // camera services pointer
        public static uint CurrentFov { get; set; } = 60; // default FOV
        public static IntPtr currentPawn { get; set; } // get current pawn pointer
        public static List<Entity.Entity> Entities { get; set; } = [];
        public static Entity.Entity LocalPlayer { get; set; } = new Entity.Entity(); // local player entity
        public static IntPtr BoneMatrix { get; set; } // bone matrix pointer
        public static IntPtr SceneNode { get; set; } // scene node pointer
        public static IntPtr GameRules { get; set; } // game rules pointer
        public static IntPtr ForceAttack { get; set; } // sensitivity pointer
        public static IntPtr ForceJump { get; set; } // sensitivity pointer
        public static int crosshairEnt {  get; set; }
        public static uint Fflag  { get; set; }
        public static uint Standing = 65665;
        public static uint Crouching = 655667; // crouching state
        public static bool BombPlanted { get; set; }
        public static IntPtr MoneyServices { get; set; }
        public static uint WeaponServices { get; set; }
        public static IntPtr ActionTrackingServices { get; set; }
        public static IntPtr BulletServices { get; set; }
        public static IntPtr currentController = IntPtr.Zero;
        public static bool IsScoped { get; set; }
        public static IntPtr LocalController { get; set; }
        public static int RoundHeadshots {  get; set; }
        public static int RoundKills { get; set; }
        public static int RoundDamage { get; set; }
    }
}