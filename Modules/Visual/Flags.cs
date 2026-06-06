using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu;

namespace Titled_Gui.Modules.Visual
{
    internal class Flags
    {
        public static Vector4 FlagColor = new(1, 1, 1, 1);
        public static bool ScopedEnabled = false;
        public static bool FlashEnabled = false;
        private static Dictionary<string, int> enabledFlags = new();
        private static float _baseFontSize = 18f;

        public static void DrawFlags(Entity entity)
        {
            Types.BoxRect? rect = entity.GetBoxRect();
            if (rect == null)
                return;

            if (ScopedEnabled)
                ScopedFlag(entity, rect);
            else
                enabledFlags.Remove("Scoped");

            if (FlashEnabled)
                FlashedFlag(entity, rect);
            else
                enabledFlags.Remove("Flash");
        }

        public static void ScopedFlag(Entity entity, Types.BoxRect boxRect)
        {
            if (entity == null || entity.PawnAddress == GameState.LocalPlayer.PawnAddress|| entity.Health <= 0 || entity.Position2D == new Vector2(-99, -99))
                return;

            if (!enabledFlags.ContainsKey("Scoped"))
                enabledFlags.TryAdd("Scoped", enabledFlags.Count + 1);

            enabledFlags.TryGetValue("Scoped", out int offsetY);

            string scopedText = entity.IsScoped ? "Scoped" : "Not Scoped";
            Vector2 textPos = new(boxRect.TopRight.X + 4, boxRect.TopRight.Y + (offsetY * 5) * 5);
            float fontSize = (float)Math.Clamp(_baseFontSize - (entity.Distance * 0.004f), 12f, _baseFontSize);

            GameState.renderer.DrawList.AddText(Renderer.TextFont60, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(FlagColor), scopedText);
        }

        public static void FlashedFlag(Entity entity, Types.BoxRect boxRect)
        {
            if (entity == null || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Health <= 0 || entity.Position2D == new Vector2(-99, -99))
                return;

            if (!enabledFlags.ContainsKey("Flash"))
                enabledFlags.TryAdd("Flash", enabledFlags.Count + 1);

            enabledFlags.TryGetValue("Flash", out int offsetY);

            string flashText = entity.FlashDuration > 0.1 ? $"Flashed {MathF.Round(entity.FlashDuration, 2)}" : $"Not Flashed";
            Vector2 textPos = new(boxRect.TopRight.X + 4, boxRect.TopRight.Y + offsetY * 4);
            float fontSize = (float)Math.Clamp(_baseFontSize - (entity.Distance * 0.004f), 12f, _baseFontSize);

            GameState.renderer.DrawList.AddText(Renderer.TextFont60, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(FlagColor), flashText);
        }
    }
}
