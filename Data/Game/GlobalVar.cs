using System;

namespace Titled_Gui.Data.Game
{
    internal class GlobalVar
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
            Update();
            if (address == 0) return -1;

            int tickCount = GameState.swed.ReadInt((nint)(address + TickCountOffset));
            return tickCount;
        }

        public static float GetRealTime()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadFloat((nint)(address + RealTimeOffset));
        }

        public static int GetFrameCount()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadInt((nint)(address + FrameCountOffset));
        }

        public static int GetMaxClients()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadInt((nint)(address + MaxClientsOffset));
        }

        public static int GetIntervalPerTick()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadInt((nint)(address + IntervalPerTickOffset));
        }

        public static float GetIntervalPerTick2()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadFloat((nint)(address + IntervalPerTick2Offset));
        }

        public static float GetcurrentTime()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadFloat((nint)(address + CurrentTimeOffset));
        }

        public static float GetcurrentTime2()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadFloat((nint)(address + CurrentTime2Offset));
        }

        public static int GetCurrentMap()
        {
            Update();
            if (address == 0) return -1;

            return GameState.swed.ReadChar((nint)(address + CurrentMapOffset));
        }

        public static string GetCurrentMapName()
        {
            Update();
            if (address == 0) return "";

            nint mapNamePtr = GameState.swed.ReadPointer((nint)(address + CurrentMapOffset));

            string raw = GameState.swed.ReadString(mapNamePtr, 64);
            raw = raw.Trim('\0', ' ', '\r', '\n').Split('\0')[0].Replace("?", "").Replace("\0", "");
            if (raw.Length > 0 && raw[0] == '_')
                raw = raw.Substring(1);

            return raw;
            //return GameState.swed.ReadString((nint)(address + CurrentMapNameOffset), 64);
        }
    }
}
