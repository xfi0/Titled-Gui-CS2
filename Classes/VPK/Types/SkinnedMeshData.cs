using System.Numerics;

namespace Titled_Gui.Classes.VPK.Types
{
    public struct SkinnedMeshData
    {
        public Vector3[] Positions;
        public Vector3[] Normals;
        public uint[] BlendIndices;
        public float[] BlendWeights;
        public uint[] Indices;
        public int BoneCount;
        public float[] InvBindPoses;
    }
}