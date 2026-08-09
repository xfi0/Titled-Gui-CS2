using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.MapParser;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Classes
{
    public class VisibilityCheck : ThreadService
    {
        public static MapLoader? mapLoaderInstance = null;

        public static bool IsEntityVisible(Entity? e)
        {
            if (e == null || GameState.LocalPlayer?.Bones == null || e.Health <= 0 || e.Position2D == new Vector2(-99, -99) || e.Bones == null || mapLoaderInstance == null || e.IsDormant || GameState.renderer == null || GameState.memory == null)
                return false;

            Vector3 origin = GameState.LocalPlayer.EyePosition;
            Vector3 target = e.Bones[(int)BoneESP.BoneIds.Head].Position; // head
            if (!EntityManager.UseOldVisibilityCheck)
                return mapLoaderInstance.IsVisible(origin, target);

            return GameState.memory.ReadBool(e.PawnAddress, Offsets.m_entitySpottedState + Offsets.m_bSpotted);
        }

        public static bool Visible(Vector3 origin, Vector3 target)
        {
            if (mapLoaderInstance == null)
                return false;

            var visible = mapLoaderInstance.IsVisible(origin, target);
            return visible;
        }

        protected override void FrameAction()
        {
            string map = GlobalVar.GetCurrentMapName().Replace("maps/", "").Replace(".vpk", "");

            if (string.IsNullOrEmpty(map) || map == "<empty>") return;

            if (mapLoaderInstance == null)
            {
                mapLoaderInstance ??= new MapLoader();

                if (!mapLoaderInstance.LoadMap(map))
                {
                    Console.WriteLine("Failed to load map: " + map);
                    return;
                }
            }
            if (mapLoaderInstance.PreviousMapName != map)
            {
                Events.GameEvents.BroadcastMapChanged(map);
                if (!mapLoaderInstance.LoadMap(map))
                {
                    Console.WriteLine("Failed to load map: " + map);
                    return;
                }
            }

            Thread.Sleep(3000);
        }
    }
}
