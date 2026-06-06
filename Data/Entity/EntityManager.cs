using System.Diagnostics;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Math;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Data.Entity
{
    public class EntityManager
    {
        public static bool UseOldVisibilityCheck = false;
        public static List<Entity> GetEntities()
        {
            try
            {
                if (memory == null)
                {
                    Console.WriteLine("Memory Instance was null.");
                    return [];
                }
                float[] viewMatrix = memory.ReadMatrix(client + Offsets.dwViewMatrix);

                List<Entity> entities = [];
                EntityList = memory.ReadPointer(client + Offsets.dwEntityList);
                IntPtr listEntry = IntPtr.Zero;

                if (EntityList != IntPtr.Zero)
                    listEntry = memory.ReadPointer(EntityList + 0x10);
                else
                    Console.WriteLine("Entity List Was Null");

                if (listEntry == IntPtr.Zero)
                    return [];

                IntPtr localPlayerPawnAddress = memory.ReadPointer(client + Offsets.dwLocalPlayerPawn);

                for (int i = 0; i < 64; i++) // loop through all entities
                {
                    var controller = memory.ReadPointer(listEntry + 0x70 * (i & 0x1FF));
                    if (controller == IntPtr.Zero)
                        continue;

                    int pawnHandle = memory.ReadInt(controller, Offsets.m_hPlayerPawn);
                    if (pawnHandle <= 0)
                        continue;

                    int pawn = memory.ReadInt(controller, Offsets.m_hPawn);
                    if (pawn == 0)
                        continue;

                    var currentPawn = GetPlayerPawn(EntityList, pawnHandle);
                    if (currentPawn == IntPtr.Zero)
                        continue;
                    

                    if (currentPawn == localPlayerPawnAddress)
                    {
                        Entity localPlayer = EntityManager.GetLocalPlayer(controller);

                        LocalPlayer = localPlayer;

                        renderer.UpdateLocalPlayer(localPlayer);
                    }

                    int lifeState = memory.ReadInt(currentPawn, Offsets.m_lifeState);
                    if (lifeState != 256)
                        continue;

                    Entity? entity = PopulateEntity(currentPawn, viewMatrix, pawn, pawnHandle, controller, EntityList);

                    if (entity != null && entity.Position.X != 0 && entity.Position.Y != 0)
                        entities?.Add(entity);

                }
                return [.. entities.OrderBy(e => e?.Distance)];
            }
            catch (Exception ex)
            {
                Console.WriteLine("Entity List Exception: " + ex.ToString());
                return [];
            }
        }

        public static IntPtr GetPlayerPawn(IntPtr entitylist, IntPtr pawn)
        {
            IntPtr listEntry2 = memory.ReadPointer(entitylist + (0x8 * ((pawn & 0x7FFF) >> 9)) + 0x10);
            if (listEntry2 == IntPtr.Zero)
                return IntPtr.Zero;
            

            IntPtr pPawn = memory.ReadPointer(listEntry2 + 0x70 * (pawn & 0x1FF));
            if (pPawn == IntPtr.Zero)
                return IntPtr.Zero; // wait this is useless no? because im returinging what would already be.
            

            return pPawn;
        }

        public static Entity GetLocalPlayer(IntPtr controller)
        {
            IntPtr localPlayerPawn = memory.ReadPointer(client + Offsets.dwLocalPlayerPawn);
            LocalPlayerPawn = localPlayerPawn;
            float[] viewMatrix = memory.ReadMatrix(client + Offsets.dwViewMatrix);
            IntPtr gameSceneNode = memory.ReadPointer(LocalPlayerPawn, Offsets.m_pGameSceneNode);
            IntPtr boneMatrix = memory.ReadPointer(gameSceneNode, Offsets.m_modelState + 0x80);
            IntPtr dwSensitivity = memory.ReadPointer(client + Offsets.dwSensitivity);
            float sensitivity = memory.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);
            IntPtr clippingWeapon = memory.ReadPointer(localPlayerPawn + Offsets.m_hActiveWeapon
                );
            IntPtr weaponData = memory.ReadPointer(clippingWeapon + 0x10);
            IntPtr weaponNameAddress = memory.ReadPointer(weaponData + 0x20);
            IntPtr collisionBase = localPlayerPawn + Offsets.m_Collision;
            IntPtr aimPunchServices = memory.ReadPointer(localPlayerPawn + Offsets.m_pAimPunchServices);
            List<EntityTypes.Bone> bones = Calculate.ReadBones(boneMatrix, viewMatrix);

            string weaponName = "Invalid Weapon Name";
            if (weaponNameAddress != 0)
            {
                byte[] buffer = memory.ReadBytes(weaponNameAddress, 32);
                int len = Array.IndexOf<byte>(buffer, 0);
                if (len < 0)
                    len = buffer.Length;

                string raw = System.Text.Encoding.UTF8.GetString(buffer, 0, len);

                if (raw.Length > 7)
                    weaponName = raw.Substring(7);
                else
                    weaponName = raw;
            }

            Entity localPlayer = new()
            {
                PawnAddress = localPlayerPawn,
                Origin = memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                View = memory.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset),
                AimPunchAngle = memory.ReadVec(aimPunchServices + Offsets.m_predictableBaseAngle),
                Position = memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin),
                IsFlashed = memory.ReadFloat(localPlayerPawn, Offsets.m_flFlashScreenshotAlpha) > 1.5,
                Ping = memory.ReadInt(localPlayerPawn, Offsets.m_iPing),
                Health = memory.ReadInt(localPlayerPawn, Offsets.m_iHealth),
                Team = memory.ReadInt(localPlayerPawn + Offsets.m_iTeamNum),
                LifeState = memory.ReadInt(localPlayerPawn, Offsets.m_lifeState),
                Position2D = MathUtils.WorldToScreen(viewMatrix, memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin)),
                ViewPosition2D = MathUtils.WorldToScreen(viewMatrix, Vector3.Add(memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), memory.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset))),
                //Visible => ,
                Head = Vector3.Add(memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), memory.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset)),
                Head2D = MathUtils.WorldToScreen(viewMatrix, Vector3.Add(memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin), memory.ReadVec(localPlayerPawn, Offsets.m_vecViewOffset))),
                Distance = Vector3.Distance(memory.ReadVec(LocalPlayerPawn, Offsets.m_vOldOrigin), memory.ReadVec(localPlayerPawn, Offsets.m_vOldOrigin)),
                Bones = bones,
                Name = memory.ReadString(controller + Offsets.m_iszPlayerName),
                Velocity = memory.ReadVec(LocalPlayerPawn, Offsets.m_vecAbsVelocity),
                ViewAngles = memory.ReadVec(client, Offsets.dwViewAngles),
                Armor = memory.ReadInt(localPlayerPawn, Offsets.m_ArmorValue),
                IsScoped = memory.ReadBool(localPlayerPawn, Offsets.m_bIsScoped),
                IsBuyMenuOpen = memory.ReadBool(localPlayerPawn, Offsets.m_bIsBuyMenuOpen),
                CurrentWeaponName = weaponName,
                Account = memory.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                CashSpent = memory.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                CashSpentTotal = memory.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                ShotsFired = memory.ReadInt(localPlayerPawn, Offsets.m_iShotsFired),
                IsAttacking = memory.ReadBool(client, Offsets.attack),
                Ammo = memory.ReadInt(client, Offsets.m_iAmmo),
                EyeDirection = memory.ReadVec(localPlayerPawn, Offsets.m_angEyeAngles),
                IsWalking = memory.ReadBool(localPlayerPawn, Offsets.m_bIsWalking),
                //HasBomb = memory.ReadBool(localPlayer, Offsets.)
                IsDefusing = memory.ReadBool(localPlayerPawn, Offsets.m_bIsDefusing),
                InBombZone = memory.ReadBool(localPlayerPawn, Offsets.m_bInBombZone),
                Sensitivity = sensitivity,
                GameSceneNode = memory.ReadPointer(localPlayerPawn, Offsets.m_pGameSceneNode),
                Pawn = memory.ReadPointer(controller, Offsets.m_hPawn),

            };
            //localPlayer.Visible = Visible(localPlayer);
            localPlayer.ObserverServices = memory.ReadPointer(localPlayer.Pawn + Offsets.m_pObserverServices);

            localPlayer.EyePosition = GetEyePosition(localPlayer);
            return localPlayer;
        }

        public static Vector3 GetEyePosition(Entity e)
        {
            Vector3 origin = memory.ReadVec(e.GameSceneNode + Offsets.m_vecOrigin);
            Vector3 view = memory.ReadVec(e.PawnAddress + Offsets.m_vecViewOffset);
            return origin + view;
        }

        private static Entity? PopulateEntity(IntPtr pawnAddress, float[] viewMatrix, IntPtr pawn, IntPtr playerPawn, IntPtr controller, IntPtr entityList)
        {
            try
            {
                IntPtr gameSceneNode = memory.Read<IntPtr>(pawnAddress + Offsets.m_pGameSceneNode);
                IntPtr boneMatrix = memory.ReadPointer(gameSceneNode, Offsets.m_modelState + 0x80);
                IntPtr dwSensitivity = memory.ReadPointer(client + Offsets.dwSensitivity);
                float sensitivity = memory.ReadFloat(dwSensitivity + Offsets.dwSensitivity_sensitivity);


                IntPtr weaponServices = memory.ReadPointer(pawnAddress + Offsets.m_pWeaponServices);
                uint hActiveWeapon = memory.ReadUInt(weaponServices + Offsets.m_hActiveWeapon);
                IntPtr weaponData = GetPlayerPawn(entityList, (nint)hActiveWeapon);

                short weaponDefIndex = memory.ReadShort(weaponData + Offsets.m_AttributeManager + Offsets.m_Item + Offsets.m_iItemDefinitionIndex);
                IntPtr collisionBase = pawnAddress + Offsets.m_Collision;
                List<EntityTypes.Bone> bones = Calculate.ReadBones(boneMatrix, viewMatrix);
                WorldEntityManager.WeaponNameMap.TryGetValue(weaponDefIndex, out string? weaponName);
                var viewPos = Vector3.Add(memory.ReadVec(pawnAddress, Offsets.m_vOldOrigin), memory.ReadVec(pawnAddress, Offsets.m_vecViewOffset));
                //if (weaponNameAddress != 0)
                //{
                //    byte[] Buffer = memory.ReadBytes(weaponNameAddress, 32);
                //    int len = Array.IndexOf<byte>(Buffer, 0);
                //    if (len < 0)
                //        len = Buffer.Length;

                //    string raw = System.Text.Encoding.UTF8.GetString(Buffer, 0, len);
                //    Console.WriteLine(raw);
                //    if (raw.Length > 7)
                //        weaponName = raw.Substring(7);
                //    else
                //        weaponName = raw;
                //}


                Entity entity = new()
                {
                    Team = memory.ReadInt(pawnAddress + Offsets.m_iTeamNum),
                    PawnAddress = pawnAddress,
                    Health = (int)memory.ReadUInt(pawnAddress, Offsets.m_iHealth),
                    LifeState = memory.ReadInt(pawnAddress, Offsets.m_lifeState),
                    Position = memory.ReadVec(pawnAddress, Offsets.m_vOldOrigin),
                    View = memory.ReadVec(pawnAddress, Offsets.m_vecViewOffset),
                    Position2D = MathUtils.WorldToScreen(viewMatrix, memory.ReadVec(pawnAddress, Offsets.m_vOldOrigin)),
                    ViewPosition2D = MathUtils.WorldToScreen(viewMatrix, viewPos),
                    //Visible = Visible(entity),
                    Visible = memory.ReadBool(pawnAddress, Offsets.m_entitySpottedState + Offsets.m_bSpotted),
                    SpottedByState = memory.ReadPointer(pawnAddress + 0x2718),
                    Head = viewPos,
                    Head2D = MathUtils.WorldToScreen(viewMatrix, viewPos),
                    Distance = Vector3.Distance(memory.ReadVec(LocalPlayerPawn, Offsets.m_vOldOrigin), memory.ReadVec(pawnAddress, Offsets.m_vOldOrigin)),
                    Bones = bones,
                    Name = memory.ReadString(controller + Offsets.m_iszPlayerName),
                    Velocity = memory.ReadVec(pawnAddress, Offsets.m_vecAbsVelocity),
                    ViewAngles = memory.ReadVec(client, Offsets.dwViewAngles),
                    Armor = memory.ReadInt(pawnAddress, Offsets.m_ArmorValue),
                    IsScoped = memory.ReadBool(pawnAddress, Offsets.m_bIsScoped),
                    IsBuyMenuOpen = memory.ReadBool(pawnAddress, Offsets.m_bIsBuyMenuOpen),
                    CurrentWeaponName = weaponName,
                    Account = memory.ReadInt(GameState.MoneyServices, Offsets.m_iAccount),
                    CashSpent = memory.ReadInt(GameState.MoneyServices, Offsets.m_iCashSpentThisRound),
                    CashSpentTotal = memory.ReadInt(GameState.MoneyServices, Offsets.m_iTotalCashSpent),
                    ShotsFired = memory.ReadInt(pawnAddress, Offsets.m_iShotsFired),
                    IsAttacking = memory.ReadBool(client, Offsets.attack),
                    FlashDuration = memory.ReadFloat(pawnAddress, Offsets.m_flFlashScreenshotAlpha),
                    Ammo = memory.ReadInt(pawnAddress, Offsets.m_iAmmo),
                    EyeDirection = memory.ReadVec(pawnAddress, Offsets.m_angEyeAngles),
                    Ping = (int)memory.ReadUInt(controller, Offsets.m_iPing),
                    IsWalking = memory.ReadBool(pawnAddress, Offsets.m_bIsWalking),
                    AngEyeAngles = memory.ReadVec(pawnAddress, Offsets.m_angEyeAngles),
                    GameSceneNode = gameSceneNode,
                    Sensitivity = sensitivity,
                    EmitSoundTime = memory.ReadFloat(pawnAddress, Offsets.m_flEmitSoundTime),
                    EyePosition = new(0, 0, 0),
                    VecMax = memory.ReadVec(collisionBase + Offsets.m_vecMaxs),
                    VecMin = memory.ReadVec(collisionBase + Offsets.m_vecMins),
                    HitboxComponent = memory.ReadPointer(pawnAddress, Offsets.m_CHitboxComponent),
                    IsDormant = memory.ReadBool(gameSceneNode, Offsets.m_bDormant),
                    Pawn = pawn,
                    PlayerPawn = playerPawn,
                    Controller = controller,
                };

                entity.IsFlashed = entity.FlashDuration > 0.4f;
                entity.EyePosition = GetEyePosition(entity);
                entity.HitBoxes = Calculate.ReadHitboxes(entity, viewMatrix);
                if (entity.Position2D != new Vector2(-99, -99) && !UseOldVisibilityCheck)
                    entity.Visible = VisibilityCheck.IsEntityVisible(entity);

                if (UseOldVisibilityCheck)
                {
                    entity.Visible = memory.ReadBool(pawnAddress, Offsets.m_entitySpottedState + Offsets.m_bSpotted);
                }

                entity.IsEnemy = entity.Team != LocalPlayer.Team;

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
