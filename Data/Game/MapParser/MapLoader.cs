using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.Data.Game.MapParser
{
    public class MapLoader
    {
        string cs2BaseFolder = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\";
        string mapsFolder = "maps";

        public void LoadMap(string mapName)
        {
            string filePath = Path.Combine(cs2BaseFolder, mapsFolder, mapName);
            //Console.WriteLine(filePath);
        }
    }
}
