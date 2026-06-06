using System.Numerics;
using Titled_Gui.Classes.Math;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Modules.Visual.BoneESP;
using Bone = Titled_Gui.Data.Entity.EntityTypes.Bone;
using Entity = Titled_Gui.Data.Entity.Entity;

namespace Titled_Gui.Classes
{
    public static class Calculate
    {
      
        private static readonly HashSet<int> BonesToCheck = Enum.GetValues<BoneIds>()
            .Select(b => (int)b)
            .ToHashSet();

        public static List<Bone> ReadBones(nint boneAddress, float[] viewMatrix)
        {

            int maxBoneId = 102;
            byte[] boneBytes = GameState.memory.ReadBytes(boneAddress, maxBoneId * 32);
            List<Bone> bones = new(new Bone[maxBoneId]);

            for (int i = 0; i < 102; i++)
            {
                int id = (int)i;
                int offset = id * 32;
                if (offset + 32 > boneBytes.Length) continue;

                float x = BitConverter.ToSingle(boneBytes, offset + 0);
                float y = BitConverter.ToSingle(boneBytes, offset + 4);
                float z = BitConverter.ToSingle(boneBytes, offset + 8);
                float qx = BitConverter.ToSingle(boneBytes, offset + 16);
                float qy = BitConverter.ToSingle(boneBytes, offset + 20);
                float qz = BitConverter.ToSingle(boneBytes, offset + 24);
                float qw = BitConverter.ToSingle(boneBytes, offset + 28);

                Bone bone = new()
                {
                    Position = new Vector3(x, y, z),
                    Rotation = new Quaternion(qx, qy, qz, qw)
                };

                if (BonesToCheck.Contains(id))
                    bone.IsVisible = VisibilityCheck.Visible(GameState.LocalPlayer.EyePosition, bone.Position);

                bone.Position2D = MathUtils.WorldToScreen(viewMatrix, bone.Position);

                bones[id] = bone;
            }

            return bones;
        }

        public static List<EntityTypes.Hitbox> ReadHitboxes(Entity entity, float[] viewMatrix)
        {
            List<EntityTypes.Hitbox> hitboxes = new();

            IntPtr pCModel = GameState.memory.ReadPointer(entity.GameSceneNode + Offsets.m_modelState + 0xC0);
            if (pCModel == IntPtr.Zero)
                return hitboxes;

            IntPtr CModel = GameState.memory.ReadPointer(pCModel);
            if (CModel == IntPtr.Zero)
                return hitboxes;

            IntPtr pCRenderMeshs = GameState.memory.ReadPointer(CModel + 0x78);
            if (pCRenderMeshs == IntPtr.Zero)
                return hitboxes;

            IntPtr CRenderMeshs = GameState.memory.ReadPointer(pCRenderMeshs);
            if (CRenderMeshs == IntPtr.Zero)
                return hitboxes;

            IntPtr hitboxSets = GameState.memory.ReadPointer(CRenderMeshs + 0x150);
            if (hitboxSets == IntPtr.Zero)
                return hitboxes;

            int hitboxCount = GameState.memory.ReadInt(hitboxSets + 0x28);
            IntPtr pCHitbox = GameState.memory.ReadPointer(hitboxSets + 0x30);

            int hitboxStride = 0x70;

            for (int i = 0; i < hitboxCount; i++)
            {
                IntPtr pCHitbox1 = pCHitbox + (i * hitboxStride);
                IntPtr hb = GameState.memory.ReadPointer(pCHitbox1);

                Vector3 min = GameState.memory.ReadVec(pCHitbox1 + Offsets.m_vMinBounds);
                Vector3 max = GameState.memory.ReadVec(pCHitbox1 + Offsets.m_vMaxBounds);
                float radius = GameState.memory.ReadFloat(pCHitbox1 + Offsets.m_flShapeRadius);
                string name = GameState.memory.ReadString(hb + Offsets.m_name)
                    .Trim().Replace(" ", "").Replace("\0", "").Replace("playerfl", "");

                if (name.Contains("neck") || name == "spine_3" || name == "spine_2")
                    continue;

                int boneID = Titled_Gui.Data.Entity.EntityTypes.Hitbox.HitboxToBone(i);
                if (boneID < 0 || entity.Bones == null || boneID >= entity.Bones.Count)
                    continue;

                Vector3 position = entity.Bones[boneID].Position;
                Vector2 position2D = entity.Bones[boneID].Position2D;
                Quaternion rotation = entity.Bones[boneID].Rotation;

                Vector3 worldMin = position + min;
                Vector3 worldMax = position + max;

                Vector2 min2D = MathUtils.WorldToScreen(viewMatrix, worldMin);
                Vector2 max2D = MathUtils.WorldToScreen(viewMatrix, worldMax);

                if (min2D == new Vector2(-99, -99) || max2D == new Vector2(-99, -99))
                    continue;

                EntityTypes.Hitbox hitbox = new EntityTypes.Hitbox
                {
                    Name = name,
                    MinBounds = min,
                    MaxBounds = max,
                    ShapeRadius = radius,
                    Index = i,
                    boneID = boneID,
                    WorldMin = worldMin,
                    WorldMax = worldMax,
                    BonePosition = position,
                    BonePosition2D = position2D,
                    BoneRotation = rotation,
                    Bone = entity.Bones[boneID]
                };
                hitboxes.Add(hitbox);
            }

            return hitboxes;
        }
    }
}
