using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Classes.Rendering.ChamsRenderer;
using Titled_Gui.Classes.VPK.Types;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using Titled_Gui.Data.Menu.Types;

namespace Titled_Gui.Modules.Visual
{
    internal class Chams : ChamsRendererBase
    {
        public static bool Enabled = false;
        public static bool TeamCheck = true;
        private int _nameReadFailCount;
        private bool _nameReadFailLogged;
        private IntPtr _modelNamePawn;
        private string? _modelNameCache;
        public Chams() : base("PlayerChams")
        {
            Console.WriteLine("Player chams initialized.");
        }
        protected override bool FeatureEnabled => Chams.Enabled;

        protected override bool UsePixelPerfect => Chams.PixelPerfect;
        public static Vector4 VisibleChamsColorTeam = new(1f, 1f, 1f, 1f);
        public static Vector4 VisibleChamsColorEnemy = new(1f, 0f, 0f, 1f);
        public static Vector4 OccludedChamsColorTeam = new(0f, 0f, 1f, 1f);
        public static Vector4 OccludedChamsColorEnemy = new(1f, 0.5f, 0f, 1f);
        public static Colors VisibleColors = new(VisibleChamsColorTeam, VisibleChamsColorEnemy);
        public static Colors OccludedColors = new(OccludedChamsColorTeam, OccludedChamsColorEnemy);

        public static int StyleIndex = 1;
        public static string[] StyleNames = [
            "Flat", "Textured", "Metallic", "Wireframe", "CS2 Glow", "LSD", "Plasma"
        ];
        public static int[] StyleMap = [1, 2, 3, 4, 5, 6, 7];
        public static bool PixelPerfect = false;

        protected override int GetStyleValue() => Chams.StyleMap[System.Math.Clamp(Chams.StyleIndex, 0, Chams.StyleMap.Length - 1)];

        protected override List<ChamsMeshDraw> CollectDraws()
        {
            List<ChamsMeshDraw> draws = [];

            if (GameState.renderer == null || GameState.renderer.Entities == null)
                return draws;

            foreach (Entity? entity in GameState.renderer.Entities)
            {
                if (entity == null || entity.Bones == null || entity.Bones.Count == 0 || entity.PawnAddress == GameState.renderer.LocalPlayer.PawnAddress || Chams.TeamCheck && entity.Team == GameState.renderer.LocalPlayer.Team)
                    continue;

                GpuMesh? mesh = GetMesh(entity);
                if (mesh == null)
                    continue;

                bool visible = entity.Bones.Any(b => b.IsVisible);

                var enemyVisible = Chams.VisibleColors.EnemyRGB ? Colors.Rgb(Chams.VisibleColors.EnemyColor.W) : Chams.VisibleColors.EnemyColor;
                var teamVisible = Chams.VisibleColors.TeamRGB ? Colors.Rgb(Chams.VisibleColors.TeamColor.W) : Chams.VisibleColors.TeamColor;
                var enemyOccluded = Chams.OccludedColors.EnemyRGB ? Colors.Rgb(Chams.OccludedColors.EnemyColor.W) : Chams.OccludedColors.EnemyColor;
                var teamOccluded = Chams.OccludedColors.TeamRGB ? Colors.Rgb(Chams.OccludedColors.TeamColor.W) : Chams.OccludedColors.TeamColor;

                Vector4 visibleColor;
                Vector4 occludedColor;
                if (entity.IsEnemy)
                {
                    visibleColor = visible ? enemyVisible : enemyOccluded;
                    occludedColor = visible ? enemyOccluded : enemyVisible;
                }
                else
                {
                    visibleColor = visible ? teamVisible : teamOccluded;
                    occludedColor = visible ? teamOccluded : teamVisible;
                }

                draws.Add(new ChamsMeshDraw(entity.Bones, mesh.Value, VisibleColor: visibleColor, OccludedColor: occludedColor));
            }

            return draws;
        }

        private GpuMesh? GetMesh(Entity entity)
        {
            string? name = ReadModelName(entity);
            if (string.IsNullOrEmpty(name))
            {
                _nameReadFailCount++;
                if (!_nameReadFailLogged)
                    _nameReadFailLogged = true;

                return null;
            }

            return GetCachedModel(name);
        }

        private string? ReadModelName(Entity entity)
        {
            if (entity.GameSceneNode == IntPtr.Zero || entity.PawnAddress == _modelNamePawn && _modelNameCache != null || GameState.memory == null)
                return _modelNameCache;

            var modelNamePointer = GameState.memory.ReadPointer(entity.GameSceneNode + Offsets.m_modelState + Offsets.m_ModelName);
            if (modelNamePointer == IntPtr.Zero)
                return null;

            var modelName = GameState.memory.ReadString(modelNamePointer, 260);
            if (string.IsNullOrEmpty(modelName))
                return null;

            _modelNamePawn = entity.PawnAddress;
            _modelNameCache = modelName.Replace('\\', '/').ToLowerInvariant();
            return _modelNameCache;
        }
    }
}