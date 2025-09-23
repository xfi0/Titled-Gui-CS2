namespace Titled_Gui.Data.Game
{
    internal class GlobalVar : Classes.ThreadService
    {
        private static ulong address;
        private static readonly ulong TickCountOffset = 0x48;
        private static readonly ulong RealTimeOffset = 0x00;
        private static readonly ulong FrameCountOffset = 0x04;
        private static readonly ulong MaxClientsOffset = 0x10;
        private static readonly ulong IntervalPerTickOffset = 0x14;
        private static readonly ulong IntervalPerTick2Offset = 0x44;
        private static readonly ulong CurrentTimeOffset = 0x30;
        private static readonly ulong CurrentTime2Offset = 0x38;
        //private static ulong CurrentNetchanOffset = 0x00;
        private static readonly ulong CurrentMapOffset = 0x0180;
        private static readonly ulong CurrentMapNameOffset = 0x0188;

        public static void Update()
        {
            address = GameState.swed.ReadULong((nint)(GameState.client + Offsets.dwGlobalVars));
        }


        public static int GetTickCount()
        {
            if (address == 0) return -1;

            int tickCount = GameState.swed.ReadInt((nint)(address + TickCountOffset));
            return tickCount;
        }

        public static float GetRealTime()
        {
            return GameState.swed.ReadFloat((nint)(address + RealTimeOffset));
        }

        public static int GetFrameCount()
        {
            return GameState.swed.ReadInt((nint)(address + FrameCountOffset));
        }

        public static int GetMaxClients()
        {
            return GameState.swed.ReadInt((nint)(address + MaxClientsOffset));
        }

        public static int GetIntervalPerTick()
        {
            return GameState.swed.ReadInt((nint)(address + IntervalPerTickOffset));
        }

        public static float GetIntervalPerTick2()
        {
            return GameState.swed.ReadFloat((nint)(address + IntervalPerTick2Offset));
        }

        public static float GetcurrentTime()
        {
            return GameState.swed.ReadFloat((nint)(address + CurrentTimeOffset));
        }

        public static float GetcurrentTime2()
        {
            return GameState.swed.ReadFloat((nint)(address + CurrentTime2Offset));
        }

        public static int GetCurrentMap()
        {
            return GameState.swed.ReadChar((nint)(address + CurrentMapOffset));
        }

        public static int GetCurrentMapName()
        {
            return GameState.swed.ReadChar((nint)(address + CurrentMapNameOffset));
        }
        protected override void FrameAction()
        {
            Update();
        }
    }
}
