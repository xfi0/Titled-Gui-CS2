using ImGuiNET;
using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;
using Titled_Gui.ModuleHelpers;

namespace Titled_Gui.Modules.Visual
{
    public static class BoneESP
    {
        public static float BoneThickness = 5f;
        public static bool DrawOnSelf = false;
        public static readonly (int, int)[] BoneConnections = new (int, int)[]
        {
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

        public static bool EnableBoneESP = false;
        public static void DrawBoneLines(Entity entity, Renderer renderer)
        {
            if (entity == null || entity.bones2D == null)
                return;

            float thickness = Math.Clamp(BoneESP.BoneThickness / (entity.distance * 0.1f), 0.5f, 2f); // calculate thickness based on distance, minimum 0.5f and maximum 2f stops it from being massive
            uint boneColor = ImGui.GetColorU32(Colors.RGB ? Colors.Rgb(0.5f) : Colors.BoneColor); //get color

            foreach (var (a, b) in BoneConnections)
            {
                if (a >= entity.bones2D.Count || b >= entity.bones2D.Count)
                    continue;

                Vector2 boneA = entity.bones2D[a];
                Vector2 boneB = entity.bones2D[b];

                if (IsValidScreenPoint(boneA) && IsValidScreenPoint(boneB))
                {
                    renderer.drawList.AddLine(boneA, boneB, boneColor, thickness); //draw a line between the bones
                    renderer.drawList.AddCircleFilled(boneA, thickness * 2, boneColor); //draw a circle at the start bone
                }
            }

            if ((int)BoneIds.Head < entity.bones2D.Count)
            {
                Vector2 headPos = entity.bones2D[2];
                if (IsValidScreenPoint(headPos))
                {
                    float radius = Math.Clamp(10f / (entity.distance * 0.05f), 3f, 10f);
                    renderer.drawList.AddCircleFilled(headPos, radius, boneColor); // draw a circle at the head bone extra big
                }
            }
        }

        public static bool IsValidScreenPoint(Vector2 pt)
        {
            return !float.IsNaN(pt.X) && !float.IsNaN(pt.Y) &&
                   !float.IsInfinity(pt.X) && !float.IsInfinity(pt.Y) &&
                   pt.X > 0 && pt.Y > 0;
        }
    }
}