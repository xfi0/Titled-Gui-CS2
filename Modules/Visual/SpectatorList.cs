using ImGuiNET;
using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class SpectatorList : IModule
    {
        public static bool Enabled = false;
        private static Vector2 _windowSize = new(200, 300);
        private static Vector2 _defaultWindowPos = new(75, 75);
        public static void DrawMenu()
        {
            if (!Enabled)
                return;

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(16, 16));
            ImGui.SetNextWindowSize(_windowSize);
            ImGui.SetNextWindowPos(_defaultWindowPos, ImGuiCond.FirstUseEver);
            ImGui.Begin("Spectator List", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoResize);
            ImGui.Text("Spectator List");
            DrawAllPlayers();
            ImGui.PopStyleVar(1);
            ImGui.End();
        }

        private static void DrawPlayer(string name)
        {
            ImGui.Text(name);
        }

        private static void DrawAllPlayers()
        {
            foreach (Entity? entity in GameState.Entities)
            {
                if (entity == null || entity.Name == null || entity.Name.Length <= 0 || GameState.LocalPlayer == null || entity.PawnAddress == GameState.LocalPlayer.PawnAddress || entity.Health > 0)
                    continue;

                var handle = EntityManager.GetPlayerPawn(GameState.EntityList, entity.Pawn);
                if (handle == IntPtr.Zero)
                    continue;

                var target = GetObserverTarget(handle);

                if (target == GameState.LocalPlayer.PawnAddress)
                    DrawPlayer(entity.Name);
            }
        }

        private static IntPtr GetObserverTarget(IntPtr pawnAddress)
        {
            if (pawnAddress == IntPtr.Zero || GameState.memory == null)
                return IntPtr.Zero;

            var observerServices = GameState.memory.Read<IntPtr>(pawnAddress + Offsets.m_pObserverServices);
            if (observerServices == IntPtr.Zero)
                return IntPtr.Zero;

            var observerTarget = GameState.memory.Read<IntPtr>(observerServices + Offsets.m_hObserverTarget);
            if (observerTarget == IntPtr.Zero)
                return IntPtr.Zero;

            var entity = EntityManager.GetPlayerPawn(GameState.EntityList, observerTarget);

            return entity;
        }
    }
}
