using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class Chams
    {
        public static float BoneThickness = 10f;
        public static bool DrawOnSelf = false;
        public static bool EnableChams = true;

        public static void Draw(Entity? entity)
        {
            if (entity == null || entity.Bones2D == null ||
                (DrawOnSelf && entity == GameState.LocalPlayer) ||
                BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed ||
                entity.Bones2D.Count <= 0 || !EnableChams)
                return;
                
        }
    }
}
