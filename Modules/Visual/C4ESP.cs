using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class C4ESP
    {
        public static bool BoxEnabled = false;
        public static bool TextEnabled = false;
        public static Vector4 BoxColor = new(1, 1, 1, 1);
        public static Vector4 TextColor = new(1, 1, 1, 1);

        public static IntPtr GetPlanted()
        {
            IntPtr plantedPointer = GameState.swed.ReadPointer(GameState.client + Offsets.dwPlantedC4);
            if (plantedPointer == IntPtr.Zero) 
                return plantedPointer == IntPtr.Zero ? IntPtr.Zero : plantedPointer;

            return GameState.swed.ReadPointer(plantedPointer);
        }

        private static IntPtr GetNode()
        {
            IntPtr planted = GetPlanted();
            if (planted == IntPtr.Zero) return IntPtr.Zero;

            return GameState.swed.ReadPointer(planted + Offsets.m_pGameSceneNode);
        }

        private static Vector3 GetPos()
        {
            IntPtr node = GetNode();
            if (node == IntPtr.Zero) return new Vector3(0, 0, 0);

            return GameState.swed.ReadVec(node + Offsets.m_vecAbsOrigin);
        }

        public static void DrawESP()
        {
            if (!BoxEnabled && !TextEnabled) return;

            Vector3 position = GetPos();
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            Vector2 position2D = Calculate.WorldToScreen(viewMatrix, position);

            if (position == new Vector3(0f, 0f, 0f) || GetNode() == IntPtr.Zero || GetPlanted() == IntPtr.Zero) return;

            if (TextEnabled)
                GameState.renderer.drawList.AddText(position2D, ImGui.ColorConvertFloat4ToU32(TextColor), "C4");

            if (BoxEnabled)
            {
                GameState.renderer.drawList.AddRect(position2D, new(
                        position2D.X + 10 *
                        (float)Math.Clamp(Vector2.Distance(position2D, GameState.LocalPlayer.Position2D), 1.5, 10),
                        position2D.Y + 10), ImGui.ColorConvertFloat4ToU32(BoxColor));
            }
        }

        private static Vector3[] Get3DCorners(Vector3 position, Vector3 vecMin, Vector3 vecMax)
        {
            if (float.IsNaN(vecMin.X) || float.IsNaN(vecMin.Y) || float.IsNaN(vecMin.Z))
                return Array.Empty<Vector3>();

            return
            [
                position + new Vector3(vecMin.X, vecMin.Y, vecMin.Z),
                position + new Vector3(vecMax.X, vecMin.Y, vecMin.Z),
                position + new Vector3(vecMin.X, vecMax.Y, vecMin.Z),
                position + new Vector3(vecMax.X, vecMax.Y, vecMin.Z),
                position + new Vector3(vecMin.X, vecMin.Y, vecMax.Z),
                position + new Vector3(vecMax.X, vecMin.Y, vecMax.Z),
                position + new Vector3(vecMin.X, vecMax.Y, vecMax.Z),
                position + new Vector3(vecMax.X, vecMax.Y, vecMax.Z)
            ];
        }
    }
}
