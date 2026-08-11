using System.Numerics;

namespace Titled_Gui.Data.Menu.Types
{
    public class Colors(Vector4? teamColor = null, Vector4? enemyColor = null, Vector4? primaryColor = null, Vector4? secondaryColor = null, bool primaryRGB = false, bool secondaryRGB = false, bool teamRGB = false, bool enemyRGB = false) // only use enemy and team for visuals, and primary and secondary for other things like menu colors. theyre the same, just easier to read.
    {
        public Vector4 TeamColor = teamColor ?? new Vector4(0f, 1f, 0f, 1f);
        public Vector4 EnemyColor = enemyColor ?? new Vector4(1f, 0f, 0f, 1f);
        public bool TeamRGB = teamRGB;
        public bool EnemyRGB = enemyRGB;

        public Vector4 PrimaryColor = primaryColor ?? new Vector4(0f, 1f, 0f, 1f);
        public Vector4 SecondaryColor = secondaryColor ?? new Vector4(1f, 0f, 0f, 1f);
        public bool PrimaryRGB = primaryRGB;
        public bool SecondaryRGB = secondaryRGB;

        public static float RGBSpeed = 1f;

        public static Vector4 Rgb(float alpha)
        {
            float time = (float)DateTime.Now.TimeOfDay.TotalSeconds * RGBSpeed;
            float t = time % 1f;
            float r, g, b;

            if (t < 0.333f)
            {
                r = 1f - t * 3f;
                g = t * 3f;
                b = 0f;
            }
            else if (t < 0.666f)
            {
                r = 0f;
                g = 1f - (t - 0.333f) * 3f;
                b = (t - 0.333f) * 3f;
            }
            else
            {
                r = (t - 0.666f) * 3f;
                g = 0f;
                b = 1f - (t - 0.666f) * 3f;
            }

            return new Vector4(r, g, b, alpha);
        }
    }
}