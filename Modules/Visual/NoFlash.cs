using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public class NoFlash : Classes.ThreadService
    {
        public static bool NoFlashEnable = false;
        public static void RemoveFlash()
        {
            if (!NoFlashEnable) return;

            var FlashBangDuration = GameState.swed.ReadFloat(GameState.client, Offsets.m_flFlashBangTime);

            if (FlashBangDuration > 0)
            {
                GameState.swed.WriteInt(GameState.LocalPlayerPawn, Offsets.m_flFlashBangTime, 0);
            }
        }
        protected override void FrameAction()
        {
            RemoveFlash();
        }
    }
}
