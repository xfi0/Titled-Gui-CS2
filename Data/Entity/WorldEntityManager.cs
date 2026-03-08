using System.Numerics;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Data.Entity
{
    public class WorldEntityManager : ThreadService
    {
        private static IntPtr listEntry = IntPtr.Zero;
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
            {"weapon_glock", "Glock-18"},
            {"weapon_c4", "C4"}
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
                for (int i = 65; i < 1024; i++)
                {
                    listEntry = GameState.swed.ReadPointer(GameState.EntityList + 0x8 * ((i & 0x7FFF) >> 9) + 16);
                    if (listEntry == 0) continue;

                    var pawnAddress = GameState.swed.ReadPointer(listEntry + 0x70 * (i & 0x1FF));
                    if (pawnAddress == 0) continue;

                    IntPtr itemNode = GameState.swed.ReadPointer(pawnAddress + Offsets.m_pGameSceneNode);
                    if (itemNode == 0) continue;

                    IntPtr itemInfo = GameState.swed.ReadPointer((pawnAddress + 0x10));
                    if (itemInfo == 0) continue;

                    IntPtr itemTypePtr = GameState.swed.ReadPointer((itemInfo + 0x20));
                    if (itemTypePtr == 0) continue;

                    byte[] buffer = GameState.swed.ReadBytes(itemTypePtr, 128);
                    int len = Array.IndexOf<byte>(buffer, 0);
                    if (len < 0) len = buffer.Length;

                    string type =
                        Encoding.UTF8.GetString(buffer, 0, len >= 0 ? len : buffer.Length).Trim().Split('\0')[0]
                            .Replace("?", "").Replace("\0", "");
                    if (string.IsNullOrWhiteSpace(type))
                        continue;

                    WorldEntity? worldEntity = PopulateEntity(pawnAddress, type, itemNode);

                    if (worldEntity == null || worldEntity.Position2D == new Vector2(-99, -99) ||
                        worldEntity.Position.X == 0 || worldEntity.Position.Y == 0 || worldEntity.PawnAddress == 0x0)
                        continue;

                    worldEntities.Add(worldEntity);
                }

                return worldEntities != null ? worldEntities : new List<WorldEntity?>();
            }
            catch (Exception ex)
            {
                Console.WriteLine("World Entity Loop Exception: " + ex);
            }

            return new List<WorldEntity?>();
        }

        public WorldEntity? PopulateEntity(nint pawnAddress, string type, IntPtr itemNode)
        {
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

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
            else if (type.Contains("chicken"))
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

        protected override void FrameAction()
        {
            List<WorldEntity?> newSnapshot = GetWorldEntities();

            lock (_lastSnapshot)
            {
                _lastSnapshot.Clear();
                _lastSnapshot.AddRange(newSnapshot);
                GameState.worldEntities = _lastSnapshot.ToList();
            }

            Thread.SpinWait(20);
        }
    }
}
