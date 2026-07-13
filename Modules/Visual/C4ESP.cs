using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Math;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Game.C4;

namespace Titled_Gui.Modules.Visual
{
    internal class C4ESP
    {
        public static bool Enabled = false;
        public static bool TextEnabled = false;
        private static Vector4 _boxColor = new(1, 1, 1, 1);
        private static Vector4 _textColor = new(1, 1, 1, 1);
        public static Colors Colors = new Colors(primaryColor: _boxColor, secondaryColor: _textColor);
        public static void DrawESP()
        {
            if (!Enabled) return;

            C4? c4 = C4Info.C4;

            if (c4 == null || !c4.Planted || c4.Position == new Vector3(0, 0, 0) || c4.Position2D == new Vector2(-99, -99))
                return;

            float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
            Vector3[] corners3D = Get3DCorners(c4);
            Vector2[] corners2D = new Vector2[corners3D.Length];
            Vector4 textColor = Colors.SecondaryRGB ? Colors.Rgb(Colors.SecondaryColor.W) : Colors.SecondaryColor;
            Vector4 boxColor = Colors.PrimaryRGB ? Colors.Rgb(Colors.PrimaryColor.W) : Colors.PrimaryColor
                ;
            for (int i = 0; i < corners3D.Length; i++)
            {
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99))
                    return;
            }

            GameState.renderer.DrawList.AddText(c4.Position2D, ImGui.ColorConvertFloat4ToU32(textColor), "C4");
            WorldESP.Draw3DBoxESPFromMatrix(corners2D, ImGui.ColorConvertFloat4ToU32(boxColor), false, 2);
        }

        private static Vector3[] Get3DCorners(C4? c4)
        {
            if (c4 == null || c4.Matrix == null || c4.Matrix.Length < 6)
                return Array.Empty<Vector3>();

            float cos = c4.Matrix[6];
            float sin = c4.Matrix[7];

            Vector3 vecMin = new(-4f, -5.8f, -2f);
            Vector3 vecMax = new(4f, 5.8f, 2f);

            return
            [
                RotateCorner(c4.Position, vecMin.X, vecMin.Y, vecMin.Z, cos, sin),
                RotateCorner(c4.Position, vecMax.X, vecMin.Y, vecMin.Z, cos, sin),
                RotateCorner(c4.Position, vecMin.X, vecMax.Y, vecMin.Z, cos, sin),
                RotateCorner(c4.Position, vecMax.X, vecMax.Y, vecMin.Z, cos, sin),
                RotateCorner(c4.Position, vecMin.X, vecMin.Y, vecMax.Z, cos, sin),
                RotateCorner(c4.Position, vecMax.X, vecMin.Y, vecMax.Z, cos, sin),
                RotateCorner(c4.Position, vecMin.X, vecMax.Y, vecMax.Z, cos, sin),
                RotateCorner(c4.Position, vecMax.X, vecMax.Y, vecMax.Z, cos, sin),
            ];
        }
        private static Vector3 RotateCorner(Vector3 origin, float x, float y, float z, float cos, float sin)
        {
            return new Vector3(
                origin.X + x * cos - y * sin,
                origin.Y + x * sin + y * cos,
                origin.Z + z
            );
        }
    }
}
