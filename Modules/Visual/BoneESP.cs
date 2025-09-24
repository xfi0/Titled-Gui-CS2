using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public static class BoneESP
    {
        public static float BoneThickness = 5f;
        public static bool EnableBoneESP = false;
        public static bool TeamCheck = false;
        public static Vector4 BoneColor = new(1f, 1f, 1f, 1f);
        public static float GlowAmount = 1f;

        public static readonly (int, int)[] BoneConnections = 
        [
            (0, 1), // Waist to Neck
            (1, 2), // Neck to Head
            (1, 3), // Neck to ShoulderLeft
            (3, 4), // ShoulderLeft to ForeLeft
            (4, 5), // ForeLeft to HandLeft
            (1, 6), // Neck to ShoulderRight
            (6, 7), // ShoulderRight to ForeRight
            (7, 8), // ForeRight to HandRight
            (0, 9), // Waist to KneeLeft
            (9, 10), // KneeLeft to FeetLeft
            (0, 11), // Waist to KneeRight
            (11, 12), // KneeRight to FeetRight
        ];
        public enum BoneIds // short simple 
        {
            Waist = 0,
            Neck = 5,
            Head = 6,
            ShoulderLeft = 8,
            ForeLeft = 9,
            HandLeft = 11,
            ShoulderRight = 13,
            ForeRight = 14,
            HandRight = 16,
            KneeLeft = 23,
            FeetLeft = 24,
            KneeRight = 26,
            FeetRight = 27,
        }
        public static void DrawBoneLines(Data.Entity.Entity entity, Renderer renderer)
        {
            if (!EnableBoneESP || entity == null || entity.Bones2D == null || (TeamCheck && entity.Team == GameState.localPlayer.Team) || entity.PawnAddress == GameState.localPlayer.PawnAddress || entity.Bones == null || entity.Health == 0 || BoxESP.FlashCheck && GameState.localPlayer.IsFlashed) return; 

            float thickness = Math.Clamp(BoneThickness / (entity.Distance * 0.1f), 0.5f, 2f); // calculate thickness based on Distance, minimum 0.5f and maximum 2f stops it from being massive
            uint boneColor = ImGui.GetColorU32(Colors.RGB ? Colors.Rgb(0.5f) : BoneColor); //get color

            foreach (var (a, b) in BoneConnections)
            {
                if (a >= entity.Bones2D.Count || b >= entity.Bones2D.Count)
                    continue;

                Vector2 boneA = entity.Bones2D[a];
                Vector2 boneB = entity.Bones2D[b];

                if (IsValidScreenPoint(boneA) && IsValidScreenPoint(boneB))
                {
                    if (GlowAmount > 0)
                    {
                        DrawHelpers.DrawGlowLine(renderer.drawList, boneA, boneB, BoneColor, GlowAmount);
                        DrawHelpers.DrawGlowCircle(renderer.drawList, boneA, thickness * 2, BoneColor, GlowAmount);
                    }
                    renderer.drawList.AddLine(boneA, boneB, boneColor, thickness); //draw a line between the Bones
                    renderer.drawList.AddCircleFilled(boneA, thickness * 2, boneColor); //draw a circle at the start bone
                }
            }

            Vector2 HeadPos = entity.Bones2D[2];
            if (IsValidScreenPoint(HeadPos))
            {
                float radius = Math.Clamp(10f / (entity.Distance * 0.05f), 3f, 10f);

                if (GlowAmount > 0)
                    DrawHelpers.DrawGlowCircle(renderer.drawList, HeadPos, radius, BoneColor, GlowAmount);

                renderer.drawList.AddCircleFilled(HeadPos, radius, boneColor); // draw a circle at the Head bone extra big
            }
        }

        public static bool IsValidScreenPoint(Vector2 pt)
        {
            return !float.IsNaN(pt.X) && !float.IsNaN(pt.Y) && !float.IsInfinity(pt.X) && !float.IsInfinity(pt.Y) && pt.X > 0 && pt.Y > 0;
        }
        public static bool IsValidScreenPoint(Vector3 pt)
        {
            return !float.IsNaN(pt.X) && !float.IsNaN(pt.Y) && !float.IsNaN(pt.Z) && !float.IsInfinity(pt.X) && !float.IsInfinity(pt.Y) && !float.IsInfinity(pt.Z) && pt.X > 0 && pt.Y > 0 && pt.Z > 0;
        }
    }
}