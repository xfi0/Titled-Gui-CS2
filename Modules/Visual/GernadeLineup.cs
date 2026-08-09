using ImGuiNET;
using Newtonsoft.Json;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Math;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;
using static Titled_Gui.Data.Game.Events;

namespace Titled_Gui.Modules.Visual
{
    internal class GernadeLineup
    {
        public static bool Enabled = false;
        public static bool AlwaysShow = false;
        public static List<string> TypesList = ["Still", "Running", "Jump", "Run Jump"];
        public static Vector4 PositionColorInside = new(1f, 0f, 0f, 1f);
        public static Vector4 PositionColorOutside = new(0f, 1f, 0f, 1f);
        public static Vector4 AngleColor = new(1f, 0.5f, 0f, 1f);
        public static int SelectedType = 0;
        public static string LineupName = "Lineup 1";

        private static bool cacheInitialized = false;
        private static float circleRadius = 30f;
        private static float smoothing = 10f;
        private static float lockRadius = 50f;
        private static Vector2 remainder = Vector2.Zero;
        private static Dictionary<string, GrenadeLinup> lineupCache = new Dictionary<string, GrenadeLinup>();
        private static string lastMap = "";

        public static void Initialize()
        {
            GameEvents.OnMapChanged += InvalidateCache;
        }

        public static void InvalidateCache(string newMap)
        {
            lineupCache.Clear();
            cacheInitialized = false;
        }

        public static void InitializeLineupCache()
        {
            if (cacheInitialized)
                return;

            lastMap = GlobalVar.GetCurrentMapName();
            if (string.IsNullOrEmpty(lastMap))
                return;

            string directory = Path.Combine(Configs.titledDocumentsFolder, "lineups", lastMap.Replace(".vpk", ""));
            if (!Directory.Exists(directory))
                return;

            foreach (string file in Directory.GetFiles(directory, "*.json"))
            {
                var lineup = JsonConvert.DeserializeObject<GrenadeLinup>(File.ReadAllText(file));
                if (lineup == null)
                {
                    Console.WriteLine("Lineup deserialize failed.");
                    continue;
                }

                lineupCache.Add(lineup.Name, lineup);
            }
            cacheInitialized = true;
        }

        public static void DrawAllLineups()
        {
            try
            {
                if (!cacheInitialized)
                {
                    InitializeLineupCache();
                    return;
                }

                if (GameState.renderer == null || GameState.memory == null || GameState.LocalPlayer == null)
                    return;

                float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

                foreach (var lineup in lineupCache.Values)
                {
                    float distance = Vector3.Distance(GameState.LocalPlayer.Position, lineup.Position);
                    if (distance > 300f && !AlwaysShow)
                        continue;

                    bool insideCircle = InsideCircle(GameState.LocalPlayer.Position, lineup.Position, circleRadius);

                    uint circle3DColor = insideCircle ? ImGui.ColorConvertFloat4ToU32(PositionColorInside) : ImGui.ColorConvertFloat4ToU32(PositionColorOutside);
                    uint circle2DColor = ImGui.ColorConvertFloat4ToU32(AngleColor);

                    ShapeRenderer.Draw3DCircle(viewMatrix, lineup.Position, circleRadius, Vector3.UnitZ, circle3DColor);
                    ShapeRenderer.Draw3DCircle(viewMatrix, lineup.Angle, circleRadius, lineup.CircleDirection, circle2DColor);

                    Vector2 circleScreen = MathUtils.WorldToScreen(viewMatrix, lineup.Angle);
                    if (circleScreen == new Vector2(-99, -99))
                        continue;


                    Vector2 screenCenter = new(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2);
                    float screenDistance = Vector2.Distance(circleScreen, screenCenter);
                    float sensitivity = GameState.LocalPlayer.Sensitivity;

                    CircleLock(insideCircle, circleScreen, screenCenter, screenDistance, sensitivity);

                    GameState.renderer.DrawList.AddLine(screenCenter, circleScreen, circle2DColor);
                    GameState.renderer.DrawList.AddCircleFilled(circleScreen, 6f, circle2DColor);

                    GameState.renderer.DrawList.AddText(new(circleScreen.X - 50f, circleScreen.Y - 50f), ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)), TypesList[(int)lineup.LaunchType]);

                    Vector2 position2D = MathUtils.WorldToScreen(viewMatrix, lineup.Position);
                    if (position2D == new Vector2(-99, -99))
                        continue;

                    GameState.renderer.DrawList.AddText(
                        new(position2D.X - 20f, position2D.Y - 20f),
                        ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)),
                        lineup.Name
                    );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gernade Lineup exception: " + ex);
            }
        }

        private static void CircleLock(bool insideCircle, Vector2 circleScreen, Vector2 screenCenter, float screenDistance, float sensitivity)
        {
            if (!User32.GetKeyHeld(Keys.LButton) || !insideCircle || screenDistance >= lockRadius)
            {
                remainder = Vector2.Zero;
                return;
            }

            float dx = ((circleScreen.X - screenCenter.X) / sensitivity) / smoothing + remainder.X;
            float dy = ((circleScreen.Y - screenCenter.Y) / sensitivity) / smoothing + remainder.Y;

            int ix = (int)MathF.Round(dx);
            int iy = (int)MathF.Round(dy);

            remainder.X = dx - ix;
            remainder.Y = dy - iy;

            User32.MouseMove(ix, iy);
        }

        public static void SaveLineup(string name, GrenadeLaunchType launchType)
        {
            if (GameState.LocalPlayer == null)
                return;

            Vector3 eyeOrigin = GameState.LocalPlayer.EyePosition;
            Vector3 forward = MathUtils.AngleToForward(GameState.LocalPlayer.EyeDirection);
            Vector3 circlePos = eyeOrigin + forward * 100f;

            GrenadeLinup lineup = new()
            {
                Name = name,
                Position = GameState.LocalPlayer.Position,
                LaunchType = launchType,
                MapName = GlobalVar.GetCurrentMapName(),
                Angle = circlePos,
                CircleDirection = forward,
            };

            string dir = Path.Combine(Configs.titledDocumentsFolder, "lineups", lineup.MapName.Replace(".vpk", ""));
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            string path = Path.Combine(dir, $"{lineup.Name}.json");
            if (lineupCache.ContainsKey(lineup.Name))
                lineupCache.Remove(lineup.Name);

            lineupCache.Add(lineup.Name, lineup);
            File.WriteAllText(path, JsonConvert.SerializeObject(lineup, Formatting.Indented));
        }

        private static bool InsideCircle(Vector3 point, Vector3 center, float radius)
        {
            return Vector3.Distance(point, center) <= radius;
        }
    }
}
