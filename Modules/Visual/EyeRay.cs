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
                    if (e == null || e.Bones == null || GameState.LocalPlayer == null || GameState.renderer == null || e.Health <= 0 || (!DrawOnTeam && e.Team == GameState.LocalPlayer.Team) || (BoxESP.FlashCheck && GameState.LocalPlayer.IsFlashed))
                        continue;

                    Vector2 head = e.Bones[(int)BoneESP.BoneIds.Head].Position2D;
                    if (head == Vector2.Zero || head == new Vector2(-99, -99))
                        continue;

                    float yaw = e.AngEyeAngles.Y * (MathF.PI / 180.0f);

                    float dx = MathF.Cos(yaw) * Length;
                    float dy = -MathF.Sin(yaw) * Length;


                    float clampedLength = Math.Clamp(Length, 0.3f, 0.6f);

                    Vector2 end = new(head.X + dx * clampedLength, head.Y + dy * clampedLength);

                    if (end == new Vector2(-99, -99))
                        continue;

                    GameState.renderer.DrawList.AddLine(head, end, ImGui.ColorConvertFloat4ToU32(EyeRayColor));
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
