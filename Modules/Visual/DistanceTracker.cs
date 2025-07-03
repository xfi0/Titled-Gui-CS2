using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class DistanceTracker
    {
        public static bool EnableDistanceTracker = true; // toggle for distance tracker
        public static int TrackDistance()
        {
            if (Entities == null)
                return 0;

            int closestDistance = int.MaxValue; // init

            foreach (var e in Entities)
            {
                int Distance = (int)e.distance; // get the distance of the entity'
                if (Distance < 0)
                {
                    Distance = 0; // if the distance is negative, set it to 0
                }
                else if (Distance < closestDistance)
                {
                    closestDistance = Distance; // update closest distance
                }
            }

            return closestDistance == int.MaxValue ? 0 : closestDistance; // return the distance of the entity
        }
    }
}
