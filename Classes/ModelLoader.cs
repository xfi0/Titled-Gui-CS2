using System.Numerics;
using SharpGLTF.Schema2;

public class ModelLoader
{
    public struct Mesh
    {
        public Vector3[] Vertices;
        public int[] Indices;
        public Vector4[] Joints;
        public Vector4[] Weights;
        public Matrix4x4[] InverseBindMatrices;
    }
    //public static Mesh Load(string path)
    //{
    //}
}