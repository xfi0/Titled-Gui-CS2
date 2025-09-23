using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Classes
{
    internal class MoneyServices : ThreadService
    {
        public static void UpdateStuff()
        {
            GameState.MoneyServices = GameState.swed.ReadPointer(GameState.currentController, Offsets.m_pInGameMoneyServices);
        }
        public static void MoneyTest()
        {
            foreach (Entity e in GameState.Entities)
            {
                if (e == null) return;

                //Console.WriteLine(e.Account);
                //Console.WriteLine(e.CashSpent);
                //Console.WriteLine(e.CashSpentTotal);
                //Thread.Sleep(100);
            }
        }
        protected override void FrameAction()
        {
            UpdateStuff();
            //MoneyTest();
        }
    }
}
