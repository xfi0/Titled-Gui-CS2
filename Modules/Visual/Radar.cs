using ImGuiNET;
using NAudio.Gui;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
  
    internal class Radar 
    {
        public static bool IsEnabled = false;
        public static Vector4 PointColor = new(1f, 1f, 1f, 1f);

        private static Vector2 CrossPosition = new(200f, 200f);

        public static float RenderRange = 250f;

        public static float Proportion = 2600;

        public static void DrawRadar()
        {
            if (!IsEnabled) return;

            DrawPoints();
            DrawCross();
        }
        public static void DrawPoints()
        {

            foreach (var e in GameState.Entities)
            {
                float LocalYaw = GameState.swed.ReadVec(GameState.client, Offsets.dwViewAngles).Y;
                //Console.WriteLine("Yaw: " + LocalYaw);

                float dx = GameState.localPlayer.Position.X - e.Position.X;
                float dy = GameState.localPlayer.Position.Y - e.Position.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                float angleRad = (LocalYaw * (MathF.PI / 180.0f)) - (float)Math.Atan2(e.Position.Y - GameState.localPlayer.Position.Y, e.Position.X - GameState.localPlayer.Position.X);

                float scale = (2.0f * RenderRange) / Proportion;
                distance *= scale;

                Vector2 PointPos;
                PointPos.X = (float)(e.Position.X + distance * Math.Sin(angleRad));
                PointPos.Y = (float)(e.Position.Y - distance * Math.Cos(angleRad));

                float renderRange = RenderRange;
                //if (PointPos.X < e.Position.X - renderRange || PointPos.X > e.Position.X + renderRange || PointPos.Y > e.Position.Y + renderRange || PointPos.Y < e.Position.Y - renderRange)
                //{
                //    return;
                //}

                //Console.WriteLine(PointPos);
                //Console.WriteLine(GameState.localPlayer.Position);
                GameState.renderer.drawList.AddCircleFilled(PointPos, 3, ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor));
                GameState.renderer.drawList.AddCircleFilled(PointPos, 3, ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor));
            }
        }
        public static void DrawCross()
        {
            GameState.renderer.drawList.AddLine(new Vector2(CrossPosition.X - 200 / 2, CrossPosition.Y), new Vector2(CrossPosition.X + 200 / 2, CrossPosition.Y), ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor), 1);
            GameState.renderer.drawList.AddLine(new Vector2(CrossPosition.X, CrossPosition.Y - 200 / 2), new Vector2(CrossPosition.X, CrossPosition.Y + 200 / 2), ImGui.ColorConvertFloat4ToU32(Titled_Gui.Classes.Colors.EnemyColor), 1);
        }
    }
}