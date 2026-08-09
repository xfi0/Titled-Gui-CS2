using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Titled_Gui.Classes.VPK.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SkinnedVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public uint Joint0;
        public uint Joint1;
        public uint Joint2;
        public uint Joint3;
        public Vector4 Weights;
    }
}
