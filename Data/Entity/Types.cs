using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Entity
{
    public class Types
    {
        public class Bone
        {
            public Vector3 Position { get; set; }
            public Vector2 Position2D { get; set; }
            public bool IsVisible { get; set; }
        }

        public enum BombSite
        {
            A = 0,
            B,
            Unknown
        }
        public class Hitbox
        {
            public nint m_name; // CUtlString
            public nint m_sSurfaceProperty; // CUtlString
            public nint m_sBoneName; // CUtlString
            public nint m_vMinBounds; // Vector
            public nint m_vMaxBounds; // Vector
            public float m_flShapeRadius; // float32
            public nint m_nBoneNameHash; // uint32
            public int m_nGroupId; // int32
            public nint m_nShapeType; // uint8
            public uint m_bTranslationOnly; // bool
            public nint m_CRC = 0x40; // uint32
            public Color m_cRenderColor; // Color
            public UInt16 m_nHitBoxIndex; // uint16
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
            public float[] Matrix { get; set; }
        }
    }
}
