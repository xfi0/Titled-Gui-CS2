using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui.Modules.Visual
{
    internal class Chams
    {
        public static float BoneThickness = 10f;
        public static bool DrawOnSelf = false;

        public static readonly (int, int)[] BoneConnections = new (int, int)[]
        {
            (0, 1),   // Waist to Neck
            (1, 2),   // Neck to Head
            (1, 3),   // Neck to ShoulderLeft
            (3, 4),   // ShoulderLeft to ForeLeft
            (4, 5),   // ForeLeft to HandLeft
            (1, 6),   // Neck to ShoulderRight
            (6, 7),   // ShoulderRight to ForeRight
            (7, 8),   // ForeRight to HandRight
            (0, 9),   // Waist to KneeLeft
            (9, 10),  // KneeLeft to FeetLeft
            (0, 11),  // Waist to KneeRight
            (11, 12), // KneeRight to FeetRight
        };

        public enum BoneIds
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

        public static bool EnableChams = false;

        public static void DrawChams(Entity entity, Renderer renderer)
        {
            if (entity == null || entity.bones2D == null)
                return;

            float distanceFactor = entity.distance * 0.05f;
            float thickness = Math.Clamp(30f / distanceFactor, 20f, 30f); // much thicker to mimic body

            uint boneColor = ImGui.GetColorU32(Colors.RGB ? Colors.Rgb(0.5f) : Colors.BoneColor);
            uint backColor = ImGui.GetColorU32(new System.Numerics.Vector4(0f, 0f, 0f, 0.4f)); // back black outline

            foreach (var (a, b) in BoneConnections)
            {
                if (a >= entity.bones2D.Count || b >= entity.bones2D.Count)
                    continue;

                Vector2 boneA = entity.bones2D[a];
                Vector2 boneB = entity.bones2D[b];

                if (IsValidScreenPoint(boneA) && IsValidScreenPoint(boneB))
                {
                    renderer.drawList.AddLine(boneA, boneB, backColor, thickness + 2f);
                    renderer.drawList.AddLine(boneA, boneB, boneColor, thickness);

                    //fake rounded look nvm
                    float jointRadius = thickness / 2f;
                    //renderer.drawList.AddCircleFilled(boneA, jointRadius + 1.5f, backColor);
                    //renderer.drawList.AddCircleFilled(boneB, jointRadius + 1.5f, backColor);
                    renderer.drawList.AddCircleFilled(boneA, jointRadius, boneColor);
                    renderer.drawList.AddCircleFilled(boneB, jointRadius, boneColor);
                }
            }


            if ((int)BoneIds.Head < entity.bones2D.Count)
            {
                Vector2 headPos = entity.bones2D[2];
                if (IsValidScreenPoint(headPos))
                {
                    float radius = Math.Clamp(20f / (entity.distance * 0.05f), 6f, 20f);
                    renderer.drawList.AddCircleFilled(headPos, radius + 2f, backColor); // black outline
                    renderer.drawList.AddCircleFilled(headPos, radius, boneColor);      // inner
                }
            }
        }

        public static bool IsValidScreenPoint(Vector2 pt)
        {
            return !float.IsNaN(pt.X) && !float.IsNaN(pt.Y) && !float.IsInfinity(pt.X) && !float.IsInfinity(pt.Y) && pt.X > 0 && pt.Y > 0;
        }
    }
}
