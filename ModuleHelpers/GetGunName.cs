using Swed64;
using System;
using System.Text;
using Titled_Gui.Data;
using static Titled_Gui.Data.GameState;

namespace Titled_Gui.ModuleHelpers
{
    public class GetGunName
    {
        public static string GetGunNameFunction(Entity entity)
        {
            try
            {
                IntPtr clippingWeapon = entity.HeldWeapon;
                if (clippingWeapon == IntPtr.Zero)
                {
                    Console.WriteLine("No Weapon");
                    return "No Weapon";
                }

                IntPtr weaponData = GameState.swed.ReadPointer(clippingWeapon + 0x08);
                if (weaponData == IntPtr.Zero)
                {
                    Console.WriteLine("invalid");
                    return "Invalid Weapon";
                }

                IntPtr weaponNameAddress = GameState.swed.ReadPointer(weaponData + 0xC18);
                if (weaponNameAddress == IntPtr.Zero)
                {
                    return GetWeaponNameFromIndex(entity.WeaponIndex); //fallback
                }

                byte[] buffer = GameState.swed.ReadBytes(weaponNameAddress, 32);
                string weaponName = Encoding.ASCII.GetString(buffer).TrimEnd('\0');

                if (weaponName.StartsWith("weapon_")) // remove weapon prefix
                {
                    weaponName = weaponName.Substring(7);
                }

                return weaponName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetGunName] Error: {ex.Message}");
                return "Unknown";
            }
        }

        private static string GetWeaponNameFromIndex(short weaponIndex)
        {
            try
            {
                string weaponName = Enum.GetName(typeof(Weapons.WeaponIds), weaponIndex);
                return weaponName ?? "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}