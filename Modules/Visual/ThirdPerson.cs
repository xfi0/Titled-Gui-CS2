using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class ThirdPerson : Classes.ThreadService // THIS WILL GET YOU BANNED USE AT YOUR OWN RISK https://www.unknowncheats.me/forum/counter-strike-2-a/710298-third-person-external.html
    {
        public static bool Enabled = false;
        private static bool patchApplied;
        private static IntPtr cachedlp;
        static IntPtr jnePatch = 0x7E3697;
        static byte[] originalJNE = { 0x75, 0x10 }; // something idk
        static byte[] patchedJNE = { 0xEB, 0x10 };
        public static void Run(IntPtr currentPawn)
        {
            if (NeedsReapply(currentPawn))
            {
                cachedlp = currentPawn;
            }

            if (Enabled)
            {
                ApplyPatch();
                SetThirdPersonState(true);
            }
            else if (!Enabled)
            {
                SetThirdPersonState(false);
                RemovePatch();
            }
        }

        private static void ApplyPatch()
        {
            if (patchApplied)
                return;

            GameState.swed.WriteBytes(GameState.client + jnePatch, patchedJNE);
            patchApplied = true;
        }

        private static void RemovePatch()
        {
            if (!patchApplied)
                return;

            GameState.swed.WriteBytes(GameState.client + jnePatch, originalJNE);
            patchApplied = false;
        }

        private static void SetThirdPersonState(bool enabled)
        {
            GameState.swed.WriteInt(GameState.client + Offsets.dwCSGOInput + 0x251, enabled ? 256 : 0);
        }

        private static bool NeedsReapply(IntPtr currentPawn)
        {
            return currentPawn != cachedlp && currentPawn != IntPtr.Zero;
        }

        protected override void FrameAction()
        {
            if (!Enabled) return;

            ThirdPerson.Run(GameState.LocalPlayerPawn);
        }
    }
}