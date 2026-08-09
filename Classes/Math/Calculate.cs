using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Data.Game;
using static Titled_Gui.Modules.Visual.BoneESP;
using Bone = Titled_Gui.Data.Entity.Types.Bone;
using Entity = Titled_Gui.Data.Entity.Entity;

namespace Titled_Gui.Classes.Math
{
    public static class Calculate
    {
        private static readonly HashSet<int> BonesToCheck = Enum.GetValues<BoneIds>().Select(b => (int)b).ToHashSet();
        [ThreadStatic] private static byte[]? _boneBuffer = null;
        [ThreadStatic] private static byte[]? _worldBoneBuffer = null;

        public static List<Bone> ReadBones(nint boneAddress, float[] viewMatrix, bool checkVisibility = false)
        {
            if (GameState.memory == null || GameState.LocalPlayer == null)
                return [];

            int maxBoneId = 102;
            if (_boneBuffer == null || _boneBuffer.Length < maxBoneId * 32)
                _boneBuffer = new byte[maxBoneId * 32];

            byte[] boneBytes = _boneBuffer;
            GameState.memory.ReadBytes(boneAddress, boneBytes, maxBoneId * 32);
            List<Bone> bones = [.. new Bone[maxBoneId]];
            Vector3 origin = GameState.LocalPlayer.EyePosition;

            for (int i = 0; i < 102; i++)
            {
                int id = (int)i;
                int offset = id * 32;
                if (offset + 32 > boneBytes.Length)
                    continue;

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
                    Rotation = new Quaternion(qx, qy, qz, qw),
                    IsVisible = true
                };

                bone.Position2D = MathUtils.WorldToScreen(viewMatrix, bone.Position);

                bones[id] = bone;
            }

            return bones;
        }
        public static List<Bone> ReadWorldEntityBones(nint boneAddress, float[] viewMatrix, bool checkVisibility = false) // less bones probably
        {
            if (GameState.memory == null || GameState.LocalPlayer == null)
                return [];

            int maxBoneId = 64;
            if (_worldBoneBuffer == null || _worldBoneBuffer.Length < maxBoneId * 32)
                _worldBoneBuffer = new byte[maxBoneId * 32];

            byte[] boneBytes = _worldBoneBuffer;
            GameState.memory.ReadBytes(boneAddress, boneBytes, maxBoneId * 32);
            List<Bone> bones = [.. new Bone[maxBoneId]];
            Vector3 origin = GameState.LocalPlayer.EyePosition;

            for (int i = 0; i < 102; i++)
            {
                int id = (int)i;
                int offset = id * 32;
                if (offset + 32 > boneBytes.Length)
                    continue;

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
                    Rotation = new Quaternion(qx, qy, qz, qw),
                    IsVisible = true
                };

                bone.Position2D = MathUtils.WorldToScreen(viewMatrix, bone.Position);

                bones[id] = bone;
            }

            return bones;
        }

        public static List<Hitbox> ReadHitboxes(Entity entity, float[] viewMatrix)
        {
            List<Hitbox> hitboxes = [];
            if (GameState.memory == null)
                return [];

            // m_modelState = 0x40
            // pCModel = 0x150
            // pCRenderMeshs = 0x78
            // hitboxSets = 0x168
            // hitboxCount = 0x28
            // hitboxArray = 0x30

            IntPtr modelState = GameState.memory.ReadPointer(entity.GameSceneNode + 0x40);
            if (modelState == IntPtr.Zero)
            {
                return hitboxes;
            }

            IntPtr pCModel = GameState.memory.ReadPointer(modelState + 0x150);
            if (pCModel == IntPtr.Zero)
            {
                Console.WriteLine("[ReadHitboxes] pCModel is null for entity: " + entity.Name);
                return hitboxes;
            }

            IntPtr CModel = GameState.memory.ReadPointer(pCModel);
            if (CModel == IntPtr.Zero)
            {
                Console.WriteLine("[ReadHitboxes] CModel is null for entity: " + entity.Name);
                return hitboxes;
            }

            IntPtr pCRenderMeshs = GameState.memory.ReadPointer(CModel + 0x78);
            if (pCRenderMeshs == IntPtr.Zero)
            {
                Console.WriteLine("[ReadHitboxes] pCRenderMeshs is null for entity: " + entity.Name);
                return hitboxes;
            }

            IntPtr CRenderMeshs = GameState.memory.ReadPointer(pCRenderMeshs);
            if (CRenderMeshs == IntPtr.Zero)
            {
                Console.WriteLine("[ReadHitboxes] CRenderMeshs is null for entity: " + entity.Name);
                return hitboxes;
            }

            IntPtr hitboxSets = GameState.memory.ReadPointer(CRenderMeshs + 0x168);
            if (hitboxSets == IntPtr.Zero)
            {
                //Console.WriteLine("[ReadHitboxes] hitboxSets is null for entity: " + entity.Name);
                return hitboxes;
            }

            int hitboxCount = GameState.memory.ReadInt(hitboxSets + 0x28);

            IntPtr pCHitbox = GameState.memory.ReadPointer(hitboxSets + 0x30);
            if (pCHitbox == IntPtr.Zero)
            {
                Console.WriteLine("[ReadHitboxes] pCHitbox is null for entity: " + entity.Name);
                return hitboxes;
            }

            int HitboxStride = 0x70;
            int NameOffset = 0x00;
            nint MinBoundsOffset = Offsets.m_vMinBounds;
            nint MaxBoundsOffset = Offsets.m_vMaxBounds;
            int RadiusOffset = Offsets.m_flShapeRadius;

            for (int i = 0; i < hitboxCount; i++)
            {
                IntPtr pCHitbox1 = pCHitbox + (i * HitboxStride);
                if (pCHitbox1 == IntPtr.Zero)
                {
                    continue;
                }

                IntPtr pName = GameState.memory.ReadPointer(pCHitbox1 + NameOffset);
                if (pName == IntPtr.Zero)
                {
                    continue;
                }

                string name = GameState.memory.ReadString(pName).Trim().Replace(" ", "").Replace("\0", "").Replace("playerfl", "");

                if (name == "spine_3")
                    continue;

                Vector3 min = GameState.memory.ReadVec(pCHitbox1 + MinBoundsOffset);
                Vector3 max = GameState.memory.ReadVec(pCHitbox1 + MaxBoundsOffset);
                float radius = GameState.memory.ReadFloat(pCHitbox1 + RadiusOffset);

                int boneID = Hitbox.HitboxToBone(i);
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

                Hitbox hitbox = new()
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
