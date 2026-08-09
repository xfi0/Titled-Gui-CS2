using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu;

namespace Titled_Gui.Modules.Visual
{
    internal class Flags
    {
        public static Vector4 TeamTextColor = new(1, 1, 1, 1);
        public static Vector4 EnemyTextColor = new(1, 1, 1, 1);
        public static Colors TextColors = new(TeamTextColor, EnemyTextColor, null, null, false, false, false, false);
        public static bool ScopedEnabled = false;
        public static bool FlashEnabled = false;
        public static bool GunEnabled = false;
        private static Dictionary<string, int> enabledFlags = new();
        private static float _baseFontSize = 18f;

        public static void DrawFlags(Entity entity)
        {
            BoxRect? rect = entity.GetBoxRect();
            if (rect == null)
                return;

            if (GunEnabled)
                GunFlag(entity, rect);

            if (ScopedEnabled)
                ScopedFlag(entity, rect);
            else
                enabledFlags.Remove("Scoped");

            if (FlashEnabled)
                FlashedFlag(entity, rect);
            else
                enabledFlags.Remove("Flash");
        }

        private static Vector4 GetFlagColor(Entity entity)
        {
            if (entity.IsTeammate)
                return TextColors.TeamRGB ? Colors.Rgb(TextColors.TeamColor.W) : TextColors.TeamColor;
            else
                return TextColors.EnemyRGB ? Colors.Rgb(TextColors.EnemyColor.W) : TextColors.EnemyColor;
        }

        public static void ScopedFlag(Entity entity, BoxRect boxRect)
        {
            if (entity == null || GameState.renderer == null || GameState.LocalPlayer == null || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Health <= 0 || entity.Position2D == new Vector2(-99, -99))
                return;

            Vector4 color = GetFlagColor(entity);

            if (!enabledFlags.ContainsKey("Scoped"))
                enabledFlags.TryAdd("Scoped", enabledFlags.Count + 1);

            enabledFlags.TryGetValue("Scoped", out int offsetY);

            string scopedText = entity.IsScoped ? "Scoped" : "Not Scoped";
            Vector2 textPos = new(boxRect.TopRight.X + 4, boxRect.TopRight.Y + (offsetY * 5) * 5);
            float fontSize = (float)Math.Clamp(_baseFontSize - (entity.Distance * 0.004f), 12f, _baseFontSize);

            GameState.renderer.DrawList.AddText(Renderer.TextFont60, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(color), scopedText);
        }

        public static void FlashedFlag(Entity entity, BoxRect boxRect)
        {
            if (entity == null || GameState.LocalPlayer == null || GameState.renderer == null || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Health <= 0 || entity.Position2D == new Vector2(-99, -99))
                return;

            Vector4 color = GetFlagColor(entity);

            if (!enabledFlags.ContainsKey("Flash"))
                enabledFlags.TryAdd("Flash", enabledFlags.Count + 1);

            enabledFlags.TryGetValue("Flash", out int offsetY);

            string flashText = entity.FlashDuration > 0.1 ? $"Flashed {MathF.Round(entity.FlashDuration, 2)}" : $"Not Flashed";
            Vector2 textPos = new(boxRect.TopRight.X + 4, boxRect.TopRight.Y + offsetY * 4);
            float fontSize = (float)Math.Clamp(_baseFontSize - (entity.Distance * 0.004f), 12f, _baseFontSize);

            GameState.renderer.DrawList.AddText(Renderer.TextFont60, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(color), flashText);
        }

        public static void GunFlag(Entity? entity, BoxRect boxRect)
        {
            if (!GunEnabled || GameState.LocalPlayer == null || entity == null || entity.Health <= 0 || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.CurrentWeaponName == null || entity.Position2D == new Vector2(-99, -99) || GameState.renderer == null) return;

            string icon = GunHelper.GetIcon(entity.CurrentWeaponName);
            Vector4 color = GetFlagColor(entity);

            if (string.IsNullOrEmpty(icon))
                return;

            Vector2 textPos = new(boxRect.BottomMiddle.X, boxRect.BottomMiddle.Y + 10f);
            float fontSize = (float)Math.Clamp(_baseFontSize - (entity.Distance * 0.004f), 12f, _baseFontSize);

            GameState.renderer.DrawList.AddText(Renderer.GunIconsFont, fontSize, textPos, ImGui.ColorConvertFloat4ToU32(TextColors.TeamColor), icon);
        }
    }
}
