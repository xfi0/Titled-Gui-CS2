using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Data.Game
{
    internal class ActionTrackingServices // i dont think this is right add the threaded service when this needs to be used, removed for cpu useage sake
    {
        public static void Update()
        {
            GameState.LocalController = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerController);
            GameState.ActionTrackingServices = GameState.swed.ReadPointer(LocalController, Offsets.m_pActionTrackingServices);

            if (GameState.ActionTrackingServices == IntPtr.Zero) return;

            //Console.WriteLine(GameState.swed.ReadFloat(GameState.ActionTrackingServices + Offsets.m_iNumRoundKillsHeadshots));
        }
        //protected override void FrameAction()
        //{
        //    Update(); 
        //}
    }
}
