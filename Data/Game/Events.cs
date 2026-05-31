using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.Data.Game
{
    internal class Events
    {
        public static class GameEvents
        {
            public static event Action<string>? OnMapChanged = null;
            public static void BroadcastMapChanged(string newMap)
            {
                OnMapChanged?.Invoke(newMap);
            }
        }
    }
}
