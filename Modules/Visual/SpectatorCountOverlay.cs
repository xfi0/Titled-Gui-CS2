using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SpectatorCountOverlay : Classes.ThreadService
    {
        public static void Update()
        {
            //Console.WriteLine(GameState.swed.ReadInt(GameState.GameRules + Offsets.m_iSpectatorSlotCount));
        }
        protected override void FrameAction()
        {
            Update();
        }
    }
}
