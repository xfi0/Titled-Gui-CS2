using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;

namespace Titled_Gui.Modules.Legit
{
    internal class HitStuff : Classes.ThreadService // could use some settings
    {
        public static Dictionary<int, (string Display, string File)> HitSounds = new()
        {
            { 0, ("Never Lose", "NeverLose.wav") },
            { 1, ("Skeet",      "Skeet.wav")     },
            { 2, ("Hit Marker", "CODHitmarker.wav")},
        };
        public static List<string> HitSoundDisplays = HitSounds.Values.Select(s => s.Display).ToList();

        public static int CurrentHitSound = 0;
        public static bool EnableHitSounds = false;
        public static bool EnableHeadshotText = false;
        public static int PreviousDamage = 0;
        public static int PreviousHeadshots = 0;
        public static float Volume = 1.0f;
        public static Vector4 TextColor = new(1f, 1f, 1f, 1f);

        public static readonly List<HitText> Texts = [];

        public static void Update()
        {
            if (GameState.memory == null || GameState.renderer == null)
                return;

            GameState.LocalController = GameState.memory.ReadPointer(GameState.client + Offsets.dwLocalPlayerController);
            GameState.ActionTrackingServices = GameState.memory.ReadPointer(GameState.LocalController, Offsets.m_pActionTrackingServices);
            GameState.RoundHeadshots = GameState.memory.ReadInt(GameState.ActionTrackingServices + Offsets.m_iNumRoundKillsHeadshots);
            GameState.RoundDamage = GameState.memory.ReadInt(GameState.ActionTrackingServices + Offsets.m_flTotalRoundDamageDealt);

            for (int i = HitSounds.Count; i < HitSoundDisplays.Count; i++)
                HitSounds[i] = (HitSoundDisplays[i], HitSoundDisplays[i]);

            if (GameState.RoundDamage > PreviousDamage)
            {
                if (HitSounds.TryGetValue(CurrentHitSound, out var sound))
                    PlaySound(sound.File);

                PreviousDamage = GameState.RoundDamage;
            }
            if (EnableHeadshotText && GameState.RoundHeadshots > PreviousHeadshots)
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

        private static void PlaySound(string soundName)
        {
            if (!File.Exists(soundName)) // if its embedded
                Classes.PlaySound.PlaySoundFileEmbedded(soundName, "hitsounds.", Volume);
            else // if its not embedded
                Classes.PlaySound.PlaySoundWithCheck(soundName, Volume);
        }

        public static void CreateHitText()
        {
            if (GameState.renderer == null)
                return;

            ImGui.PushFont(Renderer.TextFont48);
            foreach (HitText hitText in Texts.ToList())
            {
                if (DateTime.Now > hitText.ExpireAt)
                {
                    Texts.Remove(hitText);
                    continue;
                }

                hitText.State += 1f;

                float X = hitText.BasePosition.X + 100f * MathF.Sin(hitText.State / 50f) - 50f;
                float Y = hitText.BasePosition.Y - 50f + -(hitText.State * 2);

                Vector2 textPos = new(X, Y);

                float lifeTime = (float)(hitText.ExpireAt - DateTime.Now).TotalMilliseconds;
                float totalLife = 1500f; // 1.5 s
                float alpha = Math.Clamp(1f - ((totalLife - lifeTime) / totalLife), 0.1f, 1f);

                Vector4 TextColorAdjusted = new(TextColor.X, TextColor.Y, TextColor.Z, alpha);

                GameState.renderer.DrawList.AddText(textPos, ImGui.ColorConvertFloat4ToU32(TextColorAdjusted), hitText.Text);
            }
            ImGui.PopFont();
        }


        protected override void FrameAction()
        {
            if (!EnableHitSounds && !EnableHeadshotText) return;

            Update();
            Thread.Sleep(15);
        }
    }
}
