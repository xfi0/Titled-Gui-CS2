using System;
using System.Collections.Generic;
using System.Text;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class ThirdPerson : Classes.ThreadService // THIS WILL GET YOU BANNED USE AT YOUR OWN RISK https://www.unknowncheats.me/forum/counter-strike-2-a/710298-third-person-external.html
    {
        public static bool enabled = false;
        private static bool patchApplied;
        private static IntPtr cachedlp;
        static IntPtr jnePatch = 0x7E3697;
        static byte[] originalJNE = { 0x75, 0x10 }; // something idk
        static byte[] patchedJNE = { 0x90, 0x90}; // nop nop
        public static void Run(IntPtr currentPawn, bool enabledb)
        {
            if (NeedsReapply(currentPawn))
            {
                enabled = false;
                cachedlp = currentPawn;
            }

            if (enabledb && enabled)
            {
                ApplyPatch();
                SetThirdPersonState(true);
                enabled = true;
            }
            else if (!enabledb && !enabled)
            {
                SetThirdPersonState(false);
                RemovePatch();
                enabled = false;
            }
        }

        private static void ApplyPatch()
        {
            if (patchApplied)
                return;
            
            //Console.WriteLine("Applying Patch");
             GameState.swed.WriteBytes(GameState.client + jnePatch, patchedJNE);
            patchApplied = true;
        }

        private static void RemovePatch()
        {
            if (!patchApplied)
                return;
            //Console.WriteLine("Removing Patch");

            GameState.swed.WriteBytes(GameState.client + jnePatch, originalJNE);
            patchApplied = false;
        }

        private static void SetThirdPersonState(bool enabled)
        {
             GameState.swed.WriteBool(GameState.client + Offsets.dwCSGOInput + 0x251, enabled);
        }

        private static bool NeedsReapply(IntPtr currentPawn)
        {
            return currentPawn != cachedlp && currentPawn != IntPtr.Zero;
        }

        protected override void FrameAction()
        {
            if (!enabled) return;

            ThirdPerson.Run(GameState.LocalPlayerPawn, enabled);
        }
    }
}
