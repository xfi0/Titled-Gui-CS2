using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace Titled_Gui.Data.Entity.Types
{
    public class Bone
    {
        public Vector3 Position { get; set; }
        public Vector2 Position2D { get; set; }
        public bool IsVisible { get; set; }
        public Quaternion Rotation { get; set; }
        public Matrix4x4 Transform => Matrix4x4.CreateFromQuaternion(Rotation) * Matrix4x4.CreateTranslation(Position);
    }
}
