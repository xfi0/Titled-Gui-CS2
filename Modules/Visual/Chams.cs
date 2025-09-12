using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using Titled_Gui.Data;
using Titled_Gui.Classes;

namespace Titled_Gui.Modules.Visual
{
    internal class Chams
    {
        public static float BoneThickness = 10f;
        public static bool DrawOnSelf = false;
        public static bool EnableChams = false;

        public static void DrawChams(Entity entity)
        {
            if (entity == null || entity.Bones2D == null || (DrawOnSelf && entity == GameState.localPlayer)) return;

            float DistanceFactor = entity.Distance * 0.05f;
            float thickness = Math.Clamp(30f / DistanceFactor, 20f, 30f); // much thicker to mimic body

            uint boneColor = ImGui.GetColorU32(Colors.RGB ? Colors.Rgb(0.5f) : Colors.BoneColor);
            uint backColor = ImGui.GetColorU32(new System.Numerics.Vector4(0f, 0f, 0f, 0.4f)); // back black outline

            foreach (var (a, b) in BoneESP.BoneConnections)
            {
                if (a >= entity?.Bones2D?.Count || b >= entity?.Bones2D?.Count || entity != null)
                    continue;

                Vector2 boneA = entity.Bones2D[a];
                Vector2 boneB = entity.Bones2D[b];

                if (BoneESP.IsValidScreenPoint(boneA) && BoneESP.IsValidScreenPoint(boneB))
                {
                    GameState.renderer.drawList.AddLine(boneA, boneB, backColor, thickness + 2f);
                    GameState.renderer.drawList.AddLine(boneA, boneB, boneColor, thickness);

                    //fake rounded look
                    float jointRadius = thickness / 2f;
                    //renderer.drawList.AddCircleFilled(boneA, jointRadius + 1.5f, backColor);
                    //renderer.drawList.AddCircleFilled(boneB, jointRadius + 1.5f, backColor);
                    GameState.renderer.drawList.AddCircleFilled(boneA, jointRadius, boneColor);
                    GameState.renderer.drawList.AddCircleFilled(boneB, jointRadius, boneColor);
                }
            }


            if ((int)BoneESP.BoneIds.Head < entity?.Bones2D?.Count)
            {
                Vector2 HeadPos = entity.Bones2D[2];
                if (BoneESP.IsValidScreenPoint(HeadPos))
                {
                    float radius = Math.Clamp(20f / (entity.Distance * 0.05f), 6f, 20f);
                    GameState.renderer.drawList.AddCircleFilled(HeadPos, radius + 2f, backColor); // black outline
                    GameState.renderer.drawList.AddCircleFilled(HeadPos, radius, boneColor);      // inner
                }
            }
        }
    }
}
