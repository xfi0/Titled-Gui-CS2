using Swed64;
using System.Text;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

public static class WeaponReader // this is not a good way to do it but it works so
{
    public static string GetWeaponName(Entity entity)
    {
        if (entity.Health == 0) return "Invalid";

        nint M_ClippingWeapon = swed.ReadPointer(entity.PawnAddress + Offsets.m_pClippingWeapon);
        if (M_ClippingWeapon == 0) return "Invalid";

        nint First = swed.ReadPointer(M_ClippingWeapon + 0x200);
        if (First == 0) return "Invalid";

        nint WeaponData = swed.ReadPointer(First + 0x20);
        if (WeaponData == 0) return "Invalid";

        byte[] Dump = swed.ReadBytes(WeaponData, 0x10);
        string ASCIIString = Encoding.ASCII.GetString(Dump);

        int idx = ASCIIString.IndexOf("weapon_"); if (idx < 0) return "Unkown";

        string raw = ASCIIString[idx..];
        int index = raw.IndexOf('\0');
        if (index >= 0) raw = raw.Substring(0, index);

        return raw.StartsWith("weapon_") ? raw.Substring(7) : raw;
    }

    public static void UpdateEntityWeaponName(Entity entity)
    {
        if (entity.Health == 0) return;

        entity.CurrentWeaponName = GetWeaponName(entity);
        //Console.WriteLine($"Weapon Name: {entity.CurrentWeaponName}");
    }
}
