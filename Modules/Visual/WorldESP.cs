using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static ValveResourceFormat.ResourceTypes.EntityLump;

namespace Titled_Gui.Modules.Visual
{
    internal class WorldESP
    {
        #region Bools/Toggles
        public static bool ChickenESP = false;
        public static bool DroppedWeaponESP = false;
        public static bool ProjectileESP = false;
        public static bool HostageESP = false;
        public static bool DrawBoxes = true;
        public static bool DrawText = true;
        #endregion

        #region Colors
        public static Vector4 WeaponTextColor = new(1, 1, 1, 1);
        public static Vector4 ProjectileTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenTextColor = new(1, 1, 1, 1);
        public static Vector4 HostageTextColor = new(1, 1, 1, 1);
        public static Vector4 BoxColor = new(1, 1, 1, 1);
        #endregion Colors

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
            float thickness = 2f;

            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);

            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }
            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.drawList.AddText(worldEntity.Position2D, ImGui.ColorConvertFloat4ToU32(HostageTextColor), "Hostage");
        }

        private static void DrawProjectileESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            float thickness = 2f;

            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);

            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.drawList.AddText(worldEntity.Position2D,
                    ImGui.ColorConvertFloat4ToU32(ProjectileTextColor), worldEntity.DisplayName);
        }

        private static void DrawWeaponESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            float thickness = 2f;

            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);

            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);
            if (DrawText)
                GameState.renderer.drawList.AddText(worldEntity.Position2D,
                    ImGui.ColorConvertFloat4ToU32(WeaponTextColor), worldEntity.DisplayName);
        }

        private static void DrawChickenESP(WorldEntity? worldEntity)
        {
            if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            float thickness = 2f;

            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);
            var corners2D = new Vector2[8];

            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = Calculate.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.drawList.AddText(worldEntity.Position2D,
                    ImGui.ColorConvertFloat4ToU32(ChickenTextColor),
                    "Chicken");
        }

        public static void Draw3DBoxESP(Vector2[] corners2D, uint preConvertedColor, bool filled, float rounding, uint preConvertedFilledColor = 0)
        {
            try
            {
                if (filled)
                {
                    // bottom face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2], preConvertedFilledColor);
                    // top face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[4], corners2D[5], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // front face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[5], corners2D[4], preConvertedFilledColor);
                    // back face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[2], corners2D[3], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // left face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[2], corners2D[6], corners2D[4], preConvertedFilledColor);
                    // right face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[1], corners2D[3], corners2D[7], corners2D[5], preConvertedFilledColor);
                }
                GameState.renderer.drawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[1], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[3], corners2D[0], preConvertedColor, rounding);

                GameState.renderer.drawList.AddLine(corners2D[4], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[5], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[7], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[7], corners2D[4], preConvertedColor, rounding);

                GameState.renderer.drawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[1], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[2], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[3], corners2D[7], preConvertedColor, rounding);
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Index Out Of Bounds Of The Array Drawing 3D Boxes");
            }
        }  
        public static void Draw3DBoxESPFromMatrix(Vector2[] corners2D, uint preConvertedColor, bool filled, float rounding, uint preConvertedFilledColor = 0)
        {
            try
            {
                if (filled)
                {
                    // bottom face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2], preConvertedFilledColor);
                    // top face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[4], corners2D[5], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // front face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[5], corners2D[4], preConvertedFilledColor);
                    // back face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[2], corners2D[3], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // left face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[0], corners2D[2], corners2D[6], corners2D[4], preConvertedFilledColor);
                    // right face
                    GameState.renderer.drawList.AddQuadFilled(corners2D[1], corners2D[3], corners2D[7], corners2D[5], preConvertedFilledColor);
                }
                GameState.renderer.drawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[1], corners2D[3], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[2], corners2D[0], preConvertedColor, rounding);

                GameState.renderer.drawList.AddLine(corners2D[4], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[5], corners2D[7], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[7], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[6], corners2D[4], preConvertedColor, rounding);

                GameState.renderer.drawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[1], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[2], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.drawList.AddLine(corners2D[3], corners2D[7], preConvertedColor, rounding);
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Index Out Of Bounds Of The Array Drawing 3D Boxes");
            }
        }
    }
}
