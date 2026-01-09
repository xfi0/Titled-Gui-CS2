using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class EyeRay
    {
        public static float Length = 50f;
        public static bool DrawOnTeam = true;
        public static bool Enabled = false;
        public static Vector4 EyeRayColor = new(1, 0, 0, 1);
        public static void DrawEyeRay()
        {
            try
            {
                foreach (Entity e in GameState.Entities)
                {
                    if (e == null || e.Bones2D == null || !DrawOnTeam && e.Team == GameState.LocalPlayer.Team || BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed) return;

                    Vector2 Head = e.Bones2D[2];

                    float Yaw = e.AngEyeAngles.Y * (MathF.PI / 180.0f);

                    float dx = MathF.Cos(Yaw) * Length;
                    float dy = -MathF.Sin(Yaw) * Length;


                    float ClampedLength = Math.Clamp(Length, 0.3f, 0.6f); 

                    Vector2 End = new(Head.X + dx * ClampedLength, Head.Y + dy * ClampedLength);

                    if (End == new Vector2(-99, -99)) continue;

                    GameState.renderer.drawList.AddLine(Head, End, ImGui.ColorConvertFloat4ToU32(EyeRayColor));
                    //Console.WriteLine("DRAWING" + Head + " " + End);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


    }
}
