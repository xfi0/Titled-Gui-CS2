using System.Numerics;
using ImGuiNET;
using System.Collections.Generic;
using System;
using Titled_Gui.Data.Game;
using Titled_Gui.Classes;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Visual
{
    public class BulletTrailEffect
    {
        public Vector3 StartPosition { get; set; }
        public Vector3 EndPosition { get; set; }
        public Vector4 StartColor { get; set; }
        public Vector4 EndColor { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public float Thickness { get; set; }
        public float Opacity { get; set; }
        public float CreationTime { get; set; }
        public bool IsRealTrajectory { get; set; }
        public Vector4 DynamicStartColor;
        public Vector4 DynamicEndColor;

        public BulletTrailEffect(Vector3 start, Vector3 end, Vector4 startColor, Vector4 endColor,
                               float lifetime = 1.2f, float thickness = 2.5f, float opacity = 0.9f,
                               bool isRealTrajectory = false)
        {
            StartPosition = start;
            EndPosition = end;
            StartColor = startColor;
            EndColor = endColor;
            DynamicStartColor = startColor;
            DynamicEndColor = endColor;
            MaxLifetime = lifetime;
            Lifetime = lifetime;
            Thickness = thickness;
            Opacity = opacity;
            CreationTime = BulletTrailManager.GetCurrentTime();
            IsRealTrajectory = isRealTrajectory;
        }

        public void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
        }

        public bool IsExpired => Lifetime <= 0;

        public float GetAlpha()
        {
            if (Lifetime > MaxLifetime * 0.3f)
                return 1.0f;

            float fadeProgress = 1.0f - (Lifetime / (MaxLifetime * 0.3f));
            return Math.Clamp(1.0f - fadeProgress, 0f, 1f);
        }

        public Vector4 GetColorAtPosition(float t)
        {
            return Vector4.Lerp(DynamicStartColor, DynamicEndColor, t);
        }

        public Vector3 GetPointOnTrail(float t)
        {
            return Vector3.Lerp(StartPosition, EndPosition, t);
        }

        public bool IsVisibleFromCamera(Vector3 cameraPos, Vector3 cameraForward)
        {
            Vector3 trailDir = EndPosition - StartPosition;
            Vector3 toStart = StartPosition - cameraPos;
            Vector3 toEnd = EndPosition - cameraPos;

            if (toStart.LengthSquared() > 0.01f && toEnd.LengthSquared() > 0.01f)
            {
                toStart = Vector3.Normalize(toStart);
                toEnd = Vector3.Normalize(toEnd);

                return Vector3.Dot(toStart, cameraForward) > 0.1f ||
                       Vector3.Dot(toEnd, cameraForward) > 0.1f;
            }

            return true;
        }
    }

    public static class BulletTrailManager
    {
        private static List<BulletTrailEffect> activeTrails = new List<BulletTrailEffect>();
        private static float lastUpdateTime = 0;
        private static int lastShotsFired = 0;
        private static float lastShotTime = 0f;
        private const float SHOT_COOLDOWN = 0.05f;
        private static bool forceTrailNextShot = false;
        private static bool isShooting = false;
        private static float timeSinceLastShot = 0f;

        // Cooldown zapobiegający duplikowaniu trailów
        private static float lastTrailAddedTime = 0f;
        private const float TRAIL_COOLDOWN = 0.1f; // 100ms

        // Dodatkowe śledzenie strzałów z innych modułów
        private static float lastExternalShotDetectionTime = 0f;
        private const float EXTERNAL_SHOT_COOLDOWN = 0.15f;

        // Konfiguracja
        public static bool EnableBulletTrails = false;
        public static Vector4 StartColor = new Vector4(1.0f, 0.9f, 0.3f, 0.95f);
        public static Vector4 EndColor = new Vector4(1.0f, 0.2f, 0.0f, 0.7f);
        public static float TrailLifetime = 1.5f;
        public static float TrailThickness = 2.5f;
        public static float TrailOpacity = 0.85f;
        public static int MaxTrails = 30;
        public static bool ShowGlowEffect = true;
        public static float TrailLength = 1500f;
        public static bool UseRealBulletImpact = true;
        public static float SparkSize = 3.0f;
        public static float GlowIntensity = 0.35f;
        public static int TrailSegments = 16;
        public static bool AlwaysShowTrail = true;
        public static Vector4 SparkStartColor = new Vector4(1.0f, 0.95f, 0.6f, 0.9f);
        public static Vector4 SparkEndColor = new Vector4(1.0f, 0.4f, 0.1f, 0.95f);
        public static Vector4 RealTrailSparkStartColor = new Vector4(0.0f, 1.0f, 0.3f, 0.9f);
        public static Vector4 RealTrailSparkEndColor = new Vector4(1.0f, 0.0f, 0.0f, 0.95f);

        public static float GetCurrentTime()
        {
            try
            {
                return GlobalVar.GetRealTime();
            }
            catch
            {
                return (float)Environment.TickCount / 1000f;
            }
        }

        // Publiczna metoda do zgłaszania strzałów z innych modułów
        public static void OnShootDetected()
        {
            float currentTime = GetCurrentTime();
            if (currentTime - lastExternalShotDetectionTime > EXTERNAL_SHOT_COOLDOWN)
            {
                forceTrailNextShot = true;
                lastExternalShotDetectionTime = currentTime;
            }
        }

        public static void AddTrail(Vector3 start, Vector3 end, bool isRealTrajectory = false)
        {
            if (!EnableBulletTrails)
                return;

            // Sprawdź cooldown - zapobiegaj duplikowaniu trailów
            float currentTime = GetCurrentTime();
            if (currentTime - lastTrailAddedTime < TRAIL_COOLDOWN)
                return;

            lastTrailAddedTime = currentTime;

            Vector4 actualStartColor = StartColor;
            Vector4 actualEndColor = EndColor;

            actualStartColor.W = 0.95f;
            actualEndColor.W = 0.7f;

            BulletTrailEffect trail = new BulletTrailEffect(
                start, end,
                actualStartColor, actualEndColor,
                TrailLifetime, TrailThickness, TrailOpacity,
                isRealTrajectory
            );

            if (activeTrails.Count >= MaxTrails)
            {
                int oldestIndex = 0;
                float oldestTime = float.MaxValue;

                for (int i = 0; i < activeTrails.Count; i++)
                {
                    if (activeTrails[i].CreationTime < oldestTime)
                    {
                        oldestTime = activeTrails[i].CreationTime;
                        oldestIndex = i;
                    }
                }

                if (oldestIndex < activeTrails.Count)
                {
                    activeTrails.RemoveAt(oldestIndex);
                }
            }

            activeTrails.Add(trail);

            // Debug
            // Console.WriteLine($"Added trail at {currentTime}");
        }

        public static void ForceNextTrail()
        {
            float currentTime = GetCurrentTime();
            if (currentTime - lastTrailAddedTime > TRAIL_COOLDOWN)
            {
                forceTrailNextShot = true;
                lastExternalShotDetectionTime = currentTime;
            }
        }

        public static void Update()
        {
            if (!EnableBulletTrails) return;

            try
            {
                if (GameState.swed == null || GameState.client == IntPtr.Zero)
                    return;

                if (GameState.LocalPlayer == null || GameState.LocalPlayer.Health <= 0)
                {
                    ClearAll();
                    return;
                }

                float currentTime = GetCurrentTime();
                float deltaTime = Math.Max(0.001f, currentTime - lastUpdateTime);
                deltaTime = Math.Min(deltaTime, 0.1f);

                // 1. Sprawdź wymuszone traile (z Aimbot/TriggerBot)
                if (forceTrailNextShot && (currentTime - lastShotTime) > SHOT_COOLDOWN)
                {
                    AddTrailFromCurrentPosition();
                    lastShotTime = currentTime;
                    forceTrailNextShot = false;
                }
                // 2. Normalne wykrywanie strzałów
                else
                {
                    bool shotDetected = CheckIfShooting(currentTime);
                    if (shotDetected && (currentTime - lastShotTime) > SHOT_COOLDOWN)
                    {
                        AddTrailFromCurrentPosition();
                        lastShotTime = currentTime;
                    }
                }

                // Aktualizacja istniejących trailów
                for (int i = activeTrails.Count - 1; i >= 0; i--)
                {
                    activeTrails[i].Update(deltaTime);
                    activeTrails[i].DynamicStartColor = StartColor;
                    activeTrails[i].DynamicEndColor = EndColor;
                    activeTrails[i].DynamicStartColor.W = activeTrails[i].StartColor.W;
                    activeTrails[i].DynamicEndColor.W = activeTrails[i].EndColor.W;

                    if (activeTrails[i].IsExpired)
                    {
                        activeTrails.RemoveAt(i);
                    }
                }

                lastUpdateTime = currentTime;
            }
            catch (Exception ex)
            {
                // Logowanie błędów dla debugowania
                // Console.WriteLine($"BulletTrailManager.Update error: {ex.Message}");
            }
        }

        private static void AddTrailFromCurrentPosition()
        {
            try
            {
                Vector3 eyePosition = GetEyePosition();
                Vector3 forward = GetForwardDirection();

                if (UseRealBulletImpact)
                {
                    Vector3? realImpact = FindRealBulletImpact(eyePosition);

                    if (realImpact.HasValue)
                    {
                        AddTrail(eyePosition, realImpact.Value, true);
                    }
                    else
                    {
                        Vector3 endPosition = eyePosition + (forward * TrailLength);
                        AddTrail(eyePosition, endPosition, false);
                    }
                }
                else
                {
                    Vector3 endPosition = eyePosition + (forward * TrailLength);
                    AddTrail(eyePosition, endPosition, false);
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"AddTrailFromCurrentPosition error: {ex.Message}");
            }
        }

        private static Vector3? FindRealBulletImpact(Vector3 startPosition)
        {
            try
            {
                Vector3 forward = GetForwardDirection();
                Vector3 endPosition = startPosition + (forward * TrailLength);
                return endPosition;
            }
            catch
            {
                return null;
            }
        }

        private static bool CheckIfShooting(float currentTime)
        {
            try
            {
                if (GameState.LocalPlayer == null || GameState.LocalPlayer.PawnAddress == IntPtr.Zero)
                    return false;

                // Pobierz liczbę strzałów bezpośrednio z pamięci
                int currentShotsFired = GameState.swed.ReadInt(GameState.LocalPlayer.PawnAddress, Offsets.m_iShotsFired);

                // Wykrywamy tylko faktyczny przyrost strzałów
                if (currentShotsFired > lastShotsFired)
                {
                    // Zapobiegaj wielokrotnemu wykryciu tego samego strzału
                    if ((currentTime - lastShotTime) > SHOT_COOLDOWN)
                    {
                        lastShotsFired = currentShotsFired;
                        isShooting = true;
                        return true;
                    }
                }
                // Dodatkowa ochrona: jeśli od ostatniego strzału minęło za mało czasu, ignoruj
                else if ((currentTime - lastShotTime) < SHOT_COOLDOWN)
                {
                    return false;
                }

                // Resetuj stan strzelania jeśli nie strzelano przez dłuższy czas
                if (currentShotsFired == 0)
                {
                    lastShotsFired = 0;
                    isShooting = false;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        private static Vector3 GetEyePosition()
        {
            try
            {
                if (GameState.LocalPlayer != null && GameState.LocalPlayer.PawnAddress != IntPtr.Zero)
                {
                    Vector3 origin = GameState.swed.ReadVec(GameState.LocalPlayer.PawnAddress, Offsets.m_vOldOrigin);
                    Vector3 viewOffset = GameState.swed.ReadVec(GameState.LocalPlayer.PawnAddress, Offsets.m_vecViewOffset);
                    return origin + viewOffset;
                }
            }
            catch { }

            return Vector3.Zero;
        }

        private static Vector3 GetForwardDirection()
        {
            try
            {
                Vector3 viewAngles = GameState.swed.ReadVec(GameState.client, Offsets.dwViewAngles);
                return AnglesToDirection(viewAngles);
            }
            catch
            {
                return new Vector3(0, 1, 0);
            }
        }

        private static Vector3 AnglesToDirection(Vector3 angles)
        {
            float pitch = angles.X * (float)Math.PI / 180.0f;
            float yaw = angles.Y * (float)Math.PI / 180.0f;

            float cosPitch = (float)Math.Cos(pitch);
            float sinPitch = (float)Math.Sin(pitch);
            float cosYaw = (float)Math.Cos(yaw);
            float sinYaw = (float)Math.Sin(yaw);

            return new Vector3(
                cosPitch * cosYaw,
                cosPitch * sinYaw,
                -sinPitch
            );
        }

        public static void Render()
        {
            if (!EnableBulletTrails || activeTrails.Count == 0)
                return;

            try
            {
                if (GameState.renderer == null)
                    return;

                float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                if (viewMatrix == null || viewMatrix.Length < 16)
                    return;

                Vector2 screenSize = GameState.renderer.screenSize;
                Vector3 cameraPos = GetEyePosition();
                Vector3 cameraForward = GetForwardDirection();

                for (int trailIndex = activeTrails.Count - 1; trailIndex >= 0; trailIndex--)
                {
                    var trail = activeTrails[trailIndex];

                    if (trail.Lifetime <= 0) continue;

                    if (!trail.IsVisibleFromCamera(cameraPos, cameraForward))
                    {
                        continue;
                    }

                    float trailAlpha = trail.GetAlpha() * TrailOpacity;
                    if (trailAlpha <= 0.01f) continue;

                    Vector2? previousPoint = null;

                    for (int i = 0; i <= TrailSegments; i++)
                    {
                        float t = (float)i / TrailSegments;
                        Vector3 worldPoint = trail.GetPointOnTrail(t);

                        Vector2 screenPoint = Calculate.WorldToScreen(viewMatrix, worldPoint, screenSize);

                        if (screenPoint.X == -99 || screenPoint.Y == -99)
                        {
                            if (i == 0 || i == TrailSegments)
                            {
                                previousPoint = null;
                                continue;
                            }

                            if (previousPoint.HasValue && i < TrailSegments)
                            {
                                for (int j = i + 1; j <= TrailSegments; j++)
                                {
                                    float nextT = (float)j / TrailSegments;
                                    Vector3 nextWorldPoint = trail.GetPointOnTrail(nextT);
                                    Vector2 nextScreenPoint = Calculate.WorldToScreen(viewMatrix, nextWorldPoint, screenSize);

                                    if (nextScreenPoint.X != -99 && nextScreenPoint.Y != -99)
                                    {
                                        float lerpT = (float)(j - i) / (TrailSegments - i);
                                        screenPoint = Vector2.Lerp(previousPoint.Value, nextScreenPoint, lerpT);
                                        break;
                                    }
                                }
                            }
                        }

                        if (screenPoint.X == -99 || screenPoint.Y == -99)
                        {
                            previousPoint = null;
                            continue;
                        }

                        if (previousPoint.HasValue)
                        {
                            float segmentAlpha = trailAlpha * (1.0f - (t * 0.2f));

                            if (segmentAlpha > 0.01f)
                            {
                                Vector4 segmentColor = trail.GetColorAtPosition(t);
                                segmentColor.W *= segmentAlpha;

                                uint colorU32 = ImGui.ColorConvertFloat4ToU32(segmentColor);

                                float thickness = TrailThickness;
                                if (trail.IsRealTrajectory) thickness *= 1.2f;

                                GameState.renderer.drawList.AddLine(
                                    previousPoint.Value,
                                    screenPoint,
                                    colorU32,
                                    thickness
                                );

                                if (ShowGlowEffect && segmentAlpha > 0.15f)
                                {
                                    Vector4 glowColor = new Vector4(
                                        segmentColor.X * 1.5f,
                                        segmentColor.Y * 1.5f,
                                        segmentColor.Z * 1.5f,
                                        segmentColor.W * GlowIntensity * segmentAlpha
                                    );

                                    uint glowColorU32 = ImGui.ColorConvertFloat4ToU32(glowColor);
                                    GameState.renderer.drawList.AddLine(
                                        previousPoint.Value,
                                        screenPoint,
                                        glowColorU32,
                                        thickness * 3f
                                    );
                                }
                            }
                        }

                        previousPoint = screenPoint;
                    }

                    if (trailAlpha > 0.3f && SparkSize > 0)
                    {
                        Vector2 start2D = Calculate.WorldToScreen(viewMatrix, trail.StartPosition, screenSize);
                        if (start2D.X != -99 && start2D.Y != -99)
                        {
                            DrawSpark(start2D, trailAlpha * 0.8f, true, trail.IsRealTrajectory, trail);
                        }

                        Vector2 end2D = Calculate.WorldToScreen(viewMatrix, trail.EndPosition, screenSize);
                        if (end2D.X != -99 && end2D.Y != -99)
                        {
                            DrawSpark(end2D, trailAlpha, false, trail.IsRealTrajectory, trail);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"BulletTrailManager.Render error: {ex.Message}");
            }
        }

        private static void DrawSpark(Vector2 position, float alpha, bool isStart, bool isRealTrajectory, BulletTrailEffect trail)
        {
            try
            {
                float size = SparkSize * alpha;
                if (size < 0.5f) return;

                Vector4 sparkColor;

                if (isRealTrajectory)
                {
                    sparkColor = isStart ?
                        RealTrailSparkStartColor :
                        RealTrailSparkEndColor;
                }
                else
                {
                    sparkColor = isStart ?
                        SparkStartColor :
                        SparkEndColor;
                }

                if (isStart)
                {
                    sparkColor = trail.DynamicStartColor;
                }
                else
                {
                    sparkColor = trail.DynamicEndColor;
                }

                sparkColor.W *= alpha;

                uint colorU32 = ImGui.ColorConvertFloat4ToU32(sparkColor);

                int segments = isRealTrajectory ? 20 : 16;
                GameState.renderer.drawList.AddCircleFilled(position, size, colorU32, segments);

                if (alpha > 0.4f)
                {
                    Vector4 glowColor = new Vector4(
                        sparkColor.X * 1.2f,
                        sparkColor.Y * 1.2f,
                        sparkColor.Z * 1.2f,
                        alpha * 0.3f
                    );

                    uint glowColorU32 = ImGui.ColorConvertFloat4ToU32(glowColor);
                    GameState.renderer.drawList.AddCircle(
                        position,
                        size * 2.0f,
                        glowColorU32,
                        segments + 8,
                        1.5f
                    );
                }
            }
            catch { }
        }

        public static void ClearAll()
        {
            activeTrails.Clear();
            lastShotsFired = 0;
            lastShotTime = 0f;
            forceTrailNextShot = false;
            isShooting = false;
            timeSinceLastShot = 0f;
            lastTrailAddedTime = 0f;
            lastExternalShotDetectionTime = 0f;
        }

        public static int GetActiveTrailCount()
        {
            return activeTrails.Count;
        }

        public static void TestTrail(bool realImpact = false)
        {
            if (GameState.LocalPlayer == null) return;

            Vector3 eyePosition = GetEyePosition();

            if (realImpact)
            {
                Vector3 forward = GetForwardDirection();
                Vector3 endPosition = eyePosition + (forward * 800f);
                AddTrail(eyePosition, endPosition, true);
            }
            else
            {
                Vector3 forward = GetForwardDirection();
                Vector3 endPosition = eyePosition + (forward * TrailLength);
                AddTrail(eyePosition, endPosition, false);
            }
        }

        public static void UpdateAllTrailColors()
        {
            try
            {
                foreach (var trail in activeTrails)
                {
                    trail.DynamicStartColor = StartColor;
                    trail.DynamicEndColor = EndColor;
                    trail.DynamicStartColor.W = trail.StartColor.W;
                    trail.DynamicEndColor.W = trail.EndColor.W;
                }
            }
            catch (Exception ex)
            {
                // Console.WriteLine($"UpdateAllTrailColors error: {ex.Message}");
            }
        }
    }

    public class BulletTrailService : ThreadService
    {
        protected override void FrameAction()
        {
            BulletTrailManager.Update();
            System.Threading.Thread.Sleep(10);
        }
    }
}