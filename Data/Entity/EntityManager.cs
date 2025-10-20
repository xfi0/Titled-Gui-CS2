using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Data.Entity
{
    public class EntityManager()
    {
        public static IntPtr listEntry = IntPtr.Zero;

        public List<Entity>? GetEntities()
        {
            try
            {
                List<Entity> entities = [];
                GameState.EntityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (EntityList != IntPtr.Zero)
                    listEntry = GameState.swed.ReadPointer(GameState.EntityList + 0x10);
                else
                    Console.WriteLine("Entity List Was Null");

                for (int i = 0; i < 64; i++) // loop through all entities
                {
                    currentController = GameState.swed.ReadPointer(listEntry, i * 0x70);
                    if (currentController == IntPtr.Zero) continue;

                    int pawnHandle = GameState.swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
                    if (pawnHandle == 0) continue;

                    IntPtr listEntry2 = GameState.swed.ReadPointer(GameState.EntityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
                    if (listEntry2 == IntPtr.Zero) continue;

                    GameState.currentPawn = GameState.swed.ReadPointer(listEntry2, 0x70 * (pawnHandle & 0x1FF));
                    if (GameState.currentPawn == IntPtr.Zero) continue;

                    int lifeState = GameState.swed.ReadInt(GameState.currentPawn, Offsets.m_lifeState);
                    if (lifeState != 256) continue;

                    Entity? entity = PopulateEntity(GameState.currentPawn);

                    if (entity != null)
                        entities?.Add(entity);

                }
                return entities != null ? entities?.OrderBy(e => e?.Distance)?.ToList() : null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
        public static Entity GetLocalPlayer()
        {
            IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
            GameState.LocalPlayerPawn = localPlayerPawn;

            float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
            IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
            IntPtr dwSensitivity = GameState.swed.ReadPointer(GameState.client + Offsets.dwSensitivity);
            float sensitivity = GameState.swed.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);
            IntPtr clippingWeapon = GameState.swed.ReadPointer(localPlayerPawn + Offsets.m_pClippingWeapon);
            IntPtr weaponData = GameState.swed.ReadPointer(clippingWeapon + 0x10);
            IntPtr weaponNameAddress = GameState.swed.ReadPointer(weaponData + 0x20);

            string weaponName = "Invalid Weapon Name";
            if (weaponNameAddress != 0)
            {
                byte[] Buffer = GameState.swed.ReadBytes(weaponNameAddress, 32);
                int len = Array.IndexOf<byte>(Buffer, 0);
                if (len < 0)
                    len = Buffer.Length;

                string raw = System.Text.Encoding.UTF8.GetString(Buffer, 0, len);

                if (raw.Length > 7)
                    weaponName = raw.Substring(7);
                else
                    weaponName = raw;
            }

            Entity localPlayer = new()
            {
                PawnAddress = localPlayerPawn,
                Origin = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                View = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset),
                AimPunchAngle = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchAngle),
                AimPunchAngleVel = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchCache),
                Position = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                IsFlashed = GameState.swed.ReadFloat(localPlayerPawn, Offsets.m_flFlashBangTime) > 1.5,
                Ping = GameState.swed.ReadInt(localPlayerPawn, Offsets.m_iPing),
                Health = GameState.swed.ReadInt(GameState.LocalPlayer.PawnAddress, Offsets.m_iHealth),
                Team = GameState.swed.ReadInt(localPlayerPawn + Offsets.m_iTeamNum),
                LifeState = GameState.swed.ReadInt(localPlayerPawn, Offsets.m_lifeState),
                Position2D = Calculate.WorldToScreen(ViewMatrix, GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), renderer.screenSize),
                ViewPosition2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset)), renderer.screenSize),
                //Visible => ,
                Head = Vector3.Add(GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset)),
                Head2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset)), renderer.screenSize),
                Distance = Vector3.Distance(GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin)),
                Bones = Calculate.ReadBones(boneMatrix),
                Name = GameState.swed.ReadString(currentController, Offsets.m_iszPlayerName, 32),
                Bones2D = Calculate.ReadBones2D(Calculate.ReadBones(boneMatrix), ViewMatrix, renderer.screenSize),
                dwSensitivity = dwSensitivity,
                Sensitivity = GameState.swed.ReadFloat(GameState.client + Offsets.dwSensitivity, Offsets.dwSensitivity_sensitivity),
                Velocity = GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vecAbsVelocity),
                ViewAngles = GameState.swed.ReadVec(client, Offsets.dwViewAngles),
                Armor = GameState.swed.ReadInt(localPlayerPawn, Offsets.m_ArmorValue),
                IsScoped = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsScoped),
                IsBuyMenuOpen = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsBuyMenuOpen),
                CurrentWeaponName = weaponName,
                Account = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                CashSpent = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                CashSpentTotal = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                IsShooting = IsShooting(),
                ShotsFired = swed.ReadInt(localPlayerPawn, Offsets.m_iShotsFired),
                IsAttacking = GameState.swed.ReadBool(GameState.client, Offsets.attack),
                Ammo = GameState.swed.ReadInt(client, Offsets.m_iAmmo),
                EyeDirection = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_angEyeAngles),
                IsWalking = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsWalking),
                //HasBomb = GameState.swed.ReadBool(localPlayer, Offsets.)
                IsDefusing = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsDefusing),
                InBombZone = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bInBombZone),
            };

            return localPlayer;
        }

        //public static bool Visible()
        //{
        //    if (localPlayer == null) return false;

        //    Ray ray = new(localPlayer.Bones[2], localPlayer.EyeDirection);
        //    if (ray.Intersects(localPlayer.Bones[2],))
        //}
        private static readonly Dictionary<nint, int> Shots = [];

        public static bool IsShooting()
        {
            foreach (Entity entity in GameState.Entities)
            {
                if (!Shots.TryGetValue(entity.PawnAddress, out int oldValue))
                    oldValue = entity.ShotsFired;

                if (entity.ShotsFired > oldValue)
                {
                    Shots[entity.PawnAddress] = entity.ShotsFired;
                    return true; 
                }

                Shots[entity.PawnAddress] = entity.ShotsFired;
            }

            return false;
        }
        //public static bool VisibilityCheck(Entity e)
        //{
           
        //}
        private static Entity? PopulateEntity(IntPtr pawnAddress)
        { 
            try
            {
                float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
                IntPtr dwSensitivity = GameState.swed.ReadPointer(GameState.client + Offsets.dwSensitivity);
                float sensitivity = GameState.swed.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);

                IntPtr clippingWeapon = GameState.swed.ReadPointer(currentPawn + Offsets.m_pClippingWeapon);
                IntPtr weaponData = GameState.swed.ReadPointer(clippingWeapon + 0x10);
                IntPtr weaponNameAddress = GameState.swed.ReadPointer(weaponData + 0x20);

                string weaponName = "Invalid Weapon Name";
                if (weaponNameAddress != 0)
                {
                    byte[] Buffer = GameState.swed.ReadBytes(weaponNameAddress, 32);
                    int len = Array.IndexOf<byte>(Buffer, 0);
                    if (len < 0) 
                        len = Buffer.Length;

                    string raw = System.Text.Encoding.UTF8.GetString(Buffer, 0, len);

                    if (raw.Length > 7)
                        weaponName = raw.Substring(7);
                    else
                        weaponName = raw;
                }


                Entity entity = new()
                {
                    Team = GameState.swed.ReadInt(pawnAddress + Offsets.m_iTeamNum),
                    PawnAddress = pawnAddress,
                    Health = (int)GameState.swed.ReadUInt(pawnAddress, Offsets.m_iHealth),
                    LifeState = GameState.swed.ReadInt(pawnAddress, Offsets.m_lifeState),
                    Position = GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin),
                    View = GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset),
                    Position2D = Calculate.WorldToScreen(ViewMatrix, GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), renderer.screenSize),
                    ViewPosition2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                    Visible = swed.ReadBool(currentPawn, Offsets.m_entitySpottedState + Offsets.m_bSpotted),
                    SpottedByState = swed.ReadPointer(pawnAddress + 0x2718),
                    Head = Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)),
                    Head2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                    Distance = Vector3.Distance(GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin)),
                    Bones = Calculate.ReadBones(boneMatrix),
                    Name = GameState.swed.ReadString(currentController, Offsets.m_iszPlayerName, 32),
                    Bones2D = Calculate.ReadBones2D(Calculate.ReadBones(boneMatrix), ViewMatrix, renderer.screenSize),
                    dwSensitivity = dwSensitivity,
                    Sensitivity = GameState.swed.ReadFloat(GameState.client + Offsets.dwSensitivity, Offsets.dwSensitivity_sensitivity),
                    Velocity = GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vecAbsVelocity),
                    ViewAngles = GameState.swed.ReadVec(client, Offsets.dwViewAngles),
                    AimPunchAngle = GameState.swed.ReadVec(pawnAddress + Offsets.m_aimPunchAngle),
                    AimPunchAngleVel = GameState.swed.ReadVec(pawnAddress + Offsets.m_aimPunchCache),
                    //CurrentWeapon = currentWeapon,
                    //WeaponIndex = weaponIndex,
                    Armor = GameState.swed.ReadInt(pawnAddress, Offsets.m_ArmorValue),
                    IsScoped = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsScoped),
                    IsBuyMenuOpen = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsBuyMenuOpen),
                    CurrentWeaponName = weaponName,
                    Account = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                    CashSpent = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                    CashSpentTotal = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                    IsShooting = IsShooting(),
                    ShotsFired = swed.ReadInt(pawnAddress, Offsets.m_iShotsFired),
                    IsAttacking = GameState.swed.ReadBool(GameState.client, Offsets.attack),
                    IsFlashed = GameState.swed.ReadFloat(pawnAddress, Offsets.m_flFlashBangTime) > 1.5,
                    Ammo = GameState.swed.ReadInt(pawnAddress, Offsets.m_iAmmo),
                    EyeDirection = GameState.swed.ReadVec(pawnAddress, Offsets.m_angEyeAngles),
                    Ping = (int)GameState.swed.ReadUInt(currentController, Offsets.m_iPing),
                    IsWalking = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsWalking),
                    AngEyeAngles = GameState.swed.ReadVec(pawnAddress, Offsets.m_angEyeAngles),
                };

                //Console.WriteLine(entity.Visible + " " + entity.Name);
                entity.IsEnemy = entity.Team != GameState.LocalPlayer.Team;

                return entity;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }
        }
    }
}
