using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{
    public class NoFlash
    {
        static Swed swed = new Swed("cs2");
        Renderer renderer = new Renderer();
        Program program = new Program();
        public static bool NoFlashEnable = false;
        public static void RemoveFlash()
        {
            float FlashBangDuration = swed.ReadFloat(GameState.client, Offsets.m_flFlashBangTime);
            if (NoFlashEnable && FlashBangDuration > 0)
            {
                swed.WriteInt(GameState.LocalPlayerPawn, Offsets.m_flFlashBangTime, 0);
            }
        }
    }
}
