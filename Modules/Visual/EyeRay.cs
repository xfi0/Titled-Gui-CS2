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
                foreach (Entity? e in GameState.Entities)
                {
                    if (e == null || e.Bones2D == null || (!DrawOnTeam && e.Team == GameState.LocalPlayer.Team) || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed)) 
                        continue;

                    Vector2 head = e.Bones2D[2];

                    float yaw = e.AngEyeAngles.Y * (MathF.PI / 180.0f);

                    float dx = MathF.Cos(yaw) * Length;
                    float dy = -MathF.Sin(yaw) * Length;


                    float clampedLength = Math.Clamp(Length, 0.3f, 0.6f); 

                    Vector2 end = new(head.X + dx * clampedLength, head.Y + dy * clampedLength);

                    if (end == new Vector2(-99, -99)) 
                        continue;

                    GameState.renderer.drawList.AddLine(head, end, ImGui.ColorConvertFloat4ToU32(EyeRayColor));
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
