using K4os.Compression.LZ4.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Memory;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class NoScope : ThreadService
    {
        protected override void FrameAction()
        {
            if (GameState.memory == null || GameState.LocalPlayer == null
                || GameState.LocalPlayer.PawnAddress == IntPtr.Zero)
                return;

        }
    }
}
