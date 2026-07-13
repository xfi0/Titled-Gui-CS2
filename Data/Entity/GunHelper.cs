using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.Data.Entity
{
    internal class GunHelper
    {
        private static readonly Dictionary<string, string> Guns = new() // thx sweeperxz
        {
            ["knife_ct"] = "]",
            ["knife_t"] = "[",
            ["deagle"] = "A",
            ["elite"] = "B",
            ["fiveseven"] = "C",
            ["glock"] = "D",
            ["revolver"] = "J",
            ["hkp2000"] = "E",
            ["p250"] = "F",
            ["usp_silencer"] = "G",
            ["tec9"] = "H",
            ["cz75a"] = "I",
            ["mac10"] = "K",
            ["ump45"] = "L",
            ["bizon"] = "M",
            ["mp7"] = "N",
            ["mp9"] = "R",
            ["p90"] = "O",
            ["galilar"] = "Q",
            ["famas"] = "R",
            ["m4a1_silencer"] = "T",
            ["m4a1"] = "S",
            ["aug"] = "U",
            ["sg556"] = "V",
            ["ak47"] = "W",
            ["g3sg1"] = "X",
            ["scar20"] = "Y",
            ["awp"] = "Z",
            ["ssg08"] = "a",
            ["xm1014"] = "b",
            ["sawedoff"] = "c",
            ["mag7"] = "d",
            ["nova"] = "e",
            ["negev"] = "f",
            ["m249"] = "g",
            ["taser"] = "h",
            ["flashbang"] = "i",
            ["hegrenade"] = "j",
            ["smokegrenade"] = "k",
            ["molotov"] = "l",
            ["decoy"] = "m",
            ["incgrenade"] = "n",
            ["c4"] = "o"
        };

        public static string GetIcon(string weapon)
        {
            return !string.IsNullOrEmpty(weapon) && Guns.TryGetValue(weapon, out string? icon) ? icon : string.Empty;
        }
    }
}
