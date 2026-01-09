
using System.Numerics;
using ImGuiNET;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class ArrowESP
    {
        public static bool Enabled = false;
        public static bool ShowTeam = false;
        public static float ArrowSize = 20f;
        public static float ArrowDistance = 80f; // Odległość od środka ekranu
        public static float ArrowThickness = 2f;
        public static Vector4 EnemyArrowColor = new(1f, 0f, 0f, 1f); // Czerwony dla wrogów
        public static Vector4 TeamArrowColor = new(0f, 1f, 0f, 1f); // Zielony dla sojuszników
        public static bool ShowDistanceText = true;
        public static bool AlwaysShowDistance = false; // ZAWSZE pokazuj dystans, nie tylko gdy blisko
        public static float MaxArrowDistance = 100000f; // Maksymalna odległość dla pokazania strzałki
        public static bool FadeByDistance = true;
        public static bool ShowOutOfView = true; // Pokazuj strzałki dla graczy poza ekranem
        public static bool ShowName = false;
        public static bool EnableGlow = false; // Włącz efekt glow
        public static float GlowAmount = 5f; // Siła efektu glow
        public static Vector4 GlowColor = new(1f, 1f, 1f, 0.5f); // Kolor glow (biały przezroczysty)
        public static bool ShowHealthBar = false; // Pokazuj pasek zdrowia na strzałce
        public static float HealthBarHeight = 3f;
        public static bool ShowWeaponIcon = false; // Pokazuj ikonę broni
        public static float MinDistanceToShow = 0f; // Minimalna odległość do pokazania strzałki
        public static bool UseDynamicSize = true; // Dynamiczny rozmiar strzałki w zależności od odległości
        public static float MinArrowSize = 15f;
        public static float MaxArrowSize = 30f;

        public static void Render()
        {
            if (!Enabled || GameState.LocalPlayer == null || GameState.Entities == null)
                return;

            try
            {
                Vector2 screenCenter = new(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);

                foreach (var entity in GameState.Entities)
                {
                    // Pomijaj lokalnego gracza
                    if (entity.PawnAddress == GameState.LocalPlayer.PawnAddress)
                        continue;

                    // Pomijaj martwych
                    if (entity.Health <= 0 || entity.LifeState != 256)
                        continue;

                    // Pomijaj sojuszników jeśli wyłączone
                    if (!ShowTeam && entity.Team == GameState.LocalPlayer.Team)
                        continue;

                    // Sprawdź czy pozycja jest poza ekranem lub ShowOutOfView jest włączone
                    bool isOnScreen = entity.Position2D.X >= 0 && entity.Position2D.X <= GameState.renderer.screenSize.X &&
                                      entity.Position2D.Y >= 0 && entity.Position2D.Y <= GameState.renderer.screenSize.Y;

                    if (!ShowOutOfView && !isOnScreen)
                        continue;

                    // Oblicz dystans 3D
                    float distance = Vector3.Distance(GameState.LocalPlayer.Position, entity.Position);

                    // Sprawdź minimalny i maksymalny dystans
                    if (distance < MinDistanceToShow || distance > MaxArrowDistance)
                        continue;

                    Vector2 entityScreenPos = entity.Position2D;

                    // Jeśli pozycja jest poza ekranem, oblicz punkt na krawędzi ekranu
                    if (!isOnScreen)
                    {
                        entityScreenPos = GetEdgePoint(screenCenter, entity.Position2D);
                    }

                    // Oblicz kierunek od środka ekranu do gracza
                    Vector2 direction = entityScreenPos - screenCenter;

                    // Jeśli pozycja jest na ekranie, ale chcemy i tak pokazać strzałkę
                    if (isOnScreen && ShowOutOfView)
                    {
                        // Normalizuj kierunek i ustaw na zadaną odległość
                        if (direction.Length() > 0)
                        {
                            direction = Vector2.Normalize(direction) * ArrowDistance;
                        }
                    }
                    else if (!isOnScreen)
                    {
                        // Normalizuj i ogranicz do odległości strzałki
                        if (direction.Length() > 0)
                        {
                            direction = Vector2.Normalize(direction) * ArrowDistance;
                        }
                    }

                    // Oblicz pozycję strzałki
                    Vector2 arrowPos = screenCenter + direction;

                    // Oblicz dynamiczny rozmiar strzałki
                    float dynamicArrowSize = ArrowSize;
                    if (UseDynamicSize)
                    {
                        float distanceFactor = Math.Clamp(1.0f - (distance / MaxArrowDistance), 0.1f, 1.0f);
                        dynamicArrowSize = MinArrowSize + (MaxArrowSize - MinArrowSize) * distanceFactor;
                    }

                    // Oblicz przezroczystość na podstawie dystansu
                    float alpha = FadeByDistance ? Math.Clamp(1.0f - (distance / MaxArrowDistance), 0.3f, 1.0f) : 1.0f;

                    // Wybierz kolor
                    Vector4 arrowColor = entity.Team == GameState.LocalPlayer.Team ? TeamArrowColor : EnemyArrowColor;
                    arrowColor.W = alpha;

                    // Narysuj glow jeśli włączone
                    if (EnableGlow && GlowAmount > 0)
                    {
                        DrawArrowGlow(arrowPos, direction, arrowColor, dynamicArrowSize, distance);
                    }

                    // Narysuj strzałkę
                    DrawArrow(arrowPos, direction, arrowColor, entity.Name, distance, entity.Health, dynamicArrowSize);

                    // Jeśli pozycja jest na ekranie, możesz dodać dodatkowy wskaźnik
                    if (isOnScreen && ShowOutOfView)
                    {
                        DrawOnScreenIndicator(entityScreenPos, arrowColor);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ArrowESP Error: {ex.Message}");
            }
        }

        private static Vector2 GetEdgePoint(Vector2 screenCenter, Vector2 targetPos)
        {
            Vector2 direction = targetPos - screenCenter;

            // Jeśli punkt jest już w granicach ekranu, zwróć go
            if (targetPos.X >= 0 && targetPos.X <= GameState.renderer.screenSize.X &&
                targetPos.Y >= 0 && targetPos.Y <= GameState.renderer.screenSize.Y)
            {
                return targetPos;
            }

            // Oblicz punkt przecięcia z krawędzią ekranu
            float slope = direction.Y / direction.X;

            // Sprawdź przecięcie z lewą/prawą krawędzią
            float x, y;

            if (direction.X > 0) // Prawa krawędź
            {
                x = GameState.renderer.screenSize.X;
                y = screenCenter.Y + slope * (x - screenCenter.X);

                if (y >= 0 && y <= GameState.renderer.screenSize.Y)
                    return new Vector2(x, y);
            }
            else // Lewa krawędź
            {
                x = 0;
                y = screenCenter.Y + slope * (x - screenCenter.X);

                if (y >= 0 && y <= GameState.renderer.screenSize.Y)
                    return new Vector2(x, y);
            }

            // Sprawdź przecięcie z górną/dolną krawędzią
            float invSlope = direction.X / direction.Y;

            if (direction.Y > 0) // Dolna krawędź
            {
                y = GameState.renderer.screenSize.Y;
                x = screenCenter.X + invSlope * (y - screenCenter.Y);
            }
            else // Górna krawędź
            {
                y = 0;
                x = screenCenter.X + invSlope * (y - screenCenter.Y);
            }

            return new Vector2(x, y);
        }

        private static void DrawArrow(Vector2 position, Vector2 direction, Vector4 color, string playerName,
                                     float distance, int health = 100, float arrowSize = 20f)
        {
            if (direction.Length() == 0)
                return;

            // Normalizuj kierunek
            Vector2 dirNormalized = Vector2.Normalize(direction);

            // Oblicz kąt obrotu
            float angle = MathF.Atan2(dirNormalized.Y, dirNormalized.X);

            // Punkty strzałki (trójkąt skierowany w stronę gracza)
            Vector2 arrowTip = position;
            Vector2 arrowLeft = position - new Vector2(
                MathF.Cos(angle + MathF.PI / 6) * arrowSize,
                MathF.Sin(angle + MathF.PI / 6) * arrowSize
            );
            Vector2 arrowRight = position - new Vector2(
                MathF.Cos(angle - MathF.PI / 6) * arrowSize,
                MathF.Sin(angle - MathF.PI / 6) * arrowSize
            );

            uint col = ImGui.ColorConvertFloat4ToU32(color);

            // Narysuj trójkąt strzałki
            GameState.renderer.drawList.AddTriangleFilled(arrowTip, arrowLeft, arrowRight, col);

            // Opcjonalnie: dodaj obramówkę
            GameState.renderer.drawList.AddTriangle(arrowTip, arrowLeft, arrowRight,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, color.W)), ArrowThickness);

            // Dodaj pasek zdrowia jeśli włączone
            if (ShowHealthBar && health > 0)
            {
                DrawHealthBar(position, dirNormalized, arrowSize, health, color);
            }

            // Sprawdź czy pokazywać dystans
            bool shouldShowDistance = ShowDistanceText && (AlwaysShowDistance || distance < 300f);

            if (shouldShowDistance)
            {
                Vector2 textPos = position - dirNormalized * (arrowSize + (ShowHealthBar ? 20f : 10f));
                string distanceText = $"{distance:F0}m";

                // Oblicz rozmiar tekstu
                Vector2 textSize = ImGui.CalcTextSize(distanceText);
                textPos -= textSize * 0.5f;

                // Dodaj tło pod tekstem
                GameState.renderer.drawList.AddRectFilled(
                    textPos - new Vector2(2, 2),
                    textPos + textSize + new Vector2(2, 2),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, color.W * 0.7f))
                );

                GameState.renderer.drawList.AddText(textPos, col, distanceText);
            }

            // Dodaj nazwę gracza jeśli włączone
            if (ShowName && !string.IsNullOrEmpty(playerName))
            {
                Vector2 namePos = position - dirNormalized * (arrowSize +
                    (shouldShowDistance ? 40f : 10f) +
                    (ShowHealthBar ? 10f : 0f));

                // Skróć nazwę jeśli za długa
                string displayName = playerName.Length > 12 ? playerName.Substring(0, 12) + "..." : playerName;

                Vector2 nameSize = ImGui.CalcTextSize(displayName);
                namePos -= nameSize * 0.5f;

                // Dodaj tło pod nazwą
                GameState.renderer.drawList.AddRectFilled(
                    namePos - new Vector2(2, 2),
                    namePos + nameSize + new Vector2(2, 2),
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, color.W * 0.7f))
                );

                GameState.renderer.drawList.AddText(namePos, col, displayName);
            }
        }

        private static void DrawArrowGlow(Vector2 position, Vector2 direction, Vector4 color,
                                         float arrowSize, float distance)
        {
            if (direction.Length() == 0)
                return;

            Vector2 dirNormalized = Vector2.Normalize(direction);
            float angle = MathF.Atan2(dirNormalized.Y, dirNormalized.X);

            // Wiele warstw glow z różną przezroczystością
            for (int i = 0; i < 3; i++)
            {
                float glowSize = arrowSize + GlowAmount * (i + 1);
                float glowAlpha = GlowColor.W * (1.0f - (i * 0.3f)) * color.W;

                Vector4 glowLayerColor = new(GlowColor.X, GlowColor.Y, GlowColor.Z, glowAlpha);

                Vector2 arrowTip = position;
                Vector2 arrowLeft = position - new Vector2(
                    MathF.Cos(angle + MathF.PI / 6) * glowSize,
                    MathF.Sin(angle + MathF.PI / 6) * glowSize
                );
                Vector2 arrowRight = position - new Vector2(
                    MathF.Cos(angle - MathF.PI / 6) * glowSize,
                    MathF.Sin(angle - MathF.PI / 6) * glowSize
                );

                uint glowCol = ImGui.ColorConvertFloat4ToU32(glowLayerColor);
                GameState.renderer.drawList.AddTriangle(arrowTip, arrowLeft, arrowRight, glowCol, 1.5f);
            }
        }

        private static void DrawHealthBar(Vector2 position, Vector2 direction, float arrowSize, int health, Vector4 color)
        {
            float healthPercent = Math.Clamp(health / 100f, 0f, 1f);
            Vector2 barStart = position - direction * (arrowSize + 5f);
            Vector2 barEnd = barStart - direction * 20f;

            float barLength = 20f;
            float filledLength = barLength * healthPercent;

            // Tło paska zdrowia
            GameState.renderer.drawList.AddLine(
                barStart,
                barEnd,
                ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, color.W * 0.7f)),
                HealthBarHeight + 2f
            );

            // Wypełniony pasek zdrowia
            Vector2 filledEnd = barStart - direction * filledLength;

            // Kolor paska w zależności od zdrowia
            Vector4 healthColor = healthPercent > 0.5f ?
                new Vector4(0, 1, 0, color.W) : // Zielony >50%
                healthPercent > 0.25f ?
                new Vector4(1, 1, 0, color.W) : // Żółty 25-50%
                new Vector4(1, 0, 0, color.W);  // Czerwony <25%

            GameState.renderer.drawList.AddLine(
                barStart,
                filledEnd,
                ImGui.ColorConvertFloat4ToU32(healthColor),
                HealthBarHeight
            );

            // Dodaj tekst z wartością zdrowia
            if (healthPercent < 0.8f) // Pokazuj tylko jeśli nie pełne zdrowie
            {
                string healthText = $"{health}";
                Vector2 textPos = barStart - direction * (filledLength / 2f);
                Vector2 textSize = ImGui.CalcTextSize(healthText);
                textPos -= textSize * 0.5f;

                GameState.renderer.drawList.AddText(
                    textPos,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(1, 1, 1, color.W)),
                    healthText
                );
            }
        }

        private static void DrawOnScreenIndicator(Vector2 position, Vector4 color)
        {
            // Mały znaczek wskazujący gracza na ekranie
            float indicatorSize = 8f;
            uint col = ImGui.ColorConvertFloat4ToU32(color);

            // Narysuj krzyżyk
            GameState.renderer.drawList.AddLine(
                position - new Vector2(indicatorSize, 0),
                position + new Vector2(indicatorSize, 0),
                col,
                1.5f
            );

            GameState.renderer.drawList.AddLine(
                position - new Vector2(0, indicatorSize),
                position + new Vector2(0, indicatorSize),
                col,
                1.5f
            );

            // Okrąg wokół
            GameState.renderer.drawList.AddCircle(
                position,
                indicatorSize * 1.2f,
                col,
                12,
                1f
            );
        }

        public static void RenderPreview(Vector2 center)
        {
            if (!Enabled) return;

            // Pokaż przykładowe strzałki w podglądzie
            Vector4[] previewColors = [EnemyArrowColor, TeamArrowColor];
            string[] directions = ["Prawo", "Dół", "Lewo", "Góra"];
            int[] healthValues = [100, 75, 50, 25];

            for (int i = 0; i < 4; i++)
            {
                Vector2 direction = i switch
                {
                    0 => new Vector2(1, 0),    // Prawo
                    1 => new Vector2(0, 1),    // Dół
                    2 => new Vector2(-1, 0),   // Lewo
                    3 => new Vector2(0, -1),   // Góra
                    _ => new Vector2(1, 0)
                };

                Vector2 arrowPos = center + direction * ArrowDistance;
                Vector4 color = previewColors[i % 2];

                float arrowSize = UseDynamicSize ?
                    MinArrowSize + (MaxArrowSize - MinArrowSize) * (0.2f + i * 0.2f) :
                    ArrowSize;

                if (EnableGlow)
                {
                    DrawArrowGlow(arrowPos, direction, color, arrowSize, 50f + i * 20f);
                }

                DrawArrow(arrowPos, direction, color, $"Player{i + 1}", 50f + i * 20f, healthValues[i], arrowSize);
            }
        }
    }
}
