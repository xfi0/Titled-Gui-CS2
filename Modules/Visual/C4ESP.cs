using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using Titled_Gui.Classes;
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
            IntPtr ptrToPlanted = GameState.swed.ReadPointer(GameState.client + Offsets.dwPlantedC4);
            if (ptrToPlanted == IntPtr.Zero) return IntPtr.Zero;

            return GameState.swed.ReadPointer(ptrToPlanted);
        }

        public static IntPtr GetNode()
        {
            IntPtr planted = GetPlanted();
            if (planted == IntPtr.Zero) return IntPtr.Zero;

            return GameState.swed.ReadPointer(planted + Offsets.m_pGameSceneNode);
        }

        public static Vector3 GetPos()
        {
            IntPtr node = GetNode();
            if (node == IntPtr.Zero) return new Vector3(0, 0, 0);

            return GameState.swed.ReadVec(node + Offsets.m_vecAbsOrigin);
        }
        public static void DrawESP()
        {
            if (!BoxEnabled && !TextEnabled) return;

            Vector3 Position = GetPos();
            float[] ViewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            Vector2 Position2D = Calculate.WorldToScreen(ViewMatrix, Position, GameState.renderer.screenSize);

            if (Position == new Vector3(0f, 0f, 0f) || GetNode() == IntPtr.Zero || GetPlanted() == IntPtr.Zero) return;

            if (TextEnabled)
                GameState.renderer.drawList.AddText(Position2D, ImGui.ColorConvertFloat4ToU32(TextColor), "C4");

            if (BoxEnabled)
                GameState.renderer.drawList.AddRect(Position2D, new(Position2D.X + 10, Position2D.Y + 10), ImGui.ColorConvertFloat4ToU32(BoxColor));
        }
    }
}
