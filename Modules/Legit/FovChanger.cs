using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui;

namespace Titled_Gui.Modules.Legit
{
    public class FovChanger : Classes.ThreadService // hella detected
    {
        public static uint DesiredFov = 60; // uint for the fov  
        public static int FOV = 60; // default fov int
        public static bool Enabled = false;
        // fov update loop

        public static void UpdateFov()
        {
            if (!Enabled || GameState.localPlayer.Health == 0) return;

           DesiredFov = (uint)FOV; // set desired fov to the current fov
           GameState.CurrentFov = GameState.swed.ReadUInt(GameState.CameraServices + Offsets.m_iFOV); // read current fov
           bool isScoped = GameState.swed.ReadBool(GameState.LocalPlayerPawn, Offsets.m_bIsScoped); // get scoped status
           GameState.CameraServices = GameState.swed.ReadPointer(GameState.LocalPlayerPawn, Offsets.m_pCameraServices);

            if (!isScoped && GameState.CurrentFov != DesiredFov)
            {
                GameState.swed.WriteUInt(GameState.CameraServices + Offsets.m_iFOV, DesiredFov); // set fov if not scoped & not equal to desired fov
            }
        }
        protected override void FrameAction()
        {
            UpdateFov();
        }
    }
}
