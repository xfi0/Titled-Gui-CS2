using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SoundESP // TODO: animate and optimize
    {
        public static bool Enabled = false;
        public static Vector4 TeamColor = new(0, 1, 0, 1);
        public static Vector4 EnemyColor = new(1, 0, 0, 1);
        private static readonly Dictionary<Entity, float> emitTimes = []; // entity, emittime, timesincestart

        private class Point
        {
            public Vector2 Position { get; set; }
            public float StartTime { get; set; }
            public float LifeTime { get; set; }
        }
        public static void DrawSoundESP(Entity? e)
        {
            if (!Enabled || e == null || e.Health <= 0) return;

            if (!emitTimes.ContainsKey(e) || e.EmitSoundTime > emitTimes[e]) 
                emitTimes[e] = e.EmitSoundTime;
            

            if (e.EmitSoundTime == emitTimes[e])
                return;

            //Console.WriteLine(e.emitSoundTime);
            List<Point> points = CreatePoints(e);

            AnimateAndDrawPoints(points, e);
        }
        private static List<Point> CreatePoints(Entity e)
        {
            List<Point> points = new();
            float radius = 7f;
            float step = MathF.PI * 2 / 64;
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            for (float lat = 0f; lat <= MathF.PI * 2.0f; lat += step)
            {
                Vector3 point = new(MathF.Sin(lat) * radius, MathF.Cos(lat) * radius, 0f);
                Vector2 point2D = Calculate.WorldToScreen(viewMatrix, e.Position + point);

                if (point2D == new Vector2(-99, -99)) continue;

                points.Add(new Point
                {
                    Position = point2D,
                    StartTime = e.EmitSoundTime,
                    LifeTime = DateTime.Now.Second
                });
            }

            return points;
        }

        private static void AnimateAndDrawPoints(List<Point> points, Entity e)
        {
            foreach (var point in points)
            {
                float elapsedTime = DateTime.Now.Second - point.LifeTime;

                float scale = elapsedTime;

                uint color = ImGui.ColorConvertFloat4ToU32(new Vector4(TeamColor.X, TeamColor.Y, TeamColor.Z, 1));
                Vector2 tempPoint2D = point.Position;
                List<Vector2> scaledPoints = [.. points.Select(p =>
                {
                    Vector2 scaledPoint = p.Position * 1;
                    return scaledPoint;
                })];
                Vector2[] pointArray = scaledPoints.ToArray();

                unsafe
                {
                    fixed (Vector2* pointPtr = pointArray)
                    {
                        GameState.renderer.drawList.AddPolyline(ref *pointPtr, points.Count, ImGui.ColorConvertFloat4ToU32(TeamColor), ImDrawFlags.Closed, 2f);
                    }
                }
            }
        }
    }
}
