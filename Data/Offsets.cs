using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Data
{
    //okay so shouldve put offsets here the whole time but wtv
    public class Offsets //TODO 2 of these dont auto update, i think theyre unused tho
    {
        public static int m_pCameraServices = 0x11E0; // camera services pointer
        public static int m_iFOV = 0x210; //local player fov
        public static int m_bIsScoped = 0x23E8; // scoped in a sniper
        public static int m_iHealth = 0x344; // entitys health
        public static int m_entitySpottedState = 0x1B58; // entity spotted state
        public static int m_bSpotted = 0x8; // spotted bool
        public static int m_iIDEntIndex = 0x1458; // entity index
        public static int m_pSceneNode = 0x8; // scene node pointer
        public static int dwViewMatrix = 0x1A6D260; // view matrix address
        public static int m_vecViewOffset = 0xCB0; // view offset of the entity
        public static int dwViewAngles = 0x1A774D0; // view angles address
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
        public static int dwGameRules = 0x1A66B38;
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
    }
}
