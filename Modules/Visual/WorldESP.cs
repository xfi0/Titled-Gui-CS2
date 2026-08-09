using ImGuiNET;
using System.Numerics;
using Titled_Gui.Classes;
using Titled_Gui.Classes.Math;
using Titled_Gui.Classes.Rendering;
using Titled_Gui.Classes.Rendering.ChamsRenderer;
using Titled_Gui.Classes.VPK.Types;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Modules.Visual
{
    internal class WorldESP
    {
        #region Bools/Toggles
        public static bool ChickenESP = false;
        public static bool DroppedWeaponESP = false;
        public static bool ProjectileESP = false;
        public static bool HostageESP = false;
        public static bool MolotovBoundsESP = false;
        public static bool DrawBoxes = true;
        public static bool DrawText = true;
        public static bool DrawChams = true;
        #endregion

        #region Colors
        public static Vector4 WeaponTextColor = new(1, 1, 1, 1);
        public static Vector4 ProjectileTextColor = new(1, 1, 1, 1);
        public static Vector4 ChickenTextColor = new(1, 1, 1, 1);
        public static Vector4 HostageTextColor = new(1, 1, 1, 1);
        private static Vector4 _molotovFillColor = new(1f, 0.4f, 0f, 0.196f);
        private static Vector4 _molotovOutlineColor = new(1f, 0.4f, 0f, 0.588f);
        public static Colors MolotovColors = new(primaryColor: _molotovFillColor, secondaryColor: _molotovOutlineColor);
        public static Vector4 BoxColor = new(1, 1, 1, 1);
        #endregion Colors

        #region Chams
        public class WorldChams : ChamsRendererBase
        {
            public WorldChams() : base("WorldChams")
            {
                Console.WriteLine("World Chams Initialized.");
            }

            public static List<WorldEntity> WorldEntities = [];
            protected override bool FeatureEnabled => DrawChams;

            protected override List<ChamsMeshDraw> CollectDraws()
            {
                List<ChamsMeshDraw> draws = [];

                foreach (WorldEntity? worldEntity in WorldEntities)
                {
                    if (worldEntity == null || worldEntity.Bones == null || worldEntity.Bones.Count == 0 || string.IsNullOrEmpty(worldEntity.ModelName))
                        continue;

                    GpuMesh? mesh = GetCachedModel(worldEntity.ModelName);
                    if (mesh == null)
                        continue;

                    draws.Add(new ChamsMeshDraw(worldEntity.Bones, mesh.Value, new(1, 1, 1, 1), new(1, 1, 1, 1)));
                }

                return draws;
            }
        }
        #endregion
        public static void EntityESP()
        {
            WorldChams.WorldEntities.Clear();

            foreach (WorldEntity? worldEntity in GameState.worldEntities)
            {
                if (worldEntity == null || GameState.memory == null)
                    continue;

                float[] viewMatrix = GameState.memory.ReadMatrix(GameState.client + Offsets.dwViewMatrix);
                var position2D = Titled_Gui.Classes.Math.MathUtils.WorldToScreen(viewMatrix, worldEntity.Position);
                if (position2D == new Vector2(-99, -99))
                    continue;

                if (ChickenESP && worldEntity.Type == WorldEntityManager.EntityKind.Chicken)
                {
                    WorldChams.WorldEntities.Add(worldEntity);
                    DrawChickenESP(worldEntity, position2D, viewMatrix);
                }

                if (DroppedWeaponESP && worldEntity.Type == WorldEntityManager.EntityKind.Weapon)
                {
                    WorldChams.WorldEntities.Add(worldEntity);
                    DrawWeaponESP(worldEntity, position2D, viewMatrix);
                }

                if (ProjectileESP && worldEntity.Type == WorldEntityManager.EntityKind.Projectile && worldEntity.DisplayName != "Molotov Fire")
                {
                    WorldChams.WorldEntities.Add(worldEntity);
                    DrawProjectileESP(worldEntity, position2D, viewMatrix);
                }

                if (HostageESP && worldEntity.Type == WorldEntityManager.EntityKind.Hostage)
                {
                    WorldChams.WorldEntities.Add(worldEntity);
                    DrawHostageESP(worldEntity, position2D, viewMatrix);
                }

                if (MolotovBoundsESP && worldEntity.Type == WorldEntityManager.EntityKind.Projectile && worldEntity.DisplayName == "Molotov Fire")
                {
                    WorldChams.WorldEntities.Add(worldEntity);
                    DrawMolotovBounds(worldEntity, position2D, viewMatrix);
                }
            }
        }

        private static void DrawHostageESP(WorldEntity worldEntity, Vector2 position2D, float[] viewMatrix)
        {
            if (worldEntity == null || GameState.renderer == null)
                return;

            float thickness = 2f;
            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);
            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }
            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.DrawList.AddText(position2D, ImGui.ColorConvertFloat4ToU32(HostageTextColor), "Hostage");
        }

        private static void DrawProjectileESP(WorldEntity worldEntity, Vector2 position2D, float[] viewMatrix)
        {
            if (worldEntity == null || GameState.renderer == null)
                return;

            float thickness = 2f;
            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);
            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.DrawList.AddText(position2D,
                    ImGui.ColorConvertFloat4ToU32(ProjectileTextColor), worldEntity.DisplayName);
        }

        private static void DrawWeaponESP(WorldEntity worldEntity, Vector2 position2D, float[] viewMatrix)
        {
            if (worldEntity == null || GameState.renderer == null)
                return;

            float thickness = 2f;
            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);
            var corners2D = new Vector2[8];
            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);
            if (DrawText)
                GameState.renderer.DrawList.AddText(position2D,
                    ImGui.ColorConvertFloat4ToU32(WeaponTextColor), worldEntity.DisplayName);
        }

        private static void DrawChickenESP(WorldEntity? worldEntity, Vector2 position2D, float[] viewMatrix)
        {
            if (worldEntity == null || GameState.renderer == null)
                return;

            float thickness = 2f;
            uint preConvertedColor = ImGui.ColorConvertFloat4ToU32(BoxColor);
            Vector3[] corners3D = worldEntity.Get3DCorners(worldEntity);
            var corners2D = new Vector2[8];

            for (int i = 0; i < corners2D.Length; i++)
            {
                corners2D[i] = MathUtils.WorldToScreen(viewMatrix, corners3D[i]);
                if (corners2D[i] == new Vector2(-99, -99)) return;
            }

            if (DrawBoxes)
                Draw3DBoxESP(corners2D, preConvertedColor, false, thickness);

            if (DrawText)
                GameState.renderer.DrawList.AddText(position2D,
                    ImGui.ColorConvertFloat4ToU32(ChickenTextColor),
                    "Chicken");
        }

        private static Vector4 GetMolotovColor(bool fill)
        {
            if (fill && MolotovColors.PrimaryRGB)
                return Colors.Rgb(alpha: MolotovColors.PrimaryColor.W);
            else if (!fill && MolotovColors.SecondaryRGB)
                return Colors.Rgb(alpha: MolotovColors.SecondaryColor.W);
            else if (fill)
                return MolotovColors.PrimaryColor;
            else
                return MolotovColors.SecondaryColor;
        }

        public static void DrawMolotovBounds(WorldEntity worldEntity, Vector2 position2D, float[] viewMatrix)
        {
            if (worldEntity == null || GameState.memory == null)
                return;

            const float fireRadius = 60.0f;
            const int pointsPerFire = 12;

            int firePointCount = GameState.memory.ReadInt(worldEntity.PawnAddress + Offsets.m_fireCount);
            if (firePointCount <= 0 || firePointCount >= 128)
                return;

            List<Vector2> points = new();

            for (int i = 0; i < firePointCount; i++)
            {
                unsafe
                {
                    Vector3 firePoint = GameState.memory.ReadVec(worldEntity.PawnAddress + Offsets.m_firePositions + i * sizeof(Vector3));

                    for (int j = 0; j < pointsPerFire; j++)
                    {
                        float angle = (float)j / pointsPerFire * MathF.PI * 2.0f;
                        Vector3 world = firePoint + new Vector3(MathF.Cos(angle) * fireRadius, MathF.Sin(angle) * fireRadius, 0f);
                        Vector2 projected = MathUtils.WorldToScreen(viewMatrix, world);

                        if (projected == new Vector2(-99, -99))
                            return;

                        points.Add(projected);
                    }
                }
            }

            if (points.Count < 3)
                return;

            uint fill = ImGui.ColorConvertFloat4ToU32(GetMolotovColor(true));
            uint outline = ImGui.ColorConvertFloat4ToU32(GetMolotovColor(false));
            ShapeRenderer.DrawConvexHull(points, fill, outline);
        }

        public static void Draw3DBoxESP(Vector2[] corners2D, uint preConvertedColor, bool filled, float rounding, uint preConvertedFilledColor = 0)
        {
            if (GameState.renderer == null)
                return;

            try
            {
                if (filled)
                {
                    // bottom face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2], preConvertedFilledColor);
                    // top face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[4], corners2D[5], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // front face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[5], corners2D[4], preConvertedFilledColor);
                    // back face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[2], corners2D[3], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // left face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[2], corners2D[6], corners2D[4], preConvertedFilledColor);
                    // right face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[1], corners2D[3], corners2D[7], corners2D[5], preConvertedFilledColor);
                }

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[0], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[4], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[5], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[7], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[7], corners2D[4], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[7], preConvertedColor, rounding);
            }
            catch (IndexOutOfRangeException)
            {
                Console.WriteLine("Index Out Of Bounds Of The Array Drawing 3D Boxes");
            }
        }
        public static void Draw3DBoxESPFromMatrix(Vector2[] corners2D, uint preConvertedColor, bool filled, float rounding, uint preConvertedFilledColor = 0)
        {
            if (GameState.renderer == null)
                return;

            try
            {
                if (filled)
                {
                    // bottom face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[3], corners2D[2], preConvertedFilledColor);
                    // top face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[4], corners2D[5], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // front face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[1], corners2D[5], corners2D[4], preConvertedFilledColor);
                    // back face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[2], corners2D[3], corners2D[7], corners2D[6], preConvertedFilledColor);
                    // left face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[0], corners2D[2], corners2D[6], corners2D[4], preConvertedFilledColor);
                    // right face
                    GameState.renderer.DrawList.AddQuadFilled(corners2D[1], corners2D[3], corners2D[7], corners2D[5], preConvertedFilledColor);
                }

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[1], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[3], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[2], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[0], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[4], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[5], corners2D[7], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[7], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[6], corners2D[4], preConvertedColor, rounding);

                GameState.renderer.DrawList.AddLine(corners2D[0], corners2D[4], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[1], corners2D[5], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[2], corners2D[6], preConvertedColor, rounding);
                GameState.renderer.DrawList.AddLine(corners2D[3], corners2D[7], preConvertedColor, rounding);
            }
            catch (IndexOutOfRangeException ex)
            {
                Console.WriteLine("Index Out Of Bounds Of The Array Drawing 3D Boxes: " + ex);
            }
        }
    }
}
