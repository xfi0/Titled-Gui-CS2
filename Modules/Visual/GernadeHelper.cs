using ImGuiNET;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Xml.Linq;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu;

namespace Titled_Gui.Modules.Visual
{
    internal class GernadeHelper
    {

        public static void DrawAllLineups()
        {
            string mapName = GlobalVar.GetCurrentMapName();
            if (string.IsNullOrEmpty(mapName)) return;

            string dir = Path.Combine(AppContext.BaseDirectory, "lineups", mapName.Replace(".vpk", ""));
            if (!Directory.Exists(dir)) return;

            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            foreach (string file in Directory.GetFiles(dir, "*.json"))
            {
                var lineup = JsonConvert.DeserializeObject<Types.GernadeHelperType>(File.ReadAllText(file));
                if (lineup == null) continue;

                float dist = Vector3.Distance(GameState.LocalPlayer.Position, lineup.Position);
                if (dist > 300f) continue;

                Draw3DCircle(viewMatrix, lineup.Position, 30f,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(0f, 1f, 0f, 1f)));


                GameState.renderer.drawList.AddCircleFilled(
                    lineup.Angle,
                    6f,
                    ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 0.5f, 0f, 1f))
                );


                Vector2 screenPos = Calculate.WorldToScreen(viewMatrix, lineup.Position);
                if (screenPos != new Vector2(-99, -99))
                {
                    GameState.renderer.drawList.AddText(
                        new System.Numerics.Vector2(screenPos.X - 20f, screenPos.Y - 20f),
                        ImGui.ColorConvertFloat4ToU32(new Vector4(1f, 1f, 1f, 1f)),
                        lineup.Name
                    );
                }
            }
        }

        private static void Draw3DCircle(float[] viewMatrix, Vector3 center, float radius, uint color, int segments = 32)
        {
            List<Vector2> screenPoints = new();

            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * Math.PI * 2.0 / segments);

                Vector3 worldPoint = new Vector3(
                    center.X + radius * MathF.Cos(angle),
                    center.Y + radius * MathF.Sin(angle),
                    center.Z
                );

                Vector2 screenPoint = Calculate.WorldToScreen(viewMatrix, worldPoint);
                if (screenPoint == new Vector2(-99, -99))
                {
                    screenPoints.Clear();
                    continue;
                }

                screenPoints.Add(screenPoint);
            }

            for (int i = 0; i < screenPoints.Count; i++)
            {
                Vector2 a = screenPoints[i];
                Vector2 b = screenPoints[(i + 1) % screenPoints.Count];
                GameState.renderer.drawList.AddLine(a, b, color, 2f);
            }
        }

        public static void SaveLineup(string name, Types.GernadeLaunchType launchType)
        {
            Types.GernadeHelperType lineup = new()
            {
                Name = name,
                Position = GameState.LocalPlayer.Position,
                LaunchType = launchType,
                MapName = GlobalVar.GetCurrentMapName(),
                Angle = new Vector2(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2)
            };

            string dir = Path.Combine(AppContext.BaseDirectory, "lineups", lineup.MapName.Replace(".vpk", ""));
            Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, $"{lineup.Name}.json");

            File.WriteAllText(path, JsonConvert.SerializeObject(lineup, Formatting.Indented));
        }
    }
}
