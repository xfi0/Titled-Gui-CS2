using System;
using System.Collections.Generic;
using System.Text;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Data.Game.MapParser
{
    public class MapLoader
    {
        string cs2BaseFolder = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\";
        string mapsFolder = "maps";

        public bool IsMapLoaded { get; internal set; }

        public void LoadMap(string mapName)
        {
            string filePath = Path.Combine(cs2BaseFolder, mapsFolder, mapName);
            //Console.WriteLine(filePath);
        }

        internal float RayCast(WallPenetration.Ray toEnemy, float v)
        {
            throw new NotImplementedException();
        }

        internal float? RayCast(WallTrigger.Ray ray, float v)
        {
            throw new NotImplementedException();
        }
    }
}
