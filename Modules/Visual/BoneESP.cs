using ImGuiNET;
using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{
    public static class BoneESP
    {
        public static bool AimOnTeam = false;
        private static readonly (int, int)[] BoneConnections = new (int, int)[]
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

        public static bool EnableBoneESP = false; // toggle for bone ESP
        public static void DrawBoneLines(Entity entity, Renderer renderer)
        {
            if (entity == null || entity.bones2D == null)
                return;

            float thickness = Math.Clamp(Renderer.BoneThickness / (entity.distance * 0.1f), 0.5f, 2f);
            uint boneColor = ImGui.GetColorU32(Renderer.BoneColor);

            foreach (var (a, b) in BoneConnections)
            {
                if (a >= entity.bones2D.Count || b >= entity.bones2D.Count)
                    continue;

                Vector2 boneA = entity.bones2D[a];
                Vector2 boneB = entity.bones2D[b];

                if (IsValidScreenPoint(boneA) && IsValidScreenPoint(boneB))
                {
                    renderer.drawList.AddLine(boneA, boneB, boneColor, thickness);
                }
            }

            if ((int)BoneIds.Head < entity.bones2D.Count)
            {
                Vector2 headPos = entity.bones2D[2];
                if (IsValidScreenPoint(headPos))
                {
                    float radius = Math.Clamp(8f / (entity.distance * 0.05f), 3f, 10f); //circle radius
                    renderer.drawList.AddCircleFilled(headPos, radius, boneColor); //head bone
                }
            }
        }


        //this was a null check that i thought confirmed why it was broken but no it was a wrong offset!
        private static bool IsValidScreenPoint(Vector2 pt)
        {
            return !float.IsNaN(pt.X) && !float.IsNaN(pt.Y) &&
                   !float.IsInfinity(pt.X) && !float.IsInfinity(pt.Y) &&
                   pt.X > 0 && pt.Y > 0;
        }
    }
}
