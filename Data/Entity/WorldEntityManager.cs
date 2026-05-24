using System.Diagnostics;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Data.Entity
{
    public class WorldEntityManager : ThreadService
    {
        private static readonly Dictionary<string, string> EntityType = new() {
    {"C_Chicken", "Chicken"},
    {"C_Hostage", "Hostage"}
};

        private static readonly Dictionary<string, string> ProjectilesType = new() {
    {"C_SmokeGrenadeProjectile", "Smoke Grenade"},
    {"C_FlashbangProjectile", "Flashbang"},
    {"C_HEGrenadeProjectile", "HE Grenade"},
    {"C_MolotovProjectile", "Molotov"},
    {"C_Inferno", "Molotov Fire"},
    {"C_IncendiaryGrenadeProjectile", "Incendiary Grenade"},
    {"C_DecoyProjectile", "Decoy Grenade"}
};

        private static readonly Dictionary<string, string> WeaponsType = new() {
    {"C_AK47", "AK-47"},
    {"C_WeaponM4A1", "M4A1"},
    {"C_WeaponAWP", "AWP"},
    {"C_WeaponElite", "Elite"},
    {"C_WeaponFamas", "Famas"},
    {"C_Flashbang", "Flashbang"},
    {"C_WeaponG3SG1", "G3SG1"},
    {"C_WeaponGalilAR", "Galil AR"},
    {"C_Item_Healthshot", "Health Shot"},
    {"C_HEGrenade", "HE Grenade"},
    {"C_IncendiaryGrenade", "Incendiary Grenade"},
    {"C_WeaponM249", "M249"},
    {"C_WeaponM4A1Silencer", "M4A1-S"},
    {"C_WeaponMAC10", "MAC-10"},
    {"C_WeaponMag7", "MAG-7"},
    {"C_MolotovGrenade", "Molotov"},
    {"C_WeaponMP5SD", "MP5-SD"},
    {"C_WeaponMP7", "MP7"},
    {"C_WeaponMP9", "MP9"},
    {"C_WeaponNegev", "Negev"},
    {"C_WeaponNOVA", "Nova"},
    {"C_WeaponP90", "P90"},
    {"C_WeaponSawedoff", "Sawed-Off"},
    {"C_WeaponSCAR20", "SCAR-20"},
    {"C_WeaponSG556", "SG 553"},
    {"C_SmokeGrenade", "Smoke Grenade"},
    {"C_WeaponSSG08", "SSG 08"},
    {"C_WeaponTaser", "Taser"},
    {"C_WeaponUMP45", "UMP-45"},
    {"C_WeaponXM1014", "XM1014"},
    {"C_WeaponAug", "AUG"},
    {"C_WeaponBizon", "PP-Bizon"},
    {"C_DecoyGrenade", "Decoy Grenade"},
    {"C_WeaponFiveSeven", "Five-Seven"},
    {"C_WeaponHKP2000", "P2000"},
    {"C_WeaponUSPSilencer", "USP-S"},
    {"C_WeaponP250", "P250"},
    {"C_WeaponTec9", "Tec-9"},
    {"C_WeaponCZ75a", "CZ75-Auto"},
    {"C_DEagle", "Desert Eagle"},
    {"C_WeaponRevolver", "R8 Revolver"},
    {"C_WeaponGlock", "Glock-18"},
    {"C_C4", "C4"},
    {"C_Knife", "Knife"}
};
        public static readonly Dictionary<int, string> WeaponNameMap = new(){
            { 1, "deagle"},
            { 2, "elite"},
            { 3, "fiveseven"},
            { 4, "glock"},
            { 7, "ak47"},
            { 8, "aug"},
            { 9, "awp"},
            { 10, "famas"},
            { 11, "g3sg1"},
            { 13, "galilar"},
            { 14, "m249"},
            { 16, "m4a1"},
            { 17, "mac10"},
            { 19, "p90"},
            { 24, "ump"},
            { 25, "xm1014"},
            { 26, "bizon"},
            { 27, "mag7"},
            { 28, "negev"},
            { 29, "sawedoff"},
            { 30, "tec9"},
            { 31, "taser"},
            { 32, "hkp2000"},
            { 33, "mp7"},
            { 34, "mp9"},
            { 35, "nova"},
            { 36, "p250"},
            { 38, "scar20"},
            { 39, "sg556"},
            { 40, "ssg08"},
            { 42, "knife"},
            { 43, "flashbang"},
            { 44, "hegrenade"},
            { 45, "smokegrenade"},
            { 46, "molotov"},
            { 47, "decoy"},
            { 48, "incgrenade"},
            { 49, "c4"},
            { 59, "knife"}, // _t
            { 60, "m4a1_silencer"},
            { 61, "usp_silencer"},
            { 63, "cz75a"},
            { 64, "revolver"},
            { 500, "knife"}, // bayonet
            { 505, "knife"}, // _flip
            { 506, "knife"}, // _gut
            { 507, "knife"}, // _karambit
            { 508, "knife"}, // _m9_bayonet
            { 509, "knife"}, // _tactical
            { 512, "knife"}, // _falchion
            { 514, "knife"}, // _survival_bowie
            { 515, "knife"}, // _butterfly
            { 516, "knife"}, // _push
            { 526, "knife"} // _kukri
        };

        public enum EntityKind 
        { 
            Unknown = 0,
            Weapon, 
            Projectile, 
            Chicken, 
            Hostage 
        }


        public List<WorldEntity?> GetWorldEntities()
        {
            try
            {
                List<WorldEntity?> worldEntities = new List<WorldEntity?>();
                float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

                for (int i = 65; i < 1024; i++)
                {
                    IntPtr listEntry = GameState.swed.ReadPointer(GameState.EntityList + 0x8 * ((i & 0x7FFF) >> 9) + 16);
                    if (listEntry == 0) continue;

                    var pawnAddress = GameState.swed.ReadPointer(listEntry + 0x70 * (i & 0x1FF));
                    if (pawnAddress == 0) continue;

                    IntPtr itemNode = GameState.swed.ReadPointer(pawnAddress + Offsets.m_pGameSceneNode);
                    if (itemNode == 0) continue;

                    IntPtr itemInfo = GameState.swed.ReadPointer(pawnAddress + 0x10);
                    if (itemInfo == 0) continue;

                    IntPtr classInfo = GameState.swed.ReadPointer(itemInfo + 0x8);
                    if (classInfo == 0) continue;

                    IntPtr nameContainer = GameState.swed.ReadPointer(classInfo + 0x8);
                    if (nameContainer == 0) continue;

                    IntPtr namePtr = GameState.swed.ReadPointer(nameContainer + 0x8);
                    if (namePtr == 0) continue;

                    byte[] buffer = GameState.swed.ReadBytes(namePtr, 64);

                    int len = Array.IndexOf<byte>(buffer, 0);
                    if (len < 0) len = buffer.Length;

                    string type = Encoding.UTF8.GetString(buffer, 0, len >= 0 ? len : buffer.Length).Trim().Split('\0')[0].Replace("?", "").Replace("\0", "");
                    if (string.IsNullOrWhiteSpace(type))
                        continue;

                    WorldEntity? worldEntity = PopulateEntity(pawnAddress, type, itemNode, viewMatrix);

                    if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99) ||
                        worldEntity.Position.X == 0 || worldEntity.Position.Y == 0 || worldEntity.PawnAddress == 0x0)
                        continue;

                    worldEntities.Add(worldEntity);
                }

                return worldEntities;
            }
            catch (Exception ex)
            {
                Console.WriteLine("World Entity Loop Exception: " + ex);
            }

            return new List<WorldEntity?>();
        }

        public WorldEntity? PopulateEntity(nint pawnAddress, string type, IntPtr itemNode, float[] viewMatrix)
        {
            Vector3 itemOrigin = GameState.swed.ReadVec((nint)itemNode + Offsets.m_vecOrigin);
            IntPtr collisionBase = pawnAddress + Offsets.m_Collision;

            WorldEntity newWorldEntity = new()
            {
                PawnAddress = pawnAddress,
                ItemNode = itemNode,
                Position = itemOrigin,
                Position2D = Calculate.WorldToScreen(viewMatrix, itemOrigin),
                DisplayName = "",
                Type = EntityKind.Unknown,
                RawType = type,
                VecMax = GameState.swed.ReadVec(collisionBase, Offsets.m_vecMaxs),
                VecMin = GameState.swed.ReadVec(collisionBase, Offsets.m_vecMins),
                Matrix = GameState.swed.ReadMatrix(itemNode + Offsets.m_nodeToWorld),
                Rotation = GameState.swed.ReadMatrix(itemNode + Offsets.m_angRotation)
            };

            if (WeaponsType.TryGetValue(type, out var weaponName))
            {
                newWorldEntity.Type = EntityKind.Weapon;
                newWorldEntity.DisplayName = weaponName;
            }
            else if (ProjectilesType.TryGetValue(type, out var projectileName))
            {
                newWorldEntity.Type = EntityKind.Projectile;
                newWorldEntity.DisplayName = projectileName;
            }
            else if (type.Contains("C_Chicken"))
            {
                newWorldEntity.Type = EntityKind.Chicken;
                newWorldEntity.DisplayName = "Chicken";
            }
            else if (type.Contains("hostage_entity"))
            {
                newWorldEntity.Type = EntityKind.Hostage;
                newWorldEntity.DisplayName = "Hostage";
            }

            return newWorldEntity;
        }

        private readonly List<WorldEntity?> _lastSnapshot = new();
        private static readonly Stopwatch FrameTimer = Stopwatch.StartNew();
        private const double TargetFrameMs = 0.5;
        protected override void FrameAction()
        {
            if (FrameTimer.Elapsed.TotalMilliseconds < TargetFrameMs)
                return;

            FrameTimer.Restart();
            List<WorldEntity?> newSnapshot = GetWorldEntities();

            lock (_lastSnapshot)
            {
                _lastSnapshot.Clear();
                _lastSnapshot.AddRange(newSnapshot);
                GameState.worldEntities = _lastSnapshot.ToList();
            }

        }
    }
}
