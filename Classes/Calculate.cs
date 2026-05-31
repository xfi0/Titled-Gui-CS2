using System.Numerics;
using Titled_Gui.Data.Entity;
using Titled_Gui.Data.Game;
using static Titled_Gui.Modules.Visual.BoneESP;
using Bone = Titled_Gui.Data.Entity.Types.Bone;
using Entity = Titled_Gui.Data.Entity.Entity;

namespace Titled_Gui.Classes
{
    public static class Calculate
    {
        public static Vector2 WorldToScreen(float[] matrix, Vector3 pos) // this seemed slightly better or same idk
        {
            // calculate depth
            float view = matrix[12] * pos.X + matrix[13] * pos.Y + matrix[14] * pos.Z + matrix[15];

            // if entity is not visible
            if (view <= 0.01f)
                return new(-99, -99); // if entity is not visible


            // calculate screen x and y
            float screenX = matrix[0] * pos.X + matrix[1] * pos.Y + matrix[2] * pos.Z + matrix[3];
            float screenY = matrix[4] * pos.X + matrix[5] * pos.Y + matrix[6] * pos.Z + matrix[7];

            // perspective division 
            float halfW = GameState.renderer.ScreenSize.X * 0.5f;
            float halfH = GameState.renderer.ScreenSize.Y * 0.5f;

            float X = halfW + (screenX / view) * halfW;
            float Y = halfH - (screenY / view) * halfH;

            if (X < -halfW || X > halfW * 3 || Y < -halfH || Y > halfH * 3)
                return new(-99, -99);

            return new(X, Y);
        }

        public static Vector3 AngleToForward(Vector3 angles)
        {
            float pitch = angles.X * (MathF.PI / 180f);
            float yaw = angles.Y * (MathF.PI / 180f);

            return new Vector3(MathF.Cos(pitch) * MathF.Cos(yaw), MathF.Cos(pitch) * MathF.Sin(yaw), -MathF.Sin(pitch));
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle > 180) angle -= 360;
            while (angle < -180) angle += 360;
            return angle;
        }

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

                bone.Position2D = WorldToScreen(viewMatrix, bone.Position);

                bones[id] = bone;
            }

            return bones;
        }

        public static List<Types.Hitbox> ReadHitboxes(Entity entity, float[] viewMatrix)
        {
            List<Types.Hitbox> hitboxes = new();

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

                int boneID = Titled_Gui.Data.Entity.Types.Hitbox.HitboxToBone(i);
                if (boneID < 0 || entity.Bones == null || boneID >= entity.Bones.Count) continue;

                Vector3 position = entity.Bones[boneID].Position;
                Vector2 position2D = entity.Bones[boneID].Position2D;
                Quaternion rotation = entity.Bones[boneID].Rotation;

                Vector3 worldMin = position + min;
                Vector3 worldMax = position + max;

                Vector2 min2D = Calculate.WorldToScreen(viewMatrix, worldMin);
                Vector2 max2D = Calculate.WorldToScreen(viewMatrix, worldMax);

                if (min2D == new Vector2(-99, -99) || max2D == new Vector2(-99, -99)) continue;

                Types.Hitbox hitbox = new Types.Hitbox
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
