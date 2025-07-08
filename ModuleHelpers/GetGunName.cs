using Swed64;
using System;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;
using static Titled_Gui.Data.Entity;
using static Titled_Gui.Data.GameState;

namespace Titled_Gui.ModuleHelpers
{
    public class GetGunName // TODO fix
    {
        public static string GetGunNameFunction(Entity entity)
        {
            entity.HeldWeaponName = Enum.GetName(typeof(Weapons.WeaponIds), WeaponIndex);
            //string name = entity.HeldWeaponName.ToString(); ;
            //Console.WriteLine($"name{name}");
            return "null";
        }
    }
}
