using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class Chams
    {
        public static bool Enabled = false;
        public static bool VisibilityCheck = true;
        public static bool TeamCheck = true;
        public static float BoneThickness = 10f;
        public static Vector4 EnemyColor = new(1, 0, 0, 1f);
        public static Vector4 TeamColor = new(1, 0, 0, 1f);
        public static Vector4 EnemyColorOccluded = new(1, 0, 0, 1f);
        public static Vector4 TeamColorOccluded = new(1, 0, 0, 1f);
        public static Colors VisibleColors = new(TeamColor, EnemyColor);
        public static Colors OccludedColors = new(TeamColorOccluded, EnemyColorOccluded);

        private static Vector4 GetHitboxColor(Hitbox hitbox, bool isTeamate, bool isVisible)
        {
            if (isTeamate)
                return isVisible ? (VisibleColors.TeamRGB ? (Colors.Rgb(VisibleColors.TeamColor.W)) : VisibleColors.TeamColor) : (OccludedColors.TeamRGB ? (Colors.Rgb(OccludedColors.TeamColor.W)) : OccludedColors.TeamColor);
            else
                return isVisible ? (VisibleColors.EnemyRGB ? (Colors.Rgb(VisibleColors.EnemyColor.W)) : VisibleColors.EnemyColor) : (OccludedColors.EnemyRGB ? (Colors.Rgb(OccludedColors.EnemyColor.W)) : OccludedColors.EnemyColor);
        }

        public static void Draw(Entity? entity)
        {
            if (entity == null || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed || entity.Health <= 0 || !Enabled || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || TeamCheck && entity.Team == GameState.LocalPlayer.Team || entity.HitBoxes == null)
                return;

            foreach (Hitbox? hitbox in entity.HitBoxes)
            {
                if (hitbox == null || hitbox.Bone == null || hitbox.BonePosition2D == new Vector2(-99, -99))
                    continue;

                uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(GetHitboxColor(hitbox, entity.IsTeammate, entity.Visible));

                float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

                ShapeRenderer.DrawCapsule3D(hitbox.MinBounds, hitbox.MaxBounds, hitbox.ShapeRadius, hitbox.BoneRotation,
                    hitbox.BonePosition, viewMatrix, preConvertedColor);
            }
        }
    }
}