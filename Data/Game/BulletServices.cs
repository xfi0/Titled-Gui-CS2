using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace Titled_Gui.Data.Game
{
    internal class BulletServices : Classes.ThreadService
    {
        public static void Update()
        {
            //GameState.bulletServices = GameState.swed.ReadPointer(GameState.LocalPlayerPawn + Offsets.m_pBulletServices);
        }
        protected override void FrameAction()
        {
            //Update();
        }
    }
}
