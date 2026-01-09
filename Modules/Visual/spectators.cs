using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Entity;
using ImGuiNET;
using Swed64;

namespace Titled_Gui.Modules.Visual
{
    public class SpectatorList
    {
        // Konfiguracja
        public static bool Enabled = false;
        public static Vector2 Position = new Vector2(10, 100);

        // Styl - Spectators (niebieski)
        public static Vector4 WindowBgColor = new Vector4(0.08f, 0.08f, 0.10f, 0.92f);
        public static Vector4 BorderColor = new Vector4(0.26f, 0.59f, 0.98f, 0.8f);
        public static Vector4 AccentColor = new Vector4(0.26f, 0.59f, 0.98f, 1f);

        // Styl - Spectating (pomarańczowy)
        public static Vector4 SpectatingWindowBgColor = new Vector4(0.15f, 0.10f, 0.05f, 0.92f);
        public static Vector4 SpectatingBorderColor = new Vector4(1.0f, 0.65f, 0.0f, 0.8f);
        public static Vector4 SpectatingAccentColor = new Vector4(1.0f, 0.65f, 0.0f, 1f);

        public static Vector4 TextColor = new Vector4(0.95f, 0.95f, 0.95f, 1f);

        // Wymiary
        public static float Width = 200f;
        public static float ItemHeight = 24f;
        public static float HeaderHeight = 30f;
        public static float Padding = 10f;
        public static float Rounding = 8f;

        private static List<string> spectators = new List<string>();
        private static string spectatingTarget = "";
        private static int spectatingMode = 0;
        private static bool isSpectating = false;
        private static DateTime lastUpdateTime = DateTime.Now;
        private static bool debugMode = true; // Włącz debugowanie!

        public static void Update()
        {
            if (!Enabled)
                return;

            // Update co 300ms
            if ((DateTime.Now - lastUpdateTime).TotalMilliseconds < 300)
                return;

            try
            {
                // DEBUG: Wyświetl status
                if (debugMode)
                    Console.WriteLine($"[SpectatorList] Starting update...");

                UpdateSpectators();
                UpdateSpectating();

                lastUpdateTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpectatorList] Update error: {ex.Message}");
            }
        }

        private static void UpdateSpectators()
        {
            spectators.Clear();

            try
            {
                if (GameState.client == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] client is zero");
                    return;
                }

                // Sprawdź czy lokalny gracz jest żywy
                IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
                if (localPlayerPawn == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] localPlayerPawn is zero");
                    return;
                }

                int localHealth = GameState.swed.ReadInt(localPlayerPawn + Offsets.m_iHealth);
                if (localHealth <= 0)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] Local player is dead, skipping spectators");
                    return; // Martwi nie mają spectatorów
                }

                // Pobierz entity list
                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] entityList is zero");
                    return;
                }

                // Pobierz list entry
                IntPtr listEntry = GameState.swed.ReadPointer(entityList + 0x10);
                if (listEntry == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] listEntry is zero");
                    return;
                }

                for (int i = 0; i < 64; i++)
                {
                    try
                    {
                        // Pobierz controller
                        IntPtr controller = GameState.swed.ReadPointer(listEntry + (i * 0x78));
                        if (controller == IntPtr.Zero)
                            continue;

                        // Pobierz pawn handle z controllera
                        uint pawnHandle = GameState.swed.ReadUInt(controller + Offsets.m_hPawn);
                        if (pawnHandle == 0)
                            continue;

                        // Resolve pawn
                        IntPtr pawn = ResolveEntityHandle(pawnHandle);
                        if (pawn == IntPtr.Zero)
                            continue;

                        // Sprawdź czy to nie local player
                        if (pawn == localPlayerPawn)
                            continue;

                        // Sprawdź czy gracz jest martwy (tylko martwi mogą spectować)
                        int health = GameState.swed.ReadInt(pawn + Offsets.m_iHealth);
                        if (health > 0)
                            continue;

                        // Pobierz observer services
                        IntPtr observerServices = GameState.swed.ReadPointer(pawn + Offsets.m_pObserverServices);
                        if (observerServices == IntPtr.Zero)
                            continue;

                        // Pobierz observer target
                        uint targetHandle = GameState.swed.ReadUInt(observerServices + Offsets.m_hObserverTarget);
                        if (targetHandle == 0)
                            continue;

                        // Resolve target pawn
                        IntPtr targetPawn = ResolveEntityHandle(targetHandle);
                        if (targetPawn == IntPtr.Zero)
                            continue;

                        // Sprawdź czy obserwuje local player
                        if (targetPawn == localPlayerPawn)
                        {
                            // Pobierz nazwę gracza
                            string playerName = GetPlayerName(controller);
                            if (!string.IsNullOrEmpty(playerName) && playerName != "Unknown")
                            {
                                spectators.Add(playerName);
                                if (debugMode)
                                    Console.WriteLine($"[SpectatorList] Found spectator: {playerName}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (debugMode && i < 5)
                            Console.WriteLine($"[SpectatorList] Error entity {i}: {ex.Message}");
                        continue;
                    }
                }

                if (debugMode)
                    Console.WriteLine($"[SpectatorList] Total spectators: {spectators.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpectatorList] UpdateSpectators error: {ex.Message}");
            }
        }

        private static void UpdateSpectating()
        {
            spectatingTarget = "";
            spectatingMode = 0;
            isSpectating = false;

            try
            {
                if (GameState.client == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] client is zero (spectating)");
                    return;
                }

                // Pobierz local player pawn
                IntPtr localPlayerPawn = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerPawn);
                if (localPlayerPawn == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] localPlayerPawn is zero (spectating)");
                    return;
                }

                // Sprawdź czy local player jest martwy
                int localHealth = GameState.swed.ReadInt(localPlayerPawn + Offsets.m_iHealth);
                if (localHealth > 0)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] Local player is alive, not spectating");
                    return; // Żywi nie spectują
                }

                if (debugMode) Console.WriteLine("[SpectatorList] Local player is dead, checking spectating...");

                // Pobierz observer services local player
                IntPtr localObserverServices = GameState.swed.ReadPointer(localPlayerPawn + Offsets.m_pObserverServices);
                if (localObserverServices == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] localObserverServices is zero");
                    return;
                }

                // Pobierz observer target
                uint targetHandle = GameState.swed.ReadUInt(localObserverServices + Offsets.m_hObserverTarget);
                if (targetHandle == 0)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] targetHandle is zero");
                    return;
                }

                // Pobierz tryb obserwacji
                spectatingMode = GameState.swed.ReadInt(localObserverServices + Offsets.m_iObserverMode);
                if (debugMode) Console.WriteLine($"[SpectatorList] Observer mode: {spectatingMode}");

                // Resolve target pawn
                IntPtr targetPawn = ResolveEntityHandle(targetHandle);
                if (targetPawn == IntPtr.Zero)
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] targetPawn is zero");
                    return;
                }

                // Znajdź nazwę gracza którego obserwujemy
                spectatingTarget = FindPlayerNameByPawn(targetPawn);

                if (!string.IsNullOrEmpty(spectatingTarget) && spectatingTarget != "Unknown")
                {
                    isSpectating = true;
                    if (debugMode)
                        Console.WriteLine($"[SpectatorList] You are spectating: {spectatingTarget} (mode: {GetObserverModeString(spectatingMode)})");
                }
                else
                {
                    if (debugMode) Console.WriteLine("[SpectatorList] Could not find target name");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpectatorList] UpdateSpectating error: {ex.Message}");
            }
        }

        public static void Render()
        {
            if (!Enabled || GameState.renderer == null)
                return;

            try
            {
                // Jeśli obserwujesz kogoś - pokaż POMARAŃCZOWY napis "Spectating"
                if (isSpectating)
                {
                    RenderSpectating();
                }
                else // W przeciwnym razie pokaż normalną listę spectatorów (niebieską)
                {
                    RenderSpectators();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SpectatorList] Render error: {ex.Message}");
            }
        }

        private static void RenderSpectators()
        {
            var drawList = GameState.renderer.drawList;

            // Oblicz wysokość okna
            float windowHeight = HeaderHeight + Padding * 2;
            if (spectators.Count > 0)
            {
                windowHeight += spectators.Count * ItemHeight + Padding;
            }

            Vector2 windowPos = Position;
            Vector2 windowSize = new Vector2(Width, windowHeight);

            // Tło okna (niebieskie)
            drawList.AddRectFilled(windowPos, windowPos + windowSize,
                ImGui.ColorConvertFloat4ToU32(WindowBgColor), Rounding);

            // Obramowanie (niebieskie)
            drawList.AddRect(windowPos, windowPos + windowSize,
                ImGui.ColorConvertFloat4ToU32(BorderColor), Rounding, ImDrawFlags.None, 1.5f);

            // Nagłówek
            Vector2 headerPos = windowPos;
            Vector2 headerSize = new Vector2(Width, HeaderHeight);
            drawList.AddRectFilled(headerPos, headerPos + headerSize,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.12f, 0.12f, 0.15f, 0.9f)),
                Rounding, ImDrawFlags.RoundCornersTop);

            // Tekst nagłówka
            string headerText = $"Spectators ({spectators.Count})";
            Vector2 textSize = ImGui.CalcTextSize(headerText);
            Vector2 textPos = headerPos + new Vector2((Width - textSize.X) / 2, (HeaderHeight - textSize.Y) / 2);
            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(AccentColor), headerText);

            // Lista spectatorów
            if (spectators.Count > 0)
            {
                float contentY = headerPos.Y + HeaderHeight + Padding;

                for (int i = 0; i < spectators.Count; i++)
                {
                    float itemY = contentY + (i * ItemHeight);
                    string spectator = spectators[i];

                    // Tło itema
                    Vector2 itemPos = new Vector2(windowPos.X + Padding, itemY);
                    Vector2 itemSize = new Vector2(Width - Padding * 2, ItemHeight - 2);

                    drawList.AddRectFilled(itemPos, itemPos + itemSize,
                        ImGui.ColorConvertFloat4ToU32(new Vector4(0.1f, 0.1f, 0.12f, 0.6f)), 4f);

                    // Tekst
                    string displayName = spectator.Length > 20 ? spectator.Substring(0, 18) + "..." : spectator;
                    Vector2 namePos = itemPos + new Vector2(8, (ItemHeight - 16) / 2);
                    drawList.AddText(namePos, ImGui.ColorConvertFloat4ToU32(TextColor), displayName);
                }
            }
            else
            {
                // Komunikat gdy nie ma spectatorów
                string emptyText = "No spectators";
                Vector2 textSizeEmpty = ImGui.CalcTextSize(emptyText);
                Vector2 emptyPos = windowPos + new Vector2((Width - textSizeEmpty.X) / 2, HeaderHeight + Padding + 4);
                drawList.AddText(emptyPos, ImGui.ColorConvertFloat4ToU32(new Vector4(0.6f, 0.6f, 0.6f, 0.7f)), emptyText);
            }
        }

        private static void RenderSpectating()
        {
            var drawList = GameState.renderer.drawList;

            // Oblicz wysokość okna dla spectating
            float windowHeight = HeaderHeight + Padding * 2 + ItemHeight;

            // Użyj tej samej pozycji co spectators
            Vector2 windowPos = Position;
            Vector2 windowSize = new Vector2(Width, windowHeight);

            // Tło okna (pomarańczowe)
            drawList.AddRectFilled(windowPos, windowPos + windowSize,
                ImGui.ColorConvertFloat4ToU32(SpectatingWindowBgColor), Rounding);

            // Obramowanie (pomarańczowe)
            drawList.AddRect(windowPos, windowPos + windowSize,
                ImGui.ColorConvertFloat4ToU32(SpectatingBorderColor), Rounding, ImDrawFlags.None, 1.5f);

            // Nagłówek (pomarańczowy)
            Vector2 headerPos = windowPos;
            Vector2 headerSize = new Vector2(Width, HeaderHeight);
            drawList.AddRectFilled(headerPos, headerPos + headerSize,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.20f, 0.13f, 0.05f, 0.9f)),
                Rounding, ImDrawFlags.RoundCornersTop);

            // Tekst nagłówka - "SPECTATING" dużymi literami
            string headerText = "SPECTATING";
            Vector2 textSize = ImGui.CalcTextSize(headerText);
            Vector2 textPos = headerPos + new Vector2((Width - textSize.X) / 2, (HeaderHeight - textSize.Y) / 2);
            drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(SpectatingAccentColor), headerText);

            // Informacja o obserwowanym graczu
            float contentY = headerPos.Y + HeaderHeight + Padding;
            Vector2 itemPos = new Vector2(windowPos.X + Padding, contentY);
            Vector2 itemSize = new Vector2(Width - Padding * 2, ItemHeight - 2);

            drawList.AddRectFilled(itemPos, itemPos + itemSize,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0.12f, 0.08f, 0.04f, 0.6f)), 4f);

            // Tekst - kto jest obserwowany
            string modeStr = GetObserverModeString(spectatingMode);
            string displayText = $"{spectatingTarget}";

            // Jeśli target jest nieznany, pokaż tylko tryb
            if (spectatingTarget == "Unknown" || string.IsNullOrEmpty(spectatingTarget))
                displayText = $"({modeStr})";
            else
                displayText = $"{spectatingTarget} ({modeStr})";

            // Skróć jeśli za długie
            if (displayText.Length > 25)
                displayText = displayText.Substring(0, 22) + "...";

            Vector2 namePos = itemPos + new Vector2(8, (ItemHeight - 16) / 2);
            drawList.AddText(namePos, ImGui.ColorConvertFloat4ToU32(TextColor), displayText);
        }

        // Pomocnicze metody
        private static IntPtr ResolveEntityHandle(uint handle)
        {
            try
            {
                if (handle == 0)
                    return IntPtr.Zero;

                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                    return IntPtr.Zero;

                // POPRAWNA formuła dla CS2
                long listEntryOffset = 0x8 * ((handle & 0x7FFF) >> 9) + 0x10;
                IntPtr listEntry = GameState.swed.ReadPointer(entityList + (int)listEntryOffset);
                if (listEntry == IntPtr.Zero)
                    return IntPtr.Zero;

                // POPRAWNA formuła dla CS2
                long entityOffset = 0x78 * (handle & 0x1FF);
                return GameState.swed.ReadPointer(listEntry + (int)entityOffset);
            }
            catch
            {
                return IntPtr.Zero;
            }
        }

        private static string GetPlayerName(IntPtr controller)
        {
            try
            {
                if (controller == IntPtr.Zero)
                    return "Unknown";

                // Spróbuj najpierw sanitized name
                IntPtr sanitizedNameAddr = controller + Offsets.m_sSanitizedPlayerName;
                if (sanitizedNameAddr != IntPtr.Zero)
                {
                    byte[] nameBuffer = GameState.swed.ReadBytes(sanitizedNameAddr, 32);
                    string sanitizedName = System.Text.Encoding.UTF8.GetString(nameBuffer).TrimEnd('\0');
                    if (!string.IsNullOrEmpty(sanitizedName))
                        return sanitizedName;
                }

                // Fallback na normalną nazwę
                IntPtr nameAddr = controller + Offsets.m_iszPlayerName;
                byte[] buffer = GameState.swed.ReadBytes(nameAddr, 32);
                string name = System.Text.Encoding.UTF8.GetString(buffer).TrimEnd('\0');

                return string.IsNullOrEmpty(name) ? "Unknown" : name;
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string FindPlayerNameByPawn(IntPtr targetPawn)
        {
            try
            {
                if (targetPawn == IntPtr.Zero)
                    return "Unknown";

                // Przeszukaj entity list
                IntPtr entityList = GameState.swed.ReadPointer(GameState.client + Offsets.dwEntityList);
                if (entityList == IntPtr.Zero)
                    return "Unknown";

                IntPtr listEntry = GameState.swed.ReadPointer(entityList + 0x10);
                if (listEntry == IntPtr.Zero)
                    return "Unknown";

                for (int i = 0; i < 64; i++)
                {
                    IntPtr controller = GameState.swed.ReadPointer(listEntry + (i * 0x78));
                    if (controller == IntPtr.Zero)
                        continue;

                    uint pawnHandle = GameState.swed.ReadUInt(controller + Offsets.m_hPawn);
                    if (pawnHandle == 0)
                        continue;

                    IntPtr pawn = ResolveEntityHandle(pawnHandle);
                    if (pawn == targetPawn)
                    {
                        return GetPlayerName(controller);
                    }
                }

                return "Unknown";
            }
            catch
            {
                return "Unknown";
            }
        }

        private static string GetObserverModeString(int mode)
        {
            return mode switch
            {
                1 => "DeathCam",
                2 => "FreezeCam",
                3 => "Fixed",
                4 => "First Person",
                5 => "Third Person",
                6 => "FreeRoam",
                _ => $"Mode {mode}"
            };
        }

        // Metoda do sprawdzenia czy entity jest obserwowane przez local playera
        public static bool IsEntityBeingSpectated(Entity entity)
        {
            if (!isSpectating || entity == null)
                return false;

            // Porównaj nazwę
            return entity.Name == spectatingTarget;
        }
    }
}