using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static Titled_Gui.Data.Entity.EntityTypes;

namespace Titled_Gui.Data.Game.C4
{
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
