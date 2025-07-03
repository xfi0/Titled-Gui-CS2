using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.Structure;
using Titled_Gui.Data;
//using Emgu.CV.BitMap;

namespace Titled_Gui.Modules.Rage
{
    public class CVTriggerBot
    {
        public static bool Enabled = false;

        private static Thread? scanThread;
        private static bool stopRequested = false;
        private static Image<Bgr, byte>? targetTemplate;

        public static void Start(Renderer renderer)
        {
            GameState.ForceAttack = GameState.client + Offsets.attack;
            foreach (var e in renderer.entities)
            {
                if (e == null)
                    return;

                if (e.Visible)
                {
                    GameState.swed.WriteInt(GameState.ForceAttack, 65537);
                    Thread.Sleep(1);
                    GameState.swed.WriteInt(GameState.ForceAttack, 250);
                }
            }
        }
    }
}
