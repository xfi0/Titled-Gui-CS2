using ImGuiNET;
using System.Numerics;
using System.Runtime.CompilerServices;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    public static class BoneESP
    {
        public static float BoneThickness = 4.5f;
        public static bool EnableBoneESP = false;
        public static bool TeamCheck = false;
        private static Vector4 _visibleBoneColorTeam = new(1f, 1f, 1f, 1f);
        private static Vector4 _visibleBoneColorEnemy = new(1f, 1f, 1f, 1f);
        private static Vector4 _occludedBoneColorTeam = new(0f, 0f, 1f, 1f);
        private static Vector4 _occludedBoneColorEnemy = new(0f, 0f, 1f, 1f);
        public static Colors VisibleColors = new(_visibleBoneColorTeam, _visibleBoneColorEnemy, null, null, false, false, false, false);
        public static Colors OccludedColors = new(_occludedBoneColorTeam, _occludedBoneColorEnemy, null, null, false, false, false, false);
        public static float GlowAmount = 0f;
        public static string[] Types = ["Straight", "Bezier"];
        public static int CurrentType = 0;
        public static bool visibilityCheck = true;
        public static bool RGB = false;
        public static (int, int)[] GetBoneConnections(int team)
        {
            var ids = team == 2 ? typeof(BoneIds_T) : typeof(BoneIds_CT);

            int Get(string name) => (int)Enum.Parse(ids, name);

            return [
                (Get("Pelvis"),       Get("Stomach")),
                (Get("Stomach"),      Get("LowerChest")),
                (Get("LowerChest"),   Get("UpperChest")),
                (Get("UpperChest"),   Get("Neck")),
                (Get("Neck"),         Get("Head")),
                (Get("Neck"),         Get("LeftShoulder")),
                (Get("LeftShoulder"), Get("LeftElbow")),
                (Get("LeftElbow"),    Get("LeftHand")),
                (Get("Neck"),         Get("RightShoulder")),
                (Get("RightShoulder"),Get("RightElbow")),
                (Get("RightElbow"),   Get("RightHand")),
                (Get("Pelvis"),       Get("LeftThigh")),
                (Get("LeftThigh"),    Get("LeftKnee")),
                (Get("LeftKnee"),     Get("LeftLeg")),
                (Get("Pelvis"),       Get("RightThigh")),
                (Get("RightThigh"),   Get("RightKnee")),
                (Get("RightKnee"),    Get("RightLeg")),
            ];
        }
        public enum BoneIds
        {
            Head = 7,
            Neck = 6,
            UpperChest = 23,
            LowerChest = 4,
            Stomach = 3,
            Pelvis = 1,
            LeftShoulder = 9,
            LeftElbow = 10,
            LeftHand = 11,
            RightShoulder = 13,
            RightElbow = 14,
            RightHand = 15,
            LeftThigh = 17,
            LeftKnee = 18,
            LeftLeg = 19,
            LeftFoot = 74,
            RightThigh = 20,
            RightKnee = 21,
            RightLeg = 22,
            RightFoot = 77,
        }

        public enum BoneIds_Full
        {
            Head = 7,
            Neck = 6,
            Neck_Upper = 6,
            UpperChest = 23,
            LowerChest = 4,
            Stomach = 3,
            Pelvis = 1,
            Spine_0 = 2,
            Spine_1 = 3,
            Spine_2 = 4,
            Spine_3 = 23,
            Right_Clavicle = 12,
            Left_Clavicle = 8,
            LeftShoulder = 9,
            Arm_Upper_Left = 9,
            Arm_Upper_Right = 13,
            Arm_Lower_Right = 14,
            Arm_Lower_Left = 10,
            LeftElbow = 10,
            LeftHand = 11,
            RightShoulder = 13,
            RightElbow = 14,
            RightHand = 15,
            LeftThigh = 17,
            LeftKnee = 18,
            LeftLeg = 19,
            LeftFoot = 74,
            RightThigh = 20,
            RightKnee = 21,
            RightLeg = 22,
            RightFoot = 77,
        }

        public enum BoneIds_T
        {
            Head = 7,
            Neck = 6,
            UpperChest = 23,
            LowerChest = 4,
            Stomach = 3,
            Pelvis = 1,
            LeftShoulder = 9,
            LeftElbow = 10,
            LeftHand = 11,
            RightShoulder = 13,
            RightElbow = 14,
            RightHand = 15,
            LeftThigh = 17,
            LeftKnee = 18,
            LeftLeg = 19,
            LeftFoot = 74,
            RightThigh = 20,
            RightKnee = 21,
            RightLeg = 22,
            RightFoot = 77,
        }

        public enum BoneIds_CT
        {
            Head = 7,
            Neck = 6,
            UpperChest = 23,
            LowerChest = 4,
            Stomach = 3,
            Pelvis = 1,
            LeftShoulder = 9,
            LeftElbow = 10,
            LeftHand = 11,
            RightShoulder = 13,
            RightElbow = 14,
            RightHand = 15,
            LeftThigh = 17,
            LeftKnee = 18,
            LeftLeg = 19,
            LeftFoot = 81,
            RightThigh = 20,
            RightKnee = 21,
            RightLeg = 22,
            RightFoot = 86,
        }

        private static Vector4 GetBoneColor(Entity entity)
        {
            bool occluded = visibilityCheck && !entity.Visible;
            bool isTeam = entity.Team == GameState.LocalPlayer.Team;
            Colors c = occluded ? OccludedColors : VisibleColors;

            Vector4 color = (isTeam ? c.TeamRGB : c.EnemyRGB) ? Colors.Rgb(isTeam ? c.TeamColor.W : c.EnemyColor.W) : (isTeam ? c.TeamColor : c.EnemyColor);

            return color;
        }

        public static void DrawBoneLines(Entity? entity, Renderer renderer)
        {
            if (!EnableBoneESP || entity == null || entity.Bones == null ||
                (TeamCheck && entity.Team == GameState.LocalPlayer.Team) ||
                entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Bones == null || entity.Health <= 0 ||
                (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) ||
                entity.Position2D == new Vector2(-99, -99)) return;

            float thickness =
                Math.Clamp(BoneThickness / (entity.Distance * 0.1f), 0.5f,
                    1f); // calculate thickness based on Distance, minimum 0.5f and maximum 2f stops it from being massive

            uint boneColor = ImGui.GetColorU32(GetBoneColor(entity));

            var boneConnections = GetBoneConnections(entity.Team);
            foreach (var (a, b) in boneConnections)
            {
                if (a >= entity.Bones.Count || b >= entity.Bones.Count)
                    continue;

                Bone boneA = entity.Bones[a];
                Bone boneB = entity.Bones[b];
                Vector2 boneAPosition2D = boneA.Position2D;
                Vector2 boneBPosition2D = boneB.Position2D;
                if (boneBPosition2D == new Vector2(-99, -99) || boneAPosition2D == new Vector2(-99, -99))
                    continue;

                if (IsValidScreenPoint(boneAPosition2D) && IsValidScreenPoint(boneBPosition2D))
                {
                    float boneLength = Vector2.Distance(boneAPosition2D, boneBPosition2D);
                    float curve = Math.Clamp(boneLength * 0.15f, 2f, 8f);
                    if (GlowAmount > 0)
                    {
                        switch (CurrentType)
                        {
                            case 0:
                                GlowRenderer.DrawGlowLine(renderer.DrawList, boneAPosition2D, boneBPosition2D,
                                    ImGui.ColorConvertU32ToFloat4(boneColor), GlowAmount,
                                    thickness: thickness);
                                GlowRenderer.DrawGlowCircle(renderer.DrawList, boneAPosition2D, thickness * 2,
                                    ImGui.ColorConvertU32ToFloat4(boneColor),
                                    GlowAmount);
                                break;
                            case 1:
                                GlowRenderer.DrawGlowBezier(renderer.DrawList, boneBPosition2D,
                                    (boneBPosition2D + boneAPosition2D) * 0.5f +
                                    Vector2.Normalize(new Vector2(-(boneAPosition2D - boneBPosition2D).Y,
                                        (boneAPosition2D - boneBPosition2D).X)) * curve,
                                    (boneBPosition2D + boneAPosition2D) * 0.5f +
                                    Vector2.Normalize(new Vector2(-(boneAPosition2D - boneBPosition2D).Y,
                                        (boneAPosition2D - boneBPosition2D).X)) *
                                    (curve * 0.5f), boneAPosition2D, ImGui.ColorConvertU32ToFloat4(boneColor),
                                    GlowAmount / 2, thickness);

                                break;
                        }
                    }

                    switch (CurrentType)
                    {
                        case 0:
                            //renderer.drawList.AddLine(boneAPosition2D, boneBPosition2D, boneColor, thickness);
                            renderer.DrawList.AddLine(boneAPosition2D, boneBPosition2D,
                               boneColor,
                                thickness); //draw a line between the Bones
                            renderer.DrawList.AddCircleFilled(boneAPosition2D, thickness * 1.5f,
                                boneColor); //draw a circle at the start bone
                            break;
                        case 1:
                            renderer.DrawList.AddBezierCubic(boneBPosition2D,
                                (boneBPosition2D + boneAPosition2D) * 0.5f +
                                Vector2.Normalize(new Vector2(-(boneAPosition2D - boneBPosition2D).Y,
                                    (boneAPosition2D - boneBPosition2D).X)) * curve,
                                (boneBPosition2D + boneAPosition2D) * 0.5f +
                                Vector2.Normalize(new Vector2(-(boneAPosition2D - boneBPosition2D).Y,
                                    (boneAPosition2D - boneBPosition2D).X)) * (curve * 0.5f),
                                boneAPosition2D, boneColor, thickness);
                            break;
                    }
                }
            }


            var head = entity.Bones[(int)BoneIds.Head];
            Vector2 headPos = head.Position2D;
            if (!IsValidScreenPoint(headPos))
                return;

            float radius = Math.Clamp(10f / (entity.Distance * 0.05f), 3f, 10f);

            if (GlowAmount > 0)
                GlowRenderer.DrawGlowCircle(renderer.DrawList, headPos, radius, GetBoneColor(entity), GlowAmount);

            renderer.DrawList.AddCircleFilled(headPos, radius, boneColor); // draw a circle at the Head bone extra big
            //DebugBones(entity, renderer);
        }

        private static void DebugBones(Entity entity, Renderer renderer)
        {
            for (int i = 0; i < entity.Bones.Count; i++)
            {
                var b = entity.Bones[i];
                if (b.Position2D == new Vector2(-99, -99))
                    continue;

                var p = b.Position2D;
                if (!IsValidScreenPoint(p))
                    continue;

                renderer.DrawList.AddText(p, 0xFFFFFFFF, i.ToString());
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
        public static void DrawBonePreview(Vector2 Center, float entityHeight)
        {
            Vector4 Color = VisibleColors.EnemyRGB ? Colors.Rgb(VisibleColors.EnemyColor.W) : VisibleColors.EnemyColor;
            uint UColor = ImGui.GetColorU32(Color);
            var DrawList = ImGui.GetWindowDrawList();

            Vector2 Neck = Center + new Vector2(0, -entityHeight / 3);
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
            Vector2 Head = Neck - new Vector2(0, 8);

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
                    GlowRenderer.DrawGlowLine(DrawList, A, B, Color, GlowAmount * 0.5f);

                DrawList.AddLine(A, B, UColor, BoneThickness * 0.5f);
                DrawList.AddCircleFilled(A, BoneThickness * 0.5f, UColor);
            }

            if (GlowAmount > 0)
                GlowRenderer.DrawGlowCircle(DrawList, Head, 10f, Color, GlowAmount);

            DrawList.AddCircleFilled(Head, 5f, UColor, 12);
        }
    }
}
