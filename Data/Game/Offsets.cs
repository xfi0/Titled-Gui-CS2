namespace Titled_Gui.Data.Game
{
    public class Offsets 
    {
        public static int m_pCameraServices = 0x11E0; 
        public static int m_iFOV = 0x210; 
        public static int m_bIsScoped = 0x23E8; 
        public static int m_iHealth = 0x344; 
        public static int m_entitySpottedState = 0x1B58; 
        public static int m_bSpotted = 0x8; 
        public static int m_iIDEntIndex = 0x1458; 
        public static int m_pSceneNode = 0x8; 
        public static int dwViewMatrix = 0x1A6D260; 
        public static int m_vecViewOffset = 0xCB0;
        public static int dwViewAngles = 0x1A774D0; 
        public static int m_lifeState = 0x348;
        public static int m_vOldOrigin = 0x1324;
        public static int m_iTeamNum = 0x3E3;
        public static int m_hPlayerPawn = 0x824;
        public static int dwLocalPlayerPawn = 0x18580D0;
        public static int dwEntityList = 0x1A044C0;
        public static int m_flFlashBangTime = 0x13F8;
        public static int m_modelState = 0x170;
        public static int m_pGameSceneNode = 0x328;
        public static int dwCSGOInput = 0x1A75250;
        public static int m_flC4Blow = 0xFC0;
        public static int current_time = 0x5C0;
        public static int dwGlobalVars = 0x1849EB0;
        public static int dwPlantedC4 = 0x1A702F8;
        public static int m_bBombPlanted = 0x1B7B;
        public static int dwGameRules = 0x9A5;
        public static int dwSensitivity_sensitivity = 0x40;
        public static int dwSensitivity = 0x1A67858;
        public static int m_iszPlayerName = 0x660;
        public static int m_pClippingWeapon = 0x13A0;
        public static int m_Item = 0x50;
        public static int m_iItemDefinitionIndex = 0x1BA;
        public static int m_AttributeManager = 0x1148;
        public static int attack = 0x184E8F0;
        public static int jump = 0x1850DF0;
        public static int m_bSpottedByMask = 0xC;
        public static int m_pWeaponServices = 0x11A8;
        public static int m_hActiveWeapon = 0x58;
        public static int m_vecAbsVelocity = 0x3F0;
        public static int m_fFlags = 0x3EC;
        // im giving some offsets 0x0 cause my offset getter will fill them in either way.
        public static int m_hMyWeapons = 0x0;
        public static int m_aimPunchAngle = 0x00;
        public static int m_nCurrentTickThisFrame = 0x0;
        public static int m_ArmorValue = 0x275C;
        public static int m_pInGameMoneyServices = 0x0;
        public static int m_iAccount = 0x0;
        public static int m_iTotalCashSpent = 0x0;
        public static int m_iCashSpentThisRound = 0x0;
        public static int m_aimPunchCache = 0x0;
        public static int m_bIsBuyMenuOpen = 0x0;
        public static int m_iAmmo = 0x0;
        public static int m_angEyeAngles = 0x0;
        public static int m_iShotsFired = 0x0;
        public static int m_pActionTrackingServices = 0x0;
        public static int m_iNumRoundKills = 0x0;
        public static int m_iNumRoundKillsHeadshots = 0x0;
        public static int m_flTotalRoundDamageDealt = 0x0;
        public static int m_pBulletServices = 0x0;
        public static int m_totalHitsOnServer = 0x0;
        public static int m_iSpectatorSlotCount = 0x0;
        public static int m_iPing = 0x0;
        public static int m_bIsWalking = 0x0;
        public static int dwLocalPlayerController = 0x0;
        public static int m_bIsDefusing = 0x0;
        public static int m_bInBombZone = 0x0;
        public static int m_vecAbsOrigin = 0x0;
        public static int m_GunGameImmunityColor = 0x0;
        public static int m_flEmitSoundTime = 0x0;
        public static int m_vecOldViewAngles = 0x0;
        public static int m_vecLastMovementImpulses = 0x0;
        public static int m_flUpMove = 0x0;
        public static int m_flLeftMove = 0x0;
        public static int m_flForwardMove = 0x0;
        public static int m_arrForceSubtickMoveWhen = 0x0;
        public static int m_vecCsViewPunchAngle = 0x0;
        public static int m_flInaccuracyPitchShift = 0x0;
        public static int m_flMaxspeed = 0x0;
        public static int m_pMovementServices = 0x0;
        public static int dwInputSystem = 0x0;

        public static int dwNetworkGameClient = 0x0;
        public static int dwLocalPlayer = 0x0;
      
        public static int m_flFallbackWear = 0x0;
        public static int m_nFallbackPaintKit = 0x0;
        public static int m_nFallbackSeed = 0x0;
        public static int m_nFallbackStatTrak = 0x0;
       
        public static int m_iItemIDHigh = 0x0;
        public static int m_iEntityQuality = 0x0;
        public static int m_iAccountID = 0x0;
        public static int m_OriginalOwnerXuidLow = 0x0;
        public static int m_WeaponCount = 0x0;
        public static int m_pViewModelServices = 0x0;
        public static int m_hViewModel = 0x0;
        // Dodaj te offsety do Offsets.cs

        public static int m_vecBulletHitModels = 0x0; // z C_CSPlayerPawn - lista C_BulletHitModel*

        // Offsety dla C_BulletHitModel
        public static int bullet_matLocal = 0x0; // matrix3x4_t
        public static int bullet_iBoneIndex = 0x0; // int32
        public static int bullet_hPlayerParent = 0x0; // CHandle<C_BaseEntity>
        public static int bullet_bIsHit = 0x0; // bool
        public static int bullet_flTimeCreated = 0x0; // float32
        public static int bullet_vecStartPos = 0x0; // Vector
        public static int m_sSanitizedPlayerName = 0x0;
        public static int m_hObserverTarget = 0x0;
        public static int m_pObserverServices = 0x0;
        public static int m_hPawn = 0x0;

        public static int m_hObserverPawn = 0x0;
        public static int m_iObserverMode = 0x0;
        public static int m_hController = 0x0;


        public static int m_nSmokeEffectTickBegin = 0x1468;
        public static int m_bDidSmokeEffect = 0x146C;
        public static int m_nRandomSeed = 0x1470;
        public static int m_vSmokeColor = 0x1474;
        public static int m_vSmokeDetonationPos = 0x1480;
        public static int m_VoxelFrameData = 0x1490;
        public static int m_nVoxelFrameDataSize = 0x14A8;
        public static int m_nVoxelUpdate = 0x14AC;
        public static int m_bSmokeVolumeDataReceived = 0x14B0;
        public static int m_bSmokeEffectSpawned = 0x14B1;

        // Entity class ID dla smoke grenade
        public static int SmokeGrenadeClassID = 157; // CS2: C_SmokeGrenadeProjectile

        // Dodaj te do Offsets.cs
        public static int m_iRenderFX = 0x0;        // RenderFX offset
        public static int m_clrRender = 0x0;        // ColorRender (RGBA)
        public static int m_nRenderMode = 0x0;      // RenderMode
        public static int m_bShouldGlow = 0x0;      // Czy ma świecić
        public static int m_flGlowColor = 0x0;      // Kolor glow
        public static int m_iGlowIndex = 0x0;       // Indeks glow w glow managerze
        public static int m_iGlowType = 0x0;        // Typ glow (0-3)
        public static int dwGlowManager = 0x0;      // Adres GlowManager

    }
}
