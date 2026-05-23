using ImGuiNET;
using System.Net.NetworkInformation;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SoundESP : ThreadService
    {
        public static bool Enabled = false;
        public static Vector4 TeamColor = new(0, 1, 0, 1);
        public static Vector4 EnemyColor = new(1, 0, 0, 1);
        public static float MaxLifetime = 1.5f;
        public static float MaxRadius = 20f;
        public static bool TeamCheck = false;
        private static readonly Dictionary<IntPtr, float> EmitTimes = []; // pawn address and emit time
        private static readonly List<SoundRing> ActiveRings = [];
        private static float Now => (float)DateTime.UtcNow.TimeOfDay.TotalSeconds;
        private static readonly object Lock = new();

        public class SoundRing
        {
            public Vector3 Position { get; set; }
            public float StartTime { get; set; }
            public bool IsTeam { get; set; }
        }


        public static void Update(Entity? e)
        {
            if (!Enabled || e == null || e.Health <= 0 || e.Position2D == new Vector2(-99, -99) || (TeamCheck && e.Team == GameState.LocalPlayer.Team)) 
                return;

            if (!EmitTimes.TryGetValue(e.PawnAddress, out float last))
            {
                EmitTimes.Add(e.PawnAddress, e.EmitSoundTime);
                return;
            }

            if (e.EmitSoundTime != last)
            {
                EmitTimes[e.PawnAddress] = e.EmitSoundTime;

                lock (Lock)
                {
                    ActiveRings.Add(new SoundRing()
                    {
                        Position = e.Position,
                        StartTime = Now,
                        IsTeam = e.Team == GameState.LocalPlayer.Team
                    });
                }
            }

            foreach (var key in EmitTimes.Keys.Where(k => k == IntPtr.Zero).ToList())
            {
                EmitTimes.Remove(key);
            }
        }

        public static void Draw()
        {
            if (!Enabled) 
                return;
            try
            {
                float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                float now = Now;

                lock (Lock)
                {
                    ActiveRings.RemoveAll(r => now - r.StartTime > MaxLifetime);
                    foreach (var ring in ActiveRings)
                    {
                        float elapsed = now - ring.StartTime;
                        float t = elapsed / MaxLifetime;
                        float radius = MaxRadius * t;
                        float alpha = 1f - t;
                        Vector4 color1 = ring.IsTeam ? TeamColor : EnemyColor;
                        uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(color1.X, color1.Y, color1.Z, alpha));

                        const int segments = 32;
                        List<Vector2> points = [];
                        float step = MathF.PI * 2f / segments;

                        for (float lat = 0f; lat < MathF.PI * 2f; lat += step)
                        {
                            Vector3 position = ring.Position +
                                               new Vector3(MathF.Cos(lat) * radius, MathF.Sin(lat) * radius, 0f);
                            Vector2 position2D = Calculate.WorldToScreen(viewMatrix, position);
                            if (position2D == new Vector2(-99, -99))
                                continue;

                            points.Add(position2D);
                        }

                        if (points.Count < 2) continue;

                        Vector2[] pointsArray = points.ToArray();
                        unsafe
                        {
                            fixed (Vector2* ptr = pointsArray)
                            {
                                GameState.renderer.drawList.AddPolyline(ref *ptr, pointsArray.Length, color,
                                    ImDrawFlags.Closed,
                                    2f);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in Sound ESP Draw: " + ex);
            }
        }

        protected override void FrameAction()
        {
            if (!Enabled)
                return;

            foreach (Entity e in GameState.Entities)
            {
                Update(e);
            }
        }
    }
}