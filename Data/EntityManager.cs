using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Titled_Gui.ModuleHelpers;
using Titled_Gui.Modules.Visual;

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
        public List<Entity> GetEntities()
        {
            List<Entity> entities = new List<Entity>();
            GameState.EntityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
            IntPtr listEntry = GameState.swed.ReadPointer(GameState.EntityList + 0x10);
            for (int i = 0; i < 64; i++) // loop through all entities
            {
                IntPtr currentController = GameState.swed.ReadPointer(listEntry, i * 0x78);
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
                GameState.WeaponIndex = GameState.swed.ReadShort(entity.HeldWeapon, Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex);
                entities.Add(entity);
            }

            return entities.OrderBy(e => e.distance).ToList();
        }
        //returns the local player with all the variables assigned
        public Entity GetLocalPlayer()
        {
            IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
            GameState.LocalPlayerPawn = localPlayerPawn;

            Entity localPlayer = new Entity
            {
                PawnAddress = localPlayerPawn,
                origin = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                team = GameState.swed.ReadInt(localPlayerPawn + Offsets.m_iTeamNum),
                view = GameState.swed.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset)
            };

            return localPlayer;
        }

        //asign all the entity variables to something
        private Entity PopulateEntity(IntPtr pawnAddress)
        {
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            IntPtr sceneNode = GameState.swed.ReadPointer(GameState.currentPawn, Offsets.m_pGameSceneNode);
            IntPtr boneMatrix = GameState.swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);
            var VisibleTest = GameState.swed.ReadInt(GameState.LocalPlayerPawn + Offsets.m_iIDEntIndex);
            IntPtr dwSensitivity = GameState.swed.ReadPointer(GameState.client + Offsets.dwSensitivity);
            float sensitivity = GameState.swed.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);

            Entity entity = new Entity
            {
                team = GameState.swed.ReadInt(pawnAddress + Offsets.m_iTeamNum),
                PawnAddress = pawnAddress,
                health = (int)GameState.swed.ReadUInt(pawnAddress, Offsets.m_iHealth),
                LifeState = GameState.swed.ReadInt(pawnAddress, Offsets.m_lifeState),
                position = GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin),
                view = GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset),
                position2D = Calculate.WorldToScreen(viewMatrix, GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), renderer.screenSize),
                viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                Visible = VisibleTest != -1,
                head = Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)),
                head2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vecViewOffset)), renderer.screenSize),
                distance = Vector3.Distance(GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vOldOrigin), GameState.swed.ReadVec(pawnAddress, Offsets.m_vOldOrigin)),
                bones = Calculate.ReadBones(boneMatrix, GameState.swed),
                bones2D = Calculate.ReadBones2D(Calculate.ReadBones(boneMatrix, GameState.swed), viewMatrix, renderer.screenSize),
                HeldWeapon = GameState.swed.ReadShort(GameState.currentPawn, Offsets.m_pClippingWeapon),
                dwSensitivity = dwSensitivity,
                WeaponIndex = GameState.WeaponIndex,
                Sensitivity = sensitivity,
                Velocity = GameState.swed.ReadVec(GameState.LocalPlayerPawn, Offsets.m_vecAbsVelocity)
            };

            return entity;
        }

    }
}