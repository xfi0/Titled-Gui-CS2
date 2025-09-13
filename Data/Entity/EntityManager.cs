using Swed64;
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Numerics;
using Titled_Gui.Classes;
using static Titled_Gui.Data.GameState;
using Titled_Gui.Modules.Rage;
using Titled_Gui.Modules.Visual;
using System.Diagnostics;

namespace Titled_Gui.Data
{
    public class EntityManager
    {
        private readonly Renderer renderer;

        public EntityManager(Swed swed, Renderer renderer)
        {
            this.renderer = renderer;
        }
        //get all entities
        public static IntPtr listEntry = IntPtr.Zero;
        public List<Entity> GetEntities()
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

                Entity entity = PopulateEntity(GameState.currentPawn);
                entities.Add(entity);
            }

            return entities.OrderBy(e => e.Distance).ToList();
        }
        //returns the local player with all the variables assigned
        public Entity GetLocalPlayer()
        {
            IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
            GameState.LocalPlayerPawn = localPlayerPawn;
            int Health = GameState.swed.ReadInt(GameState.localPlayer.PawnAddress, Offsets.m_iHealth);

            Entity localPlayer = new Entity
            {
                PawnAddress = localPlayerPawn,
                Origin = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                Team = GameState.swed.ReadInt(localPlayerPawn + Offsets.m_iTeamNum),
                View = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset),
                AimPunchAngle = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchAngle),
                AimPunchAngleVel = GameState.swed.ReadVec(localPlayerPawn + Offsets.m_aimPunchCache),
                Health = Health
            };

            return localPlayer;
        }
        public static Entity? ReturnLocalPlayer()
        {
            foreach (var e in GameState.Entities)
            {
                if (e != null)
                {
                    if (e.PawnAddress == GameState.localPlayer.PawnAddress) return e;
                }
                else
                {
                    return null;
                }
            }
            return null;
        }
        //asign all the entity variables to something
        private Entity? PopulateEntity(IntPtr pawnAddress)
        {
            try
            {
                float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
                var VisibleTest = GameState.swed.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);
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
                    Visible = VisibleTest != -1,
                    Head = Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)),
                    Head2D = Calculate.WorldToScreen(ViewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                    Distance = Vector3.Distance(GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin)),
                    Bones = Calculate.ReadBones(boneMatrix, GameState.swed),
                    Name = GameState.swed.ReadString(currentController, Offsets.m_iszPlayerName, 32),
                    Bones2D = Calculate.ReadBones2D(Calculate.ReadBones(boneMatrix, GameState.swed), ViewMatrix, renderer.screenSize),
                    dwSensitivity = dwSensitivity,
                    Sensitivity = GameState.swed.ReadFloat(GameState.client + Offsets.dwSensitivity, Offsets.dwSensitivity_sensitivity),
                    Velocity = GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vecAbsVelocity),
                    ViewAngles = GameState.swed.ReadVec(Offsets.dwViewAngles),
                    AimPunchAngle = GameState.swed.ReadVec(pawnAddress + Offsets.m_aimPunchAngle),
                    AimPunchAngleVel = GameState.swed.ReadVec(pawnAddress + Offsets.m_aimPunchCache),
                    CurrentWeapon = currentWeapon,
                    WeaponIndex = weaponIndex,
                    Armor = GameState.swed.ReadInt(pawnAddress, Offsets.m_ArmorValue),
                    IsScoped = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsScoped),
                    IsBuyMenuOpen = GameState.swed.ReadBool(pawnAddress, Offsets.m_bIsScoped),
                    //Ammo = GameState.swed.ReadInt(pawnAddress, Offsets.)
                    CurrentWeaponName = Enum.GetName(typeof(GetGunName.WeaponIds), weaponIndex),
                    Account = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                    CashSpent = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                    CashSpentTotal = GameState.swed.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                    IsShooting = GameState.swed.ReadBool(GameState.client, Offsets.attack)
                };
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
