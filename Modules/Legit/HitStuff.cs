using ImGuiNET;
using NAudio.Wave;
using System.Numerics;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Legit
{
    internal class HitStuff : Classes.ThreadService // could usesome settings
    {
        public static string[] HitSounds = new string[] { "Never Lose", "Skeet" };
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
        public static readonly List<HitText> Texts = new();

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
                Vector2 TextPos = new Vector2(GameState.renderer.screenSize.X / 2, GameState.renderer.screenSize.Y / 2);
                Texts.Add(new HitText
                {
                    Text = "HEADSHOT",
                    ExpireAt = DateTime.Now.AddSeconds(1.5),
                    Position = TextPos,
                    BasePosition = TextPos
                });
                PreviousHeadshots = GameState.RoundHeadshots;
            }
        }

        private static void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName)) return;

            try
            {
                string path = Path.Combine("..", "..", "..", "..", "Resources", $"{soundName.Replace(" ", "")}.wav");

                AudioFileReader file = new(path);
                WaveOutEvent player = new();
                player.Init(file);
                player.Volume = Volume;
                player.Play();

                player.PlaybackStopped += (s, e) =>
                {
                    file.Dispose();
                    player.Dispose();
                };
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File Was Not Found.");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
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

                Vector2 TextPos = new(X, Y);

                float LifeTime = (float)(hitText.ExpireAt - DateTime.Now).TotalMilliseconds;
                float totalLife = 1500f; // 1.5 s
                float alpha = Math.Clamp(1f - ((totalLife - LifeTime) / totalLife), 0.1f, 1f);

                var TextColorAdjusted = new Vector4(TextColor.X, TextColor.Y, TextColor.Z, alpha);

                GameState.renderer.drawList.AddText(TextPos, ImGui.ColorConvertFloat4ToU32(TextColorAdjusted), hitText.Text);
            }
            ImGui.PopFont();
        }


        protected override async void FrameAction()
        {
            if (!Enabled && !EnableHeadshotText) return;

            Update();
            await Task.Delay(15);
        }
    }
}
