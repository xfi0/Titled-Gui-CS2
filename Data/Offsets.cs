using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Data
{
    //okay so shouldve put offsets here the whole time but wtv
    public class Offsets
    {
        public static int m_pCameraServices = 0x11E0; // camera services pointer
        public static int m_iFOV = 0x210; //local player fov
        public static int m_bIsScoped = 0x23E8; // scoped in a sniper
        public static int m_iHealth = 0x344; // entitys health
        public static int m_entitySpottedState = 0x1B58; // entity spotted state
        public static int m_bSpotted = 0x8; // spotted bool
        public static int m_iIDEntIndex = 0x1458; // entity index
        public static int m_pSceneNode = 0x8; // scene node pointer
        public static int dwViewMatrix = 0x1A6B230; // view matrix address
        public static int m_vecViewOffset = 0xCB0; // view offset of the entity
        public static int dwViewAngles = 0x1A75620; // view angles address
        public static int m_lifeState = 0x348;
        public static int m_vOldOrigin = 0x1324;
        public static int m_iTeamNum = 0x3E3;
        public static int m_hPlayerPawn = 0x824;
        public static int dwLocalPlayerPawn = 0x18560D0;
        public static int dwEntityList = 0x1A020A8;
        public static int m_flFlashBangTime = 0x13F8;
        public static int m_modelState = 0x170;
        public static int m_pGameSceneNode = 0x328;
    }
}
