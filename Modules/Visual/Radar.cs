using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class Radar
    {
        public static bool IsEnabled = false;
        public static bool DrawOnTeam = true;
        public static bool DrawCrossb = false;
        public static Vector4 PointColor = new(1f, 1f, 1f, 1f);
        private static Vector2 CrossPosition = new(200f, 200f);
        public static float RenderRange = 250f;
        public static float Proportion = 2600;
        private static List<Point> Points = [];
        public static int PointType = 0; // 0 = circle, 1 = arrow, 2 = arc
        public static Vector4 EnemyPointColor = new(1, 0, 0, 1); 
        public static Vector4 TeamPointColor = new(0, 1, 0, 1);

        public static void DrawRadar()
        {
            if (!IsEnabled) return;

            DrawPoints();

            if (DrawCrossb)
                DrawCross();
        }

        public static void DrawPoints()
        {
            try
            {
                Points.Clear();

                foreach (Entity e in GameState.Entities)
                {
                    if (e == null || e.Health <= 0 || e.PawnAddress == GameState.LocalPlayer.PawnAddress) continue;

                    float dx = GameState.LocalPlayer.Position.X - e.Position.X;
                    float dy = GameState.LocalPlayer.Position.Y - e.Position.Y;
                    float Scale = (2.0f * RenderRange) / Proportion;
                    float Distance = (float)Math.Sqrt(dx * dx + dy * dy) * Scale;

                    float AngleRad = (GameState.LocalPlayer.ViewAngles.Y * (MathF.PI / 180.0f)) - (float)Math.Atan2(e.Position.Y - GameState.LocalPlayer.Position.Y, e.Position.X - GameState.LocalPlayer.Position.X);

                    Vector2 PointPos;
                    PointPos.X = (CrossPosition.X + Distance * MathF.Sin(AngleRad));
                    PointPos.Y = (CrossPosition.Y - Distance * MathF.Cos(AngleRad));

                    if (Distance <= RenderRange) // if theyre not visible on the radar dont draw them
                    {
                        if (e.Team != GameState.LocalPlayer.Team)
                            Points.Add(new Point(PointPos, EnemyPointColor, PointType, GameState.LocalPlayer.ViewAngles.Y));

                        else if (e.Team == GameState.LocalPlayer.Team && DrawOnTeam)
                            Points.Add(new Point(PointPos, TeamPointColor, PointType, GameState.LocalPlayer.ViewAngles.Y));
                    }
                }

                foreach (Point? point in Points)
                {
                    DrawPoint(point);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void DrawPoint(Point point)
        {
            switch (point.Type)
            {
                case 0:
                    GameState.renderer.drawList.AddCircleFilled(point.Position, 3, ImGui.ColorConvertFloat4ToU32(point.Color));
                    GameState.renderer.drawList.AddCircle(point.Position, 3, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.5f)));
                    break;
                case 1:
                    DrawArrow(point.Position, point.Color, point.Yaw);
                    break;
                case 2:
                    DrawArc(point.Position, point.Color);
                    break;
            }
        }

        private static void DrawArrow(Vector2 Position, Vector4 Color, float Yaw)
        {
            Vector2 a = new(Position.X, Position.Y - 10f);
            Vector2 b = new(Position.X - 10f / 2, Position.Y + 10f / 2);
            Vector2 c = new(Position.X + 10f / 2, Position.Y + 10f / 2);

            a = RotatePoint(a, Position, Yaw);
            b = RotatePoint(b, Position, Yaw);
            c = RotatePoint(c, Position, Yaw);

            GameState.renderer.drawList.AddTriangleFilled(a, b, c, ImGui.ColorConvertFloat4ToU32(Color));
        }

        private static void DrawArc(Vector2 position, Vector4 color)
        {
            GameState.renderer.drawList.AddCircleFilled(position, 8f, ImGui.ColorConvertFloat4ToU32(color), 30);
            GameState.renderer.drawList.AddCircle(position, 8f * 0.95f, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 0.5f)), 0, 0.1f);
        }

        private static Vector2 RotatePoint(Vector2 point, Vector2 origin, float angle)
        {
            float Rad = angle * (MathF.PI / 180.0f);
            float Cos = MathF.Cos(Rad);
            float Sin = MathF.Sin(Rad);

            float dx = point.X - origin.X;
            float dy = point.Y - origin.Y;

            return new Vector2(origin.X + dx * Cos - dy * Sin, origin.Y + dx * Sin + dy * Cos);
        }

        public static void DrawCross()
        {
            GameState.renderer.drawList.AddLine(new Vector2(CrossPosition.X - 100, CrossPosition.Y), new Vector2(CrossPosition.X + 100, CrossPosition.Y), ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor), 1); // enemy color because uh i felt like it
            GameState.renderer.drawList.AddLine(new Vector2(CrossPosition.X, CrossPosition.Y - 100), new Vector2(CrossPosition.X, CrossPosition.Y + 100), ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor), 1);
        }
    }

    public class Point(Vector2 position, Vector4 color, int type, float yaw)
    {
        public Vector2 Position { get; } = position;
        public Vector4 Color { get; } = color;
        public int Type { get; } = type;
        public float Yaw { get; } = yaw;
    }
}
