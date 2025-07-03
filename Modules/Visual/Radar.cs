using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Swed64;

namespace Titled_Gui.Modules.Visual
{
    public class Radar
    {
        Renderer renderer = new Renderer();
        static Swed swed = new Swed("cs2");
        public static bool EnableRadarBool = false;
        public static void EnableRadar()
        {
            //doesnt work plus writing is detecdd by vac
            //swed.WriteBool(GameState.currentPawn, Offsets.m_entitySpottedState + Offsets.m_bSpotted, true); //sets the spotted bool to true enabling them on radar, but doesnt work and writing mem is detected
        }
    }
}
