using System.Numerics;

namespace Titled_Gui.Data.Game.VRF
{
    public class Types
    {
        private const float Epsilon = 0.0000001f;

        public struct BoundingBox
        {
            public Vector3 Min, Max;

            public bool Intersect(Vector3 origin, Vector3 end)
            {
                Vector3 dir = end - origin;

                float invX = dir.X == 0f ? float.MaxValue : 1f / dir.X, invY = dir.Y == 0f ? float.MaxValue : 1f / dir.Y, invZ = dir.Z == 0f ? float.MaxValue : 1f / dir.Z;

                float t1 = (Min.X - origin.X) * invX;
                float t2 = (Max.X - origin.X) * invX;
                float t3 = (Min.Y - origin.Y) * invY;
                float t4 = (Max.Y - origin.Y) * invY;
                float t5 = (Min.Z - origin.Z) * invZ;
                float t6 = (Max.Z - origin.Z) * invZ;

                float tmin = Math.Max(Math.Max(Math.Min(t1, t2), Math.Min(t3, t4)), Math.Min(t5, t6));
                float tmax = Math.Min(Math.Min(Math.Max(t1, t2), Math.Max(t3, t4)), Math.Max(t5, t6));

                return tmax >= 0 && tmin <= tmax && tmin <= 1f;
            }
        };
        public struct Triangle
        {
            public Vector3 Point1, Point2, Point3;

            public bool Intersect(Vector3 origin, Vector3 dir)
            {
                Vector3 edge1, edge2, h, s, q;
                float a, f, u, v, t;
                edge1 = Point2 - Point1;
                edge2 = Point3 - Point1;
                h = Vector3.Cross(dir, edge2);
                a = Vector3.Dot(edge1, h);
                if (a > -Epsilon && a < Epsilon)
                    return false;

                f = 1f / a;
                s = origin - Point1;
                u = f * Vector3.Dot(s, h);

                if (u < 0f || u > 1f)
                    return false;

                q = Vector3.Cross(s, edge1);
                v = f * Vector3.Dot(dir, q);

                if (v < 0f || u + v > 1f)
                    return false;

                t = f * Vector3.Dot(edge2, q);

                if (t > Epsilon && t < 1f)
                    return true;

                return false;
            }
        }
        public class KDNode
        {
            public BoundingBox bbox;
            public Triangle[] Triangles = [];
            public KDNode? Left = null;
            public KDNode? Right = null;
            public int Axis;

            static void DeleteKDTree(KDNode? node)
            {
                if (node == null) return;

                DeleteKDTree(node?.Left);
                DeleteKDTree(node?.Right);

                node?.Left = null;
                node?.Right = null;
            }
        }
    }
}
