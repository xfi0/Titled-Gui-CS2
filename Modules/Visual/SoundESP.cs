using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SoundESP
    {
        public static bool enabled = false;
        public static Vector4 color = new(1f, 0.6f, 0.2f, 1f);

        public static float minRadius = 3f;
        public static float maxRadius = 25f;
        public static float lifeTime = 1.6f;
        public static float fadeStart = 1.2f;
        public static float expandSpeed = 40f;

        // Śledzenie emitSoundTime dla każdego gracza
        private static Dictionary<IntPtr, float> lastEmitSoundTime = new();

        // Lista aktywnych kółek dźwiękowych
        private static List<SoundCircle> circles = new();

        private class SoundCircle
        {
            public Vector3 position;
            public float startTime;
            public float radius;
            public float alpha;
            public IntPtr owner;
        }

        public static void UpdateSoundESP()
        {
            if (!enabled)
            {
                Cleanup();
                return;
            }

            float now = (float)ImGui.GetTime();
            float dt = ImGui.GetIO().DeltaTime;

            // 1. USUWANIE STARYCH KÓŁEK
            circles.RemoveAll(c => (now - c.startTime) >= lifeTime);

            // 2. AKTUALIZACJA ISTNIEJĄCYCH KÓŁEK
            for (int i = 0; i < circles.Count; i++)
            {
                var c = circles[i];
                float age = now - c.startTime;

                // Rozszerzanie
                c.radius = MathF.Min(c.radius + expandSpeed * dt, maxRadius);

                // Zanikanie
                if (age >= fadeStart)
                {
                    float t = (age - fadeStart) / (lifeTime - fadeStart);
                    c.alpha = MathF.Max(0f, 1f - t);
                }

                circles[i] = c;
            }

            // 3. WYKRYWANIE NOWYCH DŹWIĘKÓW (na podstawie emitSoundTime)
            DetectNewSounds(now);
        }

        private static void DetectNewSounds(float now)
        {
            foreach (var e in GameState.Entities)
            {
                if (e == null || e.Health <= 0 || e.PawnAddress == IntPtr.Zero)
                {
                    // Usuń martwego gracza ze śledzenia
                    if (lastEmitSoundTime.ContainsKey(e.PawnAddress))
                        lastEmitSoundTime.Remove(e.PawnAddress);
                    continue;
                }

                float currentEmitTime = e.emitSoundTime;

                // Inicjalizacja dla nowego gracza
                if (!lastEmitSoundTime.TryGetValue(e.PawnAddress, out float lastTime))
                {
                    lastEmitSoundTime[e.PawnAddress] = currentEmitTime;
                    continue;
                }

                // Sprawdź czy emitSoundTime się zmienił (nowy dźwięk)
                // Używamy tolerancji 0.15s aby uniknąć wielokrotnych wywołań
                if (MathF.Abs(currentEmitTime - lastTime) > 0.15f && currentEmitTime > lastTime)
                {
                    // Aktualizuj czas
                    lastEmitSoundTime[e.PawnAddress] = currentEmitTime;

                    // Dodaj nowe kółko dźwiękowe
                    circles.Add(new SoundCircle
                    {
                        position = GetFootPosition(e),
                        startTime = now,
                        radius = minRadius,
                        alpha = 1f,
                        owner = e.PawnAddress
                    });
                }
            }
        }

        public static void DrawSoundESP(Entity e)
        {
            if (!enabled || e == null || e.Health <= 0 || e.PawnAddress == IntPtr.Zero)
                return;

            float[] vm = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            // Rysuj tylko kółka należące do tego gracza
            foreach (var c in circles)
            {
                if (c.owner != e.PawnAddress || c.alpha <= 0.02f)
                    continue;

                DrawCircleGround(c.position, c.radius, c.alpha, vm);
            }
        }

        private static void DrawCircleGround(Vector3 center, float radius, float alpha, float[] vm)
        {
            const int segments = 24;
            Vector2[] points = new Vector2[segments];
            int validPoints = 0;

            for (int i = 0; i < segments; i++)
            {
                float angle = i * MathF.PI * 2f / segments;
                Vector3 worldPos = new Vector3(
                    center.X + MathF.Cos(angle) * radius,
                    center.Y + MathF.Sin(angle) * radius,
                    center.Z
                );

                Vector2 screenPos = Calculate.WorldToScreen(vm, worldPos, GameState.renderer.screenSize);

                // Sprawdź czy punkt jest na ekranie
                if (screenPos.X > 0 && screenPos.Y > 0 &&
                    screenPos.X < GameState.renderer.screenSize.X &&
                    screenPos.Y < GameState.renderer.screenSize.Y)
                {
                    points[validPoints++] = screenPos;
                }
            }

            if (validPoints < 3)
                return;

            // Kolor wypełnienia (przezroczysty)
            Vector4 fillColor = new Vector4(color.X, color.Y, color.Z, alpha * 0.15f);

            // Kolor obramowania
            Vector4 outlineColor = new Vector4(color.X, color.Y, color.Z, alpha);

            // Rysuj wypełnienie
            GameState.renderer.drawList.AddConvexPolyFilled(
                ref points[0],
                validPoints,
                ImGui.ColorConvertFloat4ToU32(fillColor)
            );

            // Rysuj obramowanie
            uint outlineU32 = ImGui.ColorConvertFloat4ToU32(outlineColor);
            for (int i = 0; i < validPoints; i++)
            {
                int next = (i + 1) % validPoints;
                GameState.renderer.drawList.AddLine(
                    points[i],
                    points[next],
                    outlineU32,
                    1.5f
                );
            }
        }

        private static Vector3 GetFootPosition(Entity e)
        {
            // Próbuj uzyskać pozycję między stopami
            if (e.Bones != null && e.Bones.Count >= 28)
            {
                Vector3 leftFoot = e.Bones[24];  // Bone 24: left foot
                Vector3 rightFoot = e.Bones[27]; // Bone 27: right foot

                if (leftFoot != Vector3.Zero && rightFoot != Vector3.Zero)
                    return (leftFoot + rightFoot) * 0.5f;
            }

            // Fallback na pozycję gracza
            return e.Position;
        }

        public static void Cleanup()
        {
            circles.Clear();
            lastEmitSoundTime.Clear();
        }
    }
}