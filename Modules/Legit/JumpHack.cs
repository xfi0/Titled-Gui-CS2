using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    internal class JumpHack : Classes.ThreadService 
    {
        public static bool JumpHackEnabled = false;
        public static int JumpHotkey = 0x20;
        public static void JumpShot()
        {
            if (!JumpHackEnabled || GameState.LocalPlayer.Health == 0 || GameState.Entities == null) return;

            if (User32.GetAsyncKeyState(JumpHotkey) < 0 && GameState.LocalPlayer.Velocity.Z > 287)
            {
                User32.Click();
            }
        }
        protected override void FrameAction()
        {
            JumpShot();
        }
    }
}
