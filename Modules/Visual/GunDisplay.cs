using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class GunDisplay
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
        public static bool Enabled = false;
        public static Vector4 TextColor = new(1, 1, 1, 1);
        public static void Draw(Entity e)
        {
            if (!Enabled || e.Health == 0 || e.PawnAddress == GameState.LocalPlayer.PawnAddress || e == null || e.CurrentWeaponName == null) return;

            string Icon = GetIcon(e.CurrentWeaponName);
            var rect = BoxESP.GetBoxRect(e);

            if (rect == null) return;

            if (!string.IsNullOrEmpty(Icon))
                GameState.renderer.drawList.AddText(Renderer.GunIconsFont, 24, new Vector2(rect.Value.TopRight.X, rect.Value.TopRight.Y - 10f), ImGui.ColorConvertFloat4ToU32(TextColor), Icon);
            
        }
        public static string GetIcon(string Weapon)
        {
            return !string.IsNullOrEmpty(Weapon) && Guns.TryGetValue(Weapon, out string? icon) ? icon : string.Empty;
        }
    }
}
