using ClickableTransparentOverlay;
using System;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Rage
{
    public class RCS : Classes.ThreadService
    {
        public static bool Enabled = true;
        public static float Strength = 0.5f; // in precents so 1 == 100%, 0.5 == 50% etc
        protected override void FrameAction()
        {
        }
    }
}