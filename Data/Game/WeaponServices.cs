namespace Titled_Gui.Data.Game
{
    internal class WeaponServices : Classes.ThreadService
    {
        public static void Update()
        {
            GameState.WeaponServices = GameState.swed.ReadUInt(GameState.client, Offsets.m_pWeaponServices);
        }
        protected override void FrameAction()
        {
            Update();
        }
    }
}
