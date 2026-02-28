using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class WorldESP // pasted asf
    {
        public static bool ChickenESP = false;
        public static bool DroppedWeaponESP = false;
        public static bool ProjectileESP = false;
        public static bool HostageESP = false;
        public static Vector4 WeaponTextColor = new(1, 1, 1, 1);
        public static Vector4 ProjectileTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenBoxColor = new(1, 1, 1, 1);
        public static Vector4 HostageTextColor = new(1, 1, 1, 1);
        public static Vector4 HostageBoxColor = new(1, 1, 1, 1);

        private static readonly Dictionary<string, string> EntityType = new() {
            {"chicken", "Chicken"},
            {"hostage_entity", "Hostage"}
        };

        private static readonly Dictionary<string, string> ProjectilesType = new() {
            {"smokegrenade_projectile", "Smoke Grenade"},
            {"flashbang_projectile", "Flashbang"},
            {"hegrenade_projectile", "HE Grenade"},
            {"molotov_projectile", "Molotov"},
            {"incendiarygrenade_projectile", "Incendiary Grenade"},
            {"decoy_projectile", "Decoy Grenade"}
        };

        private static readonly Dictionary<string, string> WeaponsType = new(){
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
            if (!ChickenESP && !DroppedWeaponESP && !ProjectileESP && !HostageESP) return;

            foreach (WorldEntity? worldEntity in GameState.worldEntities)
            {
                if (worldEntity == null)
                    continue;

                if (ChickenESP && worldEntity.Type == WorldEntityManager.EntityKind.Chicken)
                    DrawChickenESP(worldEntity);

                if (DroppedWeaponESP && worldEntity.Type == WorldEntityManager.EntityKind.Weapon)
                    DrawWeaponESP(worldEntity);

                if (ProjectileESP && worldEntity.Type == WorldEntityManager.EntityKind.Projectile)
                    DrawProjectileESP(worldEntity);


                if (HostageESP && worldEntity.Type == WorldEntityManager.EntityKind.Hostage)
                    DrawHostageESP(worldEntity);
            }
        }

        private static void DrawHostageESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            Vector3 hostagesHeight = worldEntity.Position + new Vector3(0f, 0f, 72f);
            Vector2 HostagesHeight2D = Calculate.WorldToScreen(viewMatrix, hostagesHeight);

            float boxHeight = MathF.Abs(HostagesHeight2D.Y - worldEntity.Position2D.Y);
            float boxWidth = boxHeight * 0.6f;
            Vector2 topLeft = new(worldEntity.Position2D.X - boxWidth / 2f, HostagesHeight2D.Y);
            Vector2 bottomRight = new(worldEntity.Position2D.X + boxWidth / 2f, worldEntity.Position2D.Y);

            GameState.renderer.drawList.AddRect(topLeft, bottomRight, ImGui.ColorConvertFloat4ToU32(HostageBoxColor));
            GameState.renderer.drawList.AddText(worldEntity.Position2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), "Hostage");
        }

        private static void DrawProjectileESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            GameState.renderer.drawList.AddText(worldEntity.Position2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), worldEntity.DisplayName);
        }

        private static void DrawWeaponESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            GameState.renderer.drawList.AddText(worldEntity.Position2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), worldEntity.DisplayName);
        }
        private static void DrawChickenESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            Vector3 chickenHeight = worldEntity.Position + new Vector3(0f, 0f, 20f);
            Vector2 chickenHeight2D = Calculate.WorldToScreen(viewMatrix, chickenHeight);

            float boxHeight = MathF.Abs(chickenHeight2D.Y - worldEntity.Position2D.Y);
            float boxWidth = boxHeight * 1.6f;
            Vector2 topLeft = new(worldEntity.Position2D.X - boxWidth / 2f, chickenHeight2D.Y);
            Vector2 bottomRight = new(worldEntity.Position2D.X + boxWidth / 2f, worldEntity.Position2D.Y);
            GameState.renderer.drawList.AddRect(topLeft, bottomRight, ImGui.ColorConvertFloat4ToU32(ChickenBoxColor));
            GameState.renderer.drawList.AddText(worldEntity.Position2D, ImGui.ColorConvertFloat4ToU32(WeaponTextColor), "Chicken");
        }
    }
}
