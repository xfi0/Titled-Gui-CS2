using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    internal class HitStuff : Classes.ThreadService // could use some settings
    {
        public static List<string> HitSounds = new()
        {
            "Never Lose", 
            "Skeet"
        };
        public static int CurrentHitSound = 0;
        public static bool Enabled = false;
        public static bool EnableHeadshotText = false;
        public static int PreviousDamage = 0;
        public static int PreviousHeadshots = 0;
        public static float Volume = 1.0f;
        public static Vector4 TextColor = new(1f, 1f, 1f, 1f);

        public class HitText
        {
            public string? Text { get; set; }
            public DateTime ExpireAt { get; set; }
            public Vector2 Position { get; set; }
            public Vector2 BasePosition { get; set; } 
            public float State { get; set; } = 0f;
        }
        public enum HitAnimation
        {
            Sin = 0,
            Fade = 1,
        }
        public static readonly List<HitText> Texts = [];

        public static void Update()
        {
            GameState.LocalController = GameState.swed.ReadPointer(GameState.client + Offsets.dwLocalPlayerController);
            GameState.ActionTrackingServices = GameState.swed.ReadPointer(GameState.LocalController, Offsets.m_pActionTrackingServices);
            GameState.RoundHeadshots = GameState.swed.ReadInt(GameState.ActionTrackingServices + Offsets.m_iNumRoundKillsHeadshots);
            GameState.RoundDamage = GameState.swed.ReadInt(GameState.ActionTrackingServices + Offsets.m_flTotalRoundDamageDealt);

            if (GameState.RoundDamage > PreviousDamage)
            {
                PlaySound(HitSounds[CurrentHitSound]);
                PreviousDamage = GameState.RoundDamage;
                //Console.Write("Hit");
            }
            if (GameState.RoundHeadshots > PreviousHeadshots)
            {
                Vector2 textPos = new Vector2(GameState.renderer.ScreenSize.X / 2, GameState.renderer.ScreenSize.Y / 2);
                Texts.Add(new HitText
                {
                    Text = "HEADSHOT",
                    ExpireAt = DateTime.Now.AddSeconds(1.5),
                    Position = textPos,
                    BasePosition = textPos
                });
                PreviousHeadshots = GameState.RoundHeadshots;
            }
        }

        private static void PlaySound(string soundName) => Classes.PlaySound.PlaySoundWithCheck(soundName, Volume);
        
        public static void CreateHitText()
        {
            ImGui.PushFont(Renderer.TextFont48);
            foreach (HitText hitText in Texts.ToList())
            {
                if (DateTime.Now > hitText.ExpireAt)
                {
                    Texts.Remove(hitText); continue;
                }

                hitText.State += 1f;

                float X = hitText.BasePosition.X + 100f * MathF.Sin(hitText.State / 50f) - 50f;
                float Y = hitText.BasePosition.Y - 50f + -(hitText.State * 2);

                Vector2 textPos = new(X, Y);

                float lifeTime = (float)(hitText.ExpireAt - DateTime.Now).TotalMilliseconds;
                float totalLife = 1500f; // 1.5 s
                float alpha = Math.Clamp(1f - ((totalLife - lifeTime) / totalLife), 0.1f, 1f);

                Vector4 TextColorAdjusted = new(TextColor.X, TextColor.Y, TextColor.Z, alpha);

                GameState.renderer.drawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(TextColorAdjusted), hitText.Text);
            }
            ImGui.PopFont();
        }


        protected override void FrameAction()
        {
            if (!Enabled && !EnableHeadshotText) return;

            Update();
            Thread.Sleep(15);
        }
    }
}
