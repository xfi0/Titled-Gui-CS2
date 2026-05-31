using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
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

        public static void Draw(Entity? entity)
        {
            if (entity == null || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed ||
                !Enabled || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || TeamCheck && entity.Team == GameState.LocalPlayer.Team)
                return;

            foreach (Types.Hitbox? hitbox in entity.HitBoxes)
            {
                if (hitbox == null || hitbox.Bone == null || hitbox.BonePosition2D == new Vector2(-99, -99))
                    continue;

                uint preConvertedColor = (VisibilityCheck && !hitbox.Bone.IsVisible)
                    ? (entity.IsEnemy
                        ? ImGui.ColorConvertFloat4ToU32(EnemyColorOccluded)
                        : ImGui.ColorConvertFloat4ToU32(TeamColorOccluded))
                    : (entity.IsEnemy
                        ? ImGui.ColorConvertFloat4ToU32(EnemyColor)
                        : ImGui.ColorConvertFloat4ToU32(TeamColor));


                float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

                ShapeRenderer.DrawCapsule3D(hitbox.MinBounds, hitbox.MaxBounds, hitbox.ShapeRadius, hitbox.BoneRotation,
                    hitbox.BonePosition, viewMatrix, preConvertedColor);
            }
        }
    }
}