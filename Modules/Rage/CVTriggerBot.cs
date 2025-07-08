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
    public class CVTriggerBot //was gonna be cv but nah
    {
        public static bool Enabled = false;

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
                    GameState.swed.WriteInt(GameState.ForceAttack, 256);
                }
            }
        }
    }
}
