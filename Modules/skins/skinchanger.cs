// SkinChanger.cs
using System;
using System.Collections.Generic;
using System.Threading;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public class SkinChanger : Classes.ThreadService
    {
        public static bool Enabled = false;
        public static bool OnlyActiveWeapon = true;
        public static bool ForceUpdate = true;

        private static short lastWeaponIndex = -1;
        private static readonly object lockObject = new object();

        private static readonly Dictionary<short, int> WeaponSkins = new Dictionary<short, int>
        {
            { 7, 433 },  // AK-47
            { 9, 344 },  // AWP
            { 4, 437 },  // M4A4
            { 61, 504 }, // USP-S
            { 1, 38 },   // Desert Eagle
            { 16, 624 }, // M4A1-S
            { 17, 433 }, // USP-S (alternatywny)
            { 24, 624 }, // UMP-45
            { 25, 433 }, // P250
            { 26, 344 }, // G3SG1
            { 27, 437 }, // FAMAS
            { 28, 504 }, // M249
            { 29, 38 },  // Negev
            { 30, 624 }, // Galil AR
            { 31, 433 }, // TEC-9
            { 32, 344 }, // P2000
            { 33, 437 }, // MP7
            { 34, 504 }, // MP9
            { 35, 38 },  // Nova
            { 36, 624 }, // P90
            { 38, 433 }, // Sawed-Off
            { 39, 344 }, // MAG-7
            { 40, 437 }, // Bizon
            { 60, 504 }, // M4A1-S (nowy)
            { 63, 38 }   // CZ75-Auto
        };

        protected override void FrameAction()
        {
            if (!Enabled)
            {
                Thread.Sleep(100);
                return;
            }

            try
            {
                ApplySkinLogic();
                Thread.Sleep(50); // Małe opóźnienie aby nie obciążać CPU
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SkinChanger Thread Error] {ex.Message}");
                Thread.Sleep(1000);
            }
        }

        private void ApplySkinLogic()
        {
            if (GameState.LocalPlayerPawn == IntPtr.Zero ||
                GameState.LocalPlayer.Health <= 0)
                return;

            if (OnlyActiveWeapon)
            {
                ApplySkinToCurrentWeapon();
            }
            else
            {
                ApplySkinsToAllWeapons();
            }
        }

        private void ApplySkinToCurrentWeapon()
        {
            try
            {
                // Pobierz aktywną broń
                IntPtr weaponServices = GameState.swed.ReadPointer(
                    GameState.LocalPlayerPawn + Offsets.m_pWeaponServices);

                if (weaponServices == IntPtr.Zero) return;

                // Pobierz uchwyt aktywnej broni
                uint activeWeaponHandle = GameState.swed.ReadUInt(
                    weaponServices + Offsets.m_hActiveWeapon);

                if (activeWeaponHandle == 0) return;

                // Konwertuj handle na wskaźnik
                IntPtr weapon = GetWeaponFromHandle(activeWeaponHandle);
                if (weapon == IntPtr.Zero) return;

                // Pobierz index broni
                short weaponIndex = GameState.swed.ReadShort(weapon + Offsets.m_iItemDefinitionIndex);

                // Sprawdź czy broń się zmieniła
                lock (lockObject)
                {
                    if (weaponIndex != lastWeaponIndex)
                    {
                        lastWeaponIndex = weaponIndex;

                        if (WeaponSkins.TryGetValue(weaponIndex, out int skinId))
                        {
                            Console.WriteLine($"[SkinChanger] Weapon changed to: {weaponIndex}, applying skin: {skinId}");
                            ApplySkinToWeapon(weapon, weaponIndex, skinId);

                            if (ForceUpdate)
                                ForceViewModelUpdate();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CurrentWeapon Error] {ex.Message}");
            }
        }

        private void ApplySkinsToAllWeapons()
        {
            try
            {
                IntPtr weaponServices = GameState.swed.ReadPointer(
                    GameState.LocalPlayerPawn + Offsets.m_pWeaponServices);

                if (weaponServices == IntPtr.Zero) return;

                // Podejście: Przejdź przez sloty broni (0-7)
                for (int slot = 0; slot < 8; slot++)
                {
                    try
                    {
                        uint weaponHandle = GameState.swed.ReadUInt(
                            weaponServices + Offsets.m_hMyWeapons + (slot * 0x4));

                        if (weaponHandle == 0) continue;

                        IntPtr weapon = GetWeaponFromHandle(weaponHandle);
                        if (weapon == IntPtr.Zero) continue;

                        short weaponIndex = GameState.swed.ReadShort(weapon + Offsets.m_iItemDefinitionIndex);

                        if (WeaponSkins.TryGetValue(weaponIndex, out int skinId))
                        {
                            ApplySkinToWeapon(weapon, weaponIndex, skinId);
                        }
                    }
                    catch { /* Ignoruj błędy dla tego slotu */ }
                }

                if (ForceUpdate)
                    ForceNetworkUpdate();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AllWeapons Error] {ex.Message}");
            }
        }

        private IntPtr GetWeaponFromHandle(uint handle)
        {
            try
            {
                IntPtr listEntry = GameState.swed.ReadPointer(
                    (nint)(GameState.client + Offsets.dwEntityList +
                    (0x8 * ((handle & 0x7FFF) >> 9)) + 0x10));

                if (listEntry == IntPtr.Zero) return IntPtr.Zero;

                return GameState.swed.ReadPointer((nint)(listEntry + 0x78 * (handle & 0x1FF)));
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private bool ApplySkinToWeapon(IntPtr weapon, short weaponIndex, int skinId)
        {
            try
            {
                // Sprawdź aktualny skin
                int currentSkin = GameState.swed.ReadInt(weapon + Offsets.m_nFallbackPaintKit);

                if (currentSkin != skinId)
                {
                    // Zastosuj nowy skin
                    GameState.swed.WriteInt(weapon + Offsets.m_iItemIDHigh, -1);
                    GameState.swed.WriteInt(weapon + Offsets.m_nFallbackPaintKit, skinId);
                    GameState.swed.WriteFloat(weapon + Offsets.m_flFallbackWear, 0.001f);
                    GameState.swed.WriteInt(weapon + Offsets.m_nFallbackStatTrak, 1337);
                    GameState.swed.WriteInt(weapon + Offsets.m_nFallbackSeed, 0);

                    // Ustaw jako StatTrak
                    GameState.swed.WriteInt(weapon + Offsets.m_iEntityQuality, 9);

                    Console.WriteLine($"[SkinChanger] Applied skin {skinId} to weapon {weaponIndex}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ApplySkin Error for weapon {weaponIndex}] {ex.Message}");
            }
            return false;
        }

        private void ForceViewModelUpdate()
        {
            try
            {
                // Znajdź ViewModel
                IntPtr viewModelServices = GameState.swed.ReadPointer(
                    GameState.LocalPlayerPawn + Offsets.m_pViewModelServices);

                if (viewModelServices != IntPtr.Zero)
                {
                    uint viewModelHandle = GameState.swed.ReadUInt(viewModelServices + Offsets.m_hViewModel);

                    if (viewModelHandle != 0)
                    {
                        IntPtr viewModel = GetWeaponFromHandle(viewModelHandle);
                        if (viewModel != IntPtr.Zero)
                        {
                            IntPtr sceneNode = GameState.swed.ReadPointer(viewModel + 0x318);
                            if (sceneNode != IntPtr.Zero)
                            {
                                GameState.swed.WriteULong(sceneNode + 0x160, 2);
                            }
                        }
                    }
                }
            }
            catch { /* Ignoruj błędy ViewModel */ }
        }

        private void ForceNetworkUpdate()
        {
            try
            {
                if (Offsets.dwNetworkGameClient != 0 && GameState.engine != IntPtr.Zero)
                {
                    IntPtr networkGameClient = GameState.swed.ReadPointer(
                        GameState.engine + Offsets.dwNetworkGameClient);

                    if (networkGameClient != IntPtr.Zero)
                    {
                        GameState.swed.WriteInt(networkGameClient + 0x258, -1);
                    }
                }
            }
            catch { /* Ignoruj błędy network update */ }
        }

        // Metoda do ręcznego wymuszenia zmiany skinu
        public static void ForceApplyCurrentWeapon()
        {
            lock (lockObject)
            {
                lastWeaponIndex = -1; // Zresetuj, aby wymusić ponowną aplikację
            }
        }

        // Metoda do zmiany skinu dla konkretnej broni
        public static void SetWeaponSkin(short weaponId, int skinId)
        {
            if (WeaponSkins.ContainsKey(weaponId))
                WeaponSkins[weaponId] = skinId;
            else
                WeaponSkins.Add(weaponId, skinId);

            ForceApplyCurrentWeapon();
        }

        // Metoda do usunięcia skinu
        public static void RemoveWeaponSkin(short weaponId)
        {
            if (WeaponSkins.ContainsKey(weaponId))
            {
                WeaponSkins.Remove(weaponId);
            }
        }
    }
}