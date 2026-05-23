using System.Numerics;
using Titled_Gui.Modules.Visual;

namespace Titled_Gui.Data.Entity
{
    public class Types
    {
        public class Bone
        {
            public Vector3 Position { get; set; }
            public Vector2 Position2D { get; set; }
            public bool IsVisible { get; set; }
            public Quaternion Rotation { get; set; }
            public Matrix4x4 Transform =>
                Matrix4x4.CreateFromQuaternion(Rotation) *
                Matrix4x4.CreateTranslation(Position);
        }

        public enum BombSite
        {
            A = 0,
            B,
            Unknown
        }
        public static class HitboxBoneMap
        {
            private const int INVALID = -1;
            private const int HEAD_0 = (int)BoneESP.BoneIds_Full.Head;
            private const int NECK_0 = (int)BoneESP.BoneIds_Full.Neck;
            private const int PELVIS = (int)BoneESP.BoneIds_Full.Pelvis;
            private const int SPINE_0 = (int)BoneESP.BoneIds_Full.Spine_0;
            private const int SPINE_1 = (int)BoneESP.BoneIds_Full.Spine_1;
            private const int SPINE_2 = (int)BoneESP.BoneIds_Full.Spine_2;
            private const int SPINE_3 = (int)BoneESP.BoneIds_Full.Spine_3;
            private const int LEG_UPPER_L = 17;
            private const int LEG_UPPER_R = 20;
            private const int LEG_LOWER_L = 18;
            private const int LEG_LOWER_R = 21;
            private const int ANKLE_L = 19;
            private const int ANKLE_R = 22;
            private const int HAND_L = 11;
            private const int HAND_R = 15;
            private const int ARM_UPPER_L = 9;
            private const int ARM_LOWER_L = 10;
            private const int ARM_UPPER_R = 13;
            private const int ARM_LOWER_R = 14;

            public static readonly int[] Map = new[]
            {
                HEAD_0,
                NECK_0,
                PELVIS,
                SPINE_0,
                SPINE_1,
                SPINE_2,
                SPINE_3,
                LEG_UPPER_L,
                LEG_UPPER_R,
                LEG_LOWER_L,
                LEG_LOWER_R,
                ANKLE_L,
                ANKLE_R,
                HAND_L,
                HAND_R,
                ARM_UPPER_L,
                ARM_LOWER_L,
                ARM_UPPER_R,
                ARM_LOWER_R,
            };
        }
        public class Hitbox
        {
            public string Name = "Invalid";
            public Vector3 MinBounds;
            public Vector3 MaxBounds;
            public float ShapeRadius;
            public int Index;
            public int boneID;
            public Vector3 WorldMin;
            public Vector3 WorldMax;
            public Vector3 BonePosition;
            public Vector2 BonePosition2D;
            public Quaternion BoneRotation;
            public Bone? Bone;

            public static int HitboxToBone(int hitboxIndex)
            {
                if (hitboxIndex < 0 || hitboxIndex >= HitboxBoneMap.Map.Length)
                    return HitboxBoneMap.Map[-1]; // invalid

                return HitboxBoneMap.Map[hitboxIndex];
            }
        }

        public class C4
        {
            public IntPtr Address { get; set; } = IntPtr.Zero;
            public BombSite PlantedSite = BombSite.Unknown;
            public Vector3 Position { get; set; }
            public Vector2 Position2D { get; set; }
            public float ExplosionTime { get; set; } = 40;
            public bool BeingDefused { get; set; }
            public bool Planted { get; set; }
            public float[]? Matrix { get; set; }
        }
    }
}
