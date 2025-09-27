using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    public class FovChanger : Classes.ThreadService // hella detected
    {
        public static uint DesiredFov = 60; 
        public static int FOV = 60; 
        public static bool Enabled = false;
        // fov update loop

        public static void UpdateFov()
        {
            if (!Enabled || GameState.LocalPlayer.Health == 0) return;

           DesiredFov = (uint)FOV; // set desired fov to the current fov
           GameState.CurrentFov = GameState.swed.ReadUInt(GameState.CameraServices + Offsets.m_iFOV); // read current fov
           GameState.IsScoped = GameState.swed.ReadBool(GameState.LocalPlayerPawn, Offsets.m_bIsScoped); // get scoped status
           GameState.CameraServices = GameState.swed.ReadPointer(GameState.LocalPlayerPawn, Offsets.m_pCameraServices);

            if (!GameState.IsScoped && GameState.CurrentFov != DesiredFov)
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
