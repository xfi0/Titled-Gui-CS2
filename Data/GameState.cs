using Swed64;
using System;
using System.Collections.Generic;

namespace Titled_Gui.Data
{
    public static class GameState
    {
        static Swed swed = new Swed("cs2"); // Swed instance for reading memory
        public static IntPtr LocalPlayerPawn { get; set; } // local player pawn pointer
        public static IntPtr EntityList { get; set; } // entity list pointer
        public static IntPtr CameraServices { get; set; } // camera services pointer
        public static uint CurrentFov { get; set; } = 60; // default FOV
        public static IntPtr currentPawn { get; set; } // get current pawn pointer
        public static IntPtr client = swed.GetModuleBase("client.dll"); // public client
        public static List<Entity> Entities { get; set; } = new List<Entity>();
        public static Entity localPlayer { get; set; } = new Entity(); // local player entity
        public static IntPtr BoneMatrix { get; set; } // bone matrix pointer
        public static IntPtr SceneNode { get; set; } // scene node pointer

    }
}