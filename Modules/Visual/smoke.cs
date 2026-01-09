using System.Collections.Concurrent;
using System.Numerics;
using System.Runtime.InteropServices;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class SmokeManager : ThreadService
    {
        public static bool AntiSmokeEnabled = false;
        public static bool SmokeColorChangerEnabled = false;
        public static Vector3 CustomSmokeColor = new Vector3(1.0f, 0.0f, 0.0f);
        public static float SmokeAlpha = 0.1f;

        private static ConcurrentDictionary<IntPtr, SmokeData> smokeData = new();
        private static List<IntPtr> currentSmokes = new();

        // Offsety
        private static int m_vecOrigin = 0xC8; // Vector origin offset
        private static Vector3 hiddenPosition = new Vector3(9999f, 9999f, 9999f);

        private class SmokeData
        {
            public Vector3 OriginalPosition { get; set; }
            public Vector3 OriginalColor { get; set; }
            public DateTime LastSeen { get; set; }
            public bool IsHidden { get; set; }
        }

        protected override void FrameAction()
        {
            Update();
        }

        public static void Update()
        {
            try
            {
                if (!AntiSmokeEnabled && !SmokeColorChangerEnabled)
                {
                    RestoreAllSmokes();
                    return;
                }

                FindAndProcessSmokes();
                CleanupOldSmokes();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SmokeManager Error: {ex.Message}");
            }
        }

        private static void FindAndProcessSmokes()
        {
            currentSmokes.Clear();

            if (client == IntPtr.Zero)
                return;

            IntPtr entityList = swed.ReadPointer(client + Offsets.dwEntityList);
            if (entityList == IntPtr.Zero)
                return;

            for (int i = 0; i < 512; i++)
            {
                try
                {
                    IntPtr listEntry = swed.ReadPointer(entityList, 0x8 * ((i & 0x7FFF) >> 9) + 0x10);
                    if (listEntry == IntPtr.Zero)
                        continue;

                    IntPtr entity = swed.ReadPointer(listEntry, 0x70 * (i & 0x1FF));
                    if (entity == IntPtr.Zero)
                        continue;

                    if (IsSmokeGrenade(entity))
                    {
                        currentSmokes.Add(entity);
                        ProcessSmoke(entity);
                    }
                }
                catch { }
            }
        }

        private static bool IsSmokeGrenade(IntPtr entity)
        {
            try
            {
                int classId = GetEntityClassID(entity);
                return classId == Offsets.SmokeGrenadeClassID;
            }
            catch
            {
                return false;
            }
        }

        private static int GetEntityClassID(IntPtr entity)
        {
            try
            {
                IntPtr clientNetworkable = swed.ReadPointer(entity + 0x8);
                if (clientNetworkable == IntPtr.Zero)
                    return -1;

                IntPtr vtable = swed.ReadPointer(clientNetworkable);
                if (vtable == IntPtr.Zero)
                    return -1;

                IntPtr getClientClassFunc = swed.ReadPointer(vtable + 0x8);
                if (getClientClassFunc == IntPtr.Zero)
                    return -1;

                GetClientClassDelegate getClientClass =
                    Marshal.GetDelegateForFunctionPointer<GetClientClassDelegate>(getClientClassFunc);

                IntPtr clientClass = getClientClass(clientNetworkable);
                if (clientClass == IntPtr.Zero)
                    return -1;

                return swed.ReadInt(clientClass + 0x20);
            }
            catch
            {
                // Alternatywna prosta metoda - sprawdź czy to smok po innych właściwościach
                return -1;
            }
        }

        private static void ProcessSmoke(IntPtr smokeEntity)
        {
            try
            {
                if (!smokeData.ContainsKey(smokeEntity))
                {
                    Vector3 originalPos = ReadVector3(smokeEntity, m_vecOrigin);
                    Vector3 originalColor = ReadVector3(smokeEntity, Offsets.m_vSmokeColor);

                    smokeData[smokeEntity] = new SmokeData
                    {
                        OriginalPosition = originalPos,
                        OriginalColor = originalColor,
                        LastSeen = DateTime.Now,
                        IsHidden = false
                    };
                }
                else
                {
                    var data = smokeData[smokeEntity];
                    data.LastSeen = DateTime.Now;
                    smokeData[smokeEntity] = data;
                }

                if (AntiSmokeEnabled)
                {
                    HideSmoke(smokeEntity);
                }
                else if (SmokeColorChangerEnabled)
                {
                    RestoreSmokePosition(smokeEntity);
                    ChangeSmokeColor(smokeEntity);
                }
            }
            catch { }
        }

        private static void HideSmoke(IntPtr smokeEntity)
        {
            try
            {
                if (smokeData.TryGetValue(smokeEntity, out var data) && !data.IsHidden)
                {
                    // Przenieś smoka poza mapę
                    WriteVector3(smokeEntity, m_vecOrigin, hiddenPosition);

                    // Wyłącz efekty dymu (zapisz 0 jako int)
                    swed.WriteInt(smokeEntity + Offsets.m_bSmokeEffectSpawned, 0);
                    swed.WriteInt(smokeEntity + Offsets.m_bDidSmokeEffect, 0);

                    data.IsHidden = true;
                    smokeData[smokeEntity] = data;
                }
            }
            catch { }
        }

        private static void RestoreSmokePosition(IntPtr smokeEntity)
        {
            try
            {
                if (smokeData.TryGetValue(smokeEntity, out var data) && data.IsHidden)
                {
                    // Przywróć oryginalną pozycję
                    WriteVector3(smokeEntity, m_vecOrigin, data.OriginalPosition);

                    // Włącz efekty dymu (zapisz 1 jako int)
                    swed.WriteInt(smokeEntity + Offsets.m_bSmokeEffectSpawned, 1);
                    swed.WriteInt(smokeEntity + Offsets.m_bDidSmokeEffect, 1);

                    data.IsHidden = false;
                    smokeData[smokeEntity] = data;
                }
            }
            catch { }
        }

        private static void ChangeSmokeColor(IntPtr smokeEntity)
        {
            try
            {
                Vector3 finalColor = CustomSmokeColor * SmokeAlpha;
                WriteVector3(smokeEntity, Offsets.m_vSmokeColor, finalColor);
            }
            catch { }
        }

        public static void RestoreAllSmokes()
        {
            try
            {
                foreach (var kvp in smokeData)
                {
                    if (kvp.Value.IsHidden)
                    {
                        RestoreSmokePosition(kvp.Key);
                        WriteVector3(kvp.Key, Offsets.m_vSmokeColor, kvp.Value.OriginalColor);
                    }
                }
            }
            catch { }
        }

        private static void CleanupOldSmokes()
        {
            try
            {
                var toRemove = new List<IntPtr>();
                var cutoffTime = DateTime.Now.AddSeconds(-10);

                foreach (var kvp in smokeData)
                {
                    if (kvp.Value.LastSeen < cutoffTime)
                    {
                        toRemove.Add(kvp.Key);
                    }
                }

                foreach (var key in toRemove)
                {
                    smokeData.TryRemove(key, out _);
                }
            }
            catch { }
        }

        // Pomocnicze metody do pracy z Vector3 używając tylko WriteFloat/ReadFloat
        private static void WriteVector3(IntPtr address, int offset, Vector3 value)
        {
            swed.WriteFloat(address + offset, value.X);
            swed.WriteFloat(address + offset + 4, value.Y);
            swed.WriteFloat(address + offset + 8, value.Z);
        }

        private static Vector3 ReadVector3(IntPtr address, int offset)
        {
            return new Vector3(
                swed.ReadFloat(address + offset),
                swed.ReadFloat(address + offset + 4),
                swed.ReadFloat(address + offset + 8)
            );
        }

        // Delegat dla GetClientClass
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr GetClientClassDelegate(IntPtr pThis);

        public static void DebugInfo()
        {
            Console.WriteLine($"Active smokes: {currentSmokes.Count}");
            Console.WriteLine($"Cached smoke data: {smokeData.Count}");

            foreach (var smoke in smokeData)
            {
                Console.WriteLine($"Smoke 0x{smoke.Key:X}: Hidden={smoke.Value.IsHidden}");
            }
        }

        // Prosta metoda wykrywania smoków po propertysach jeśli ClassID nie działa
        private static bool IsSmokeByProperties(IntPtr entity)
        {
            try
            {
                // Sprawdź czy ma offsety typowe dla smoke grenade
                // Możesz dodać więcej sprawdzeń
                float smokeTime = swed.ReadFloat(entity + Offsets.m_nSmokeEffectTickBegin);

                // Smoke grenade ma zazwyczaj określone wartości
                return true; // Tymczasowo zawsze true - dostosuj do swoich potrzeb
            }
            catch
            {
                return false;
            }
        }
    }
}