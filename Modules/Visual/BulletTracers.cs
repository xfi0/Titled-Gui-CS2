using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Math;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.MapParser;
using Titled_Gui.Data.Menu.Types;

namespace Titled_Gui.Modules.Visual
{
    internal class BulletTracers : ThreadService
    {
        public static bool Enabled = false;
        public static Colors TracerColors = new(null, null, new(1, 1, 1, 0.85f));
        public static float Duration = 5f;
        public static float Thickness = 1f;
        public static float Radius = 1f;
        private static List<BulletTracer> _bulletTracers = [];
        private static int _lastShotsFired = 0;
        private static int _segments = 16;

        public static void Render()
        {
            if (!Enabled || GameState.LocalPlayer == null || GameState.memory == null || VisibilityCheck.mapLoaderInstance == null)
                return;

            var viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            lock (_bulletTracers)
            {
                foreach (var bulletTracer in _bulletTracers)
                {
                    Vector4 adjustedColor = new(TracerColors.PrimaryColor.X, TracerColors.PrimaryColor.Y, TracerColors.PrimaryColor.Z, Math.Clamp(bulletTracer.TimeLeft / bulletTracer.TotalTime, 0, 1));
                    ShapeRenderer.DrawCylinder3D(viewMatrix, bulletTracer.StartPoint, bulletTracer.IntersectPoint, adjustedColor, _segments, Radius, Thickness);
                }
            }
        }

        protected override void FrameAction()
        {
            if (!Enabled || GameState.LocalPlayer == null || GameState.memory == null || VisibilityCheck.mapLoaderInstance == null)
                return;

            if (_lastShotsFired == 0)
                _lastShotsFired = GameState.LocalPlayer.ShotsFired; // so if you start mid round it doesnt draw e.g. 30 at once.s

            if (GameState.LocalPlayer.ShotsFired > _lastShotsFired)
            {
                Vector3 forward = MathUtils.AngleToForward(GameState.LocalPlayer.ViewAngles);
                Vector3 end = GameState.LocalPlayer.EyePosition + forward * 5000f;
                Vector3 eyePosition = GameState.LocalPlayer.EyePosition;
                if (VisibilityCheck.mapLoaderInstance.Intersects(eyePosition, end, out Vector3 intersectPoint))
                {
                    BulletTracer tracer = new(eyePosition, intersectPoint, Duration);
                    _bulletTracers.Add(tracer);
                }
            }

            _lastShotsFired = GameState.LocalPlayer.ShotsFired;
            Console.WriteLine(_bulletTracers.Count);

            for (int i = _bulletTracers.Count - 1; i >= 0; i--)
            {
                var newTime = _bulletTracers[i].TimeLeft - 0.1f;
                _bulletTracers[i].TimeLeft = newTime;
                if (_bulletTracers[i].TimeLeft <= 0.01f)
                    _bulletTracers.RemoveAt(i);
            }

            Thread.Sleep(50);
        }
    }
}
