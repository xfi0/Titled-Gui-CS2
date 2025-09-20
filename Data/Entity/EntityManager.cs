using Swed64;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Titled_Gui.Classes;
using static Titled_Gui.Data.Game.GameState;
using static Titled_Gui.Data.Game.Offsets;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using System.Diagnostics;
using Titled_Gui.Data.Game;
using Vortice.Mathematics;

namespace Titled_Gui.Data.Entity
{
    public class EntityManager()
    {
        public static IntPtr listEntry = IntPtr.Zero;

        public List<Entity>? GetEntities()
        {
            List<Entity> entities = new List<Entity>();
            GameState.EntityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
            listEntry = GameState.swed.ReadPointer(GameState.EntityList + 0x10);

            for (int i = 0; i < 64; i++) // loop through all entities
            {
                currentController = GameState.swed.ReadPointer(listEntry, i * 0x78);
                if (currentController == IntPtr.Zero) continue;

                int pawnHandle = GameState.swed.ReadInt(currentController, Offsets.m_hPlayerPawn);
                if (pawnHandle == 0) continue;

                IntPtr listEntry2 = GameState.swed.ReadPointer(GameState.EntityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);
                if (listEntry2 == IntPtr.Zero) continue;

                GameState.currentPawn = GameState.swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));
                if (GameState.currentPawn == IntPtr.Zero) continue;

                int lifeState = GameState.swed.ReadInt(GameState.currentPawn, Offsets.m_lifeState);
                if (lifeState != 256) continue;

                Entity? entity = PopulateEntity(GameState.currentPawn);

                if (entity != null)
                {
                    entities?.Add(entity);
                }
            }

            return  entities != null ? entities?.OrderBy(e => e?.Distance)?.ToList() : null;
        }
        public Entity GetLocalPlayer()
        {
            IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
            GameState.LocalPlayerPawn = localPlayerPawn;

            float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
            IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
            IntPtr dwSensitivity = GameState.swed.ReadPointer(GameState.client + Offsets.dwSensitivity);
            float sensitivity = GameState.swed.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);
            IntPtr currentWeapon = GameState.swed.ReadPointer(localPlayerPawn, Offsets.m_pClippingWeapon);
            short weaponIndex = GameState.swed.ReadShort(currentWeapon + Offsets.m_AttributeManager, Offsets.m_Item + Offsets.m_iItemDefinitionIndex);

            Entity localPlayer = new Entity
            {
                PawnAddress = localPlayerPawn,
                Origin = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                View = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset),
                AimPunchAngle = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchAngle),
                AimPunchAngleVel = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchCache),
                Position = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                IsFlashed = GameState.swed.ReadFloat(localPlayerPawn, Offsets.m_flFlashBangTime) > 1.5,
                Ping = GameState.swed.ReadInt(localPlayerPawn, Offsets.m_iPing),
                Health = GameState.swed.ReadInt(GameState.localPlayer.PawnAddress, Offsets.m_iHealth),
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
                CurrentWeapon = currentWeapon,
                WeaponIndex = weaponIndex,
                Armor = GameState.swed.ReadInt(localPlayerPawn, Offsets.m_ArmorValue),
                IsScoped = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsScoped),
                IsBuyMenuOpen = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsBuyMenuOpen),
                CurrentWeaponName = Enum.GetName(typeof(GetGunName.WeaponIds), weaponIndex),
                Account = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                CashSpent = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                CashSpentTotal = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                IsShooting = IsShooting(),
                ShotsFired = swed.ReadInt(localPlayerPawn, Offsets.m_iShotsFired),
                IsAttacking = GameState.swed.ReadBool(GameState.client, Offsets.attack),
                Ammo = GameState.swed.ReadInt(client, Offsets.m_iAmmo),
                EyeDirection = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_angEyeAngles),
                IsWalking = GameState.swed.ReadBool(localPlayerPawn, Offsets.m_bIsWalking),
            };

            return localPlayer;
        }

        //public static bool Visible()
        //{
        //    if (localPlayer == null) return false;

        //    Ray ray = new(localPlayer.Bones[2], localPlayer.EyeDirection);
        //    if (ray.Intersects(localPlayer.Bones[2],))
        //}
        private static readonly Dictionary<nint, int> Shots = new();

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
        private Entity? PopulateEntity(IntPtr pawnAddress)
        { 
            try
            {
                float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
                IntPtr dwSensitivity = GameState.swed.ReadPointer(GameState.client + Offsets.dwSensitivity);
                float sensitivity = GameState.swed.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);
                IntPtr currentWeapon = GameState.swed.ReadPointer(pawnAddress, Offsets.m_pClippingWeapon);
                short weaponIndex = GameState.swed.ReadShort(currentWeapon + Offsets.m_AttributeManager, Offsets.m_Item + Offsets.m_iItemDefinitionIndex);

                Entity entity = new Entity
                {
                    Team = GameState.swed.ReadInt(pawnAddress + Offsets.m_iTeamNum),
                    PawnAddress = pawnAddress,
                    Health = (int)GameState.swed.ReadUInt(pawnAddress, Offsets.m_iHealth),
                    LifeState = GameState.swed.ReadInt(pawnAddress, Offsets.m_lifeState),
                    Position = GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin),
                    View = GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset),
                    Position2D = Calculate.WorldToScreen(ViewMatrix, GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), renderer.screenSize),
                    ViewPosition2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                    //Visible => ,
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
                    CurrentWeapon = currentWeapon,
                    WeaponIndex = weaponIndex,
                    Armor = GameState.swed.ReadInt(pawnAddress, Offsets.m_ArmorValue),
                    IsScoped = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsScoped),
                    IsBuyMenuOpen = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsBuyMenuOpen),
                    CurrentWeaponName = Enum.GetName(typeof(GetGunName.WeaponIds), weaponIndex),
                    Account = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                    CashSpent = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                    CashSpentTotal = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                    IsShooting = IsShooting(),
                    ShotsFired = swed.ReadInt(pawnAddress, Offsets.m_iShotsFired),
                    IsAttacking = GameState.swed.ReadBool(GameState.client, Offsets.attack),
                    IsFlashed = GameState.swed.ReadFloat(pawnAddress, Offsets.m_flFlashBangTime) > 1.5,
                    Ammo = GameState.swed.ReadInt(client, Offsets.m_iAmmo),
                    EyeDirection = GameState.swed.ReadVec(pawnAddress, Offsets.m_angEyeAngles),
                    Ping = (int)GameState.swed.ReadUInt(currentController, Offsets.m_iPing),
                    IsWalking = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsWalking),
                };

                entity.IsEnemy = entity.Team != GameState.localPlayer.Team;

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
