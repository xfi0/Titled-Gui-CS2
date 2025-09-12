using ImGuiNET;
using SharpGen.Runtime.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui.Classes;
using Titled_Gui.Data;

namespace Titled_Gui.Modules.Visual
{
    internal class EyeRay
    {
        public static float Length = 50f;
        public static bool DrawOnSelf = false;
        public static bool DrawOnTeam = true;
        public static bool Enabled = false;
        public static Vector4 EyeRayColor = new(1, 0 , 0, 1);
        public static void DrawEyeRay()
        {
            foreach (Entity e in GameState.Entities)
            {
                if (e == null || (!DrawOnSelf && e.PawnAddress == GameState.localPlayer.PawnAddress) || (!DrawOnTeam && e.Team == GameState.localPlayer.Team) || e.Health == 0) continue;

                var Head = e.Bones2D![(int)BoneESP.BoneIds.Head];

                float Yaw = e.ViewAngles.Y * (float)(Math.PI / 180.0);
                float dx = Yaw * Length;
                float dy = Yaw * Length;

                Vector2 End = new(Head.X + dx, Head.Y + dy);

                GameState.renderer.drawList.AddLine(Head, End, ImGui.ColorConvertFloat4ToU32(EyeRayColor));
            }
        }


    }
}
