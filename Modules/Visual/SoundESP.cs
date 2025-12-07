using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SoundESP // TODO: animate and optimize
    {
        public static bool enabled = false;
        public static Vector4 color = new(1, 1, 1, 1);

        public static void DrawSoundESP(Entity e)
        {
            if (!enabled || e == null || e.Health <= 0 || e.emitSoundTime < 1) return;
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            Console.WriteLine(e.emitSoundTime);
            float pi = MathF.PI;
            float radius = 30f;
            float step = pi * 2 / 64;
            List<Vector2> points = new();

            for (float lat = 0f; lat <= pi * 2.0f; lat += step)
            {

                Vector3 point = new Vector3(MathF.Sin(lat) * radius, MathF.Cos(lat) * radius, 0f);
                Vector2 point2D = Calculate.WorldToScreen(viewMatrix, e.Position + point, GameState.renderer.screenSize);

                if (point == new Vector3(0, 0, 0))
                    continue;

                if (point2D == new Vector2(0, 0))
                    continue;

                points.Add(new(point2D.X, point2D.Y));
            }
            if (points.Count > 1)
            {
                foreach (var point in points)
                {
                    var tempPoint = point;
                    //GameState.renderer.drawList.AddPolyline(ref tempPoint, points.Count, ImGui.ColorConvertFloat4ToU32(color), ImGuiNET.ImDrawFlags.Closed, 0.5f);
                    unsafe
                    {
                        fixed (Vector2* pointa = points.ToArray())
                            GameState.renderer.drawList.AddPolyline(ref *pointa, points.Count, ImGui.ColorConvertFloat4ToU32(color), ImDrawFlags.Closed, 2f);
                    }
                }
            }
        }
    }
}
