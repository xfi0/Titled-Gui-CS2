using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using System.Windows.Forms;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class WorldESP // pasted asf
    {
        public static bool ChickenESP = true;
        public static bool DroppedWeaponESP = true;
        public static bool ProjectileESP = true;
        public static bool HostageESP = true;
        public static Vector4 WeaponTextColor = new(1, 1, 1, 1);
        public static Vector4 ProjectileTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenBoxColor = new(1, 1, 1, 1);
        public static Vector4 HostageTextColor = new(1, 1, 1, 1);
        public static Vector4 HostageBoxColor = new(1, 1, 1, 1);

        private static Dictionary<string, string> EntityType = new() {
            {"chicken", "Chicken"},
            {"hostage_entity", "Hostage"}
        };

        private static Dictionary<string, string> ProjectilesType = new() {
            {"smokegrenade_projectile", "Smoke Grenade"},
            {"flashbang_projectile", "Flashbang"},
            {"hegrenade_projectile", "HE Grenade"},
            {"molotov_projectile", "Molotov"},
            {"incendiarygrenade_projectile", "Incendiary Grenade"},
            {"decoy_projectile", "Decoy Grenade"}
        };

        private static Dictionary<string, string> WeaponsType = new(){
            {"weapon_ak47", "AK-47"},
            {"weapon_m4a1", "M4A1"},
            {"weapon_awp", "AWP"},
            {"weapon_elite", "Elite"},
            {"weapon_famas", "Famas"},
            {"weapon_flashbang", "Flashbang"},
            {"weapon_g3sg1", "G3SG1"},
            {"weapon_galilar", "Galil AR"},
            {"weapon_healthshot", "Health Shot"},
            {"weapon_hegrenade", "HE Grenade"},
            {"weapon_incgrenade", "Incendiary Grenade"},
            {"weapon_m249", "M249"},
            {"weapon_m4a1_silencer", "M4A1-S"},
            {"weapon_mac10", "MAC-10"},
            {"weapon_mag7", "MAG-7"},
            {"weapon_molotov", "Molotov"},
            {"weapon_mp5sd", "MP5-SD"},
            {"weapon_mp7", "MP7"},
            {"weapon_mp9", "MP9"},
            {"weapon_negev", "Negev"},
            {"weapon_nova", "Nova"},
            {"weapon_p90", "P90"},
            {"weapon_sawedoff", "Sawed-Off"},
            {"weapon_scar20", "SCAR-20"},
            {"weapon_sg556", "SG 553"},
            {"weapon_smokegrenade", "Smoke Grenade"},
            {"weapon_ssg08", "SSG 08"},
            {"weapon_tagrenade", "TA Grenade"},
            {"weapon_taser", "Taser"},
            {"weapon_ump45", "UMP-45"},
            {"weapon_xm1014", "XM1014"},
            {"weapon_aug", "AUG"},
            {"weapon_bizon", "PP-Bizon"},
            {"weapon_decoy", "Decoy Grenade"},
            {"weapon_fiveseven", "Five-Seven"},
            {"weapon_hkp2000", "P2000"},
            {"weapon_usp_silencer", "USP-S"},
            {"weapon_p250", "P250"},
            {"weapon_tec9", "Tec-9"},
            {"weapon_cz75a", "CZ75-Auto"},
            {"weapon_deagle", "Desert Eagle"},
            {"weapon_revolver", "R8 Revolver"},
            {"weapon_glock", "Glock-18"}
        };

        
        private static string GetWeaponType(string itemIdentifier)
        {
            return WeaponsType.TryGetValue(itemIdentifier, out var value) ? value : "Unknown Weapon Type";
        }

        private static string GetProjectileType(string itemIdentifier)
        {
            return ProjectilesType.TryGetValue(itemIdentifier, out var value) ? value : "Unknown Projectile Type";
        }

        private static string GetEntityType(string itemIdentifier)
        {
            return EntityType.TryGetValue(itemIdentifier, out var value) ? value : "Unknown Entity Type";
        }

        public static void EntityESP()
        {
            float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            for (int i = 64; i < 1024; i++)
            {
                ulong ItemyListEntry = GameState.swed.ReadULong((nint)((ulong)GameState.EntityList + 8UL * ((ulong)(i & 0x7FFF) >> 9) + 16));
                if (ItemyListEntry == 0) continue;

                ulong Item = GameState.swed.ReadULong((nint)((ulong)ItemyListEntry + 112UL * (ulong)(i & 0x1FF)));
                if (Item == 0) continue;

                ulong ItemNode = GameState.swed.ReadULong((nint)Item + Offsets.m_pGameSceneNode);
                Vector3 ItemOrigin = GameState.swed.ReadVec((nint)ItemNode + Offsets.m_vecAbsOrigin);
                Vector2 ItemPosition2D = Calculate.WorldToScreen(ViewMatrix, ItemOrigin, GameState.renderer.screenSize);
            

                ulong ItemInfo = GameState.swed.ReadULong((nint)(Item + 0x10));
                ulong ItemTypePtr = GameState.swed.ReadULong((nint)(ItemInfo + 0x20));

                if (ItemOrigin.X != 0f)
                {
                    byte[] Buffer = GameState.swed.ReadBytes((nint)ItemTypePtr, 128);
                    int len = Array.IndexOf<byte>(Buffer, 0);
                    if (len < 0) len = Buffer.Length;
                    string type = System.Text.Encoding.UTF8.GetString(Buffer, 0, len);

                    string Weapons = GetWeaponType(type);
                    string Projectiles = GetProjectileType(type);
                    string Entity = GetEntityType(type);

                    if (Weapons != "Unknown Weapon Type")
                    {
                        if (DroppedWeaponESP)
                            GameState.renderer.drawList.AddText(ItemPosition2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), Weapons);
                    }

                    if (Projectiles != "Unknown Projectile Type")
                    {
                        if (ProjectileESP)
                            GameState.renderer.drawList.AddText(ItemPosition2D, ImGui.ColorConvertFloat4ToU32(ProjectileTextColor), Projectiles);
                    }

                    if (Entity != "Unknown Entity Type")
                    {
                        if (ChickenESP && type.Contains("chicken"))
                        {
                            Vector3 chickenHeight = ItemOrigin + new Vector3(0f, 0f, 20f);
                            Vector2 chickenHeight2D = Calculate.WorldToScreen(ViewMatrix, chickenHeight, GameState.renderer.screenSize);

                            float boxHeight = MathF.Abs(chickenHeight2D.Y - ItemPosition2D.Y);
                            float boxWidth = boxHeight * 1.6f;
                            Vector2 topLeft = new(ItemPosition2D.X - boxWidth / 2f, chickenHeight2D.Y);
                            Vector2 bottomRight = new(ItemPosition2D.X + boxWidth / 2f, ItemPosition2D.Y);
                            Vector2 topRight = new(ItemPosition2D.X + boxWidth / 2 + 12.0f, chickenHeight2D.Y);
                            GameState.renderer.drawList.AddRect(topLeft, bottomRight, ImGui.ColorConvertFloat4ToU32(ChickenBoxColor));
                            GameState.renderer.drawList.AddText(ItemPosition2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), "Chicken");
                        }

                        if (HostageESP && type.Contains("hostage_entity"))
                        {
                            Vector3 hostagesHeight = ItemOrigin + new Vector3(0f, 0f, 72f);
                            Vector2 HostagesHeight2D = Calculate.WorldToScreen(ViewMatrix, hostagesHeight, GameState.renderer.screenSize);
                            
                            float boxHeight = MathF.Abs(HostagesHeight2D.Y - ItemPosition2D.Y);
                            float boxWidth = boxHeight * 0.6f;
                            Vector2 topLeft = new(ItemPosition2D.X - boxWidth / 2f, HostagesHeight2D.Y);
                            Vector2 topRight = new(ItemPosition2D.X + boxWidth / 2f + 12f, HostagesHeight2D.Y);
                            Vector2 bottomRight = new(ItemPosition2D.X + boxWidth / 2f, ItemPosition2D.Y);

                            GameState.renderer.drawList.AddRect(topLeft, bottomRight, ImGui.ColorConvertFloat4ToU32(HostageBoxColor));
                            GameState.renderer.drawList.AddText(ItemPosition2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), "Hostage");
                        }
                    }
                }
            }
        }
    }
}
