using ImGuiNET;
using System.Numerics;
using System.Windows.Forms.VisualStyles;
using Titled_Gui.Classes;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Game.GameState;

namespace Titled_Gui.Modules.Visual
{
    public static class BoneESP
    {
        public static float BoneThickness = 5f;
        public static bool EnableBoneESP = false;
        public static bool TeamCheck = false;
        public static Vector4 BoneColor = new(1f, 1f, 1f, 1f);
        public static float GlowAmount = 1f;
        public static string[] Types = ["Straight", "Beizer"];
        public static int CurrentType = 0;

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
            if (!EnableBoneESP || entity == null || entity.Bones2D == null || (TeamCheck && entity.Team == GameState.LocalPlayer.Team) || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Bones == null || entity.Health == 0 || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) return; 

            float thickness = Math.Clamp(BoneThickness / (entity.Distance * 0.1f), 0.5f, 1f); // calculate thickness based on Distance, minimum 0.5f and maximum 2f stops it from being massive
            uint boneColor = ImGui.GetColorU32(Colors.RGB ? Colors.Rgb(0.5f) : BoneColor); //get color

            foreach (var (a, b) in BoneConnections)
            {
                if (a >= entity.Bones2D.Count || b >= entity.Bones2D.Count)
                    continue;

                Vector2 boneA = entity.Bones2D[a];
                Vector2 boneB = entity.Bones2D[b];
                if (IsValidScreenPoint(boneA) && IsValidScreenPoint(boneB))
                {
                    float boneLength = Vector2.Distance(boneA, boneB);
                    float curve = Math.Clamp(boneLength * 0.12f, 2f, 8f);
                    if (GlowAmount > 0)
                    {
                        switch (CurrentType)
                        {
                            case 0:
                                DrawHelpers.DrawGlowLine(renderer.drawList, boneA, boneB, BoneColor, GlowAmount, ThickNess: thickness);
                                DrawHelpers.DrawGlowCircle(renderer.drawList, boneA, thickness * 2, BoneColor, GlowAmount);
                                break;
                            case 1:
                                DrawHelpers.DrawGlowBezier(renderer.drawList, boneB, (boneB + boneA) * 0.5f + Vector2.Normalize(new Vector2(-(boneA - boneB).Y, (boneA - boneB).X)) * curve, (boneB + boneA) * 0.5f + Vector2.Normalize(new Vector2(-(boneA - boneB).Y, (boneA - boneB).X)) * (curve * 0.5f), boneA, BoneColor, GlowAmount / 2, thickness);
                                break;
                        }
                    }

                    switch (CurrentType)
                    {
                        case 0:
                            renderer.drawList.AddLine(boneA, boneB, boneColor, thickness); //draw a line between the Bones
                            renderer.drawList.AddCircleFilled(boneA, thickness * 2, boneColor); //draw a circle at the start bone
                            break;
                        case 1:
                            renderer.drawList.AddBezierCubic(boneB, (boneB + boneA) * 0.5f + Vector2.Normalize(new Vector2(-(boneA - boneB).Y, (boneA - boneB).X)) * curve, (boneB + boneA) * 0.5f + Vector2.Normalize(new Vector2(-(boneA - boneB).Y, (boneA - boneB).X)) * (curve * 0.5f), boneA, boneColor, thickness);
                            break;
                    }
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
        public static void DrawBonePreview(Vector2 Center)
        {
            float EntityHeight = 150f;
            Vector4 Color = Colors.EnemyColor;
            uint UColor = ImGui.GetColorU32(Color);
            var DrawList = ImGui.GetWindowDrawList();

            Vector2 Neck = Center + new Vector2(0, -EntityHeight / 3);
            Vector2 Spine = Center + new Vector2(10, 0);
            Vector2 Pelvis = Center + new Vector2(12, 20);
            Vector2 LegLeftUp = Pelvis + new Vector2(3, 5);
            Vector2 LegLeftMid = LegLeftUp + new Vector2(-5, 35);
            Vector2 LegLeftDown = LegLeftMid + new Vector2(8, 45);
            Vector2 LegRightUp = Pelvis + new Vector2(-27, 35);
            Vector2 LegRightDown = LegRightUp + new Vector2(12, 30);
            Vector2 ScapulaLeft = Neck + new Vector2(10, 5);
            Vector2 ArmLeftUp = ScapulaLeft + new Vector2(-15, 25);
            Vector2 ArmLeftDown = ArmLeftUp + new Vector2(-20, -10);
            Vector2 ScapulaRight = Neck + new Vector2(-10, 5);
            Vector2 ArmRightUp = ScapulaRight + new Vector2(-13, 23);
            Vector2 ArmRightDown = ArmRightUp + new Vector2(-7, -8);
            Vector2 Head = Neck + new Vector2(0, -40);

            (Vector2, Vector2)[] Segments =
            [
                (Neck, Spine),
                (Spine, Pelvis),
                (Pelvis, LegLeftUp),
                (LegLeftUp, LegLeftMid),
                (LegLeftMid, LegLeftDown),
                (Pelvis, LegRightUp),
                (LegRightUp, LegRightDown),
                (Neck, ScapulaLeft),
                (ScapulaLeft, ArmLeftUp),
                (ArmLeftUp, ArmLeftDown),
                (Neck, ScapulaRight),
                (ScapulaRight, ArmRightUp),
                (ArmRightUp, ArmRightDown)
            ];

            foreach (var (A, B) in Segments)
            {
                if (GlowAmount > 0)
                    DrawHelpers.DrawGlowLine(DrawList, A, B, BoneColor, GlowAmount * 0.5f);

                DrawList.AddLine(A, B, UColor, BoneThickness * 0.5f);
                DrawList.AddCircleFilled(A, BoneThickness * 0.5f, UColor);
            }

            if (GlowAmount > 0)
                DrawHelpers.DrawGlowCircle(DrawList, Head, 10f, BoneColor, GlowAmount);

            DrawList.AddCircle(Head, 10f, UColor, 12, 0.7f);
        }
    }
}