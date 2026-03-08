using System.Diagnostics;
using System.Runtime.InteropServices;
using static Titled_Gui.Data.Game.VRF.Types;
using Vector3 = System.Numerics.Vector3;

namespace Titled_Gui.Data.Game.MapParser
{
    public class MapLoader // https://github.com/AtomicBool/cs2-map-parser  THIS TOOK 40 MINS TO CONVERT FROM CPP TO C#
    {
        private readonly string cs2BaseFolder = @"C:\Program Files (x86)\Steam\steamapps\common\Counter-Strike Global Offensive\game\csgo\";
        private readonly string _mapsFolder = "maps";
        private readonly string _pathToTris = Path.Combine(AppContext.BaseDirectory, "Data", "Game", "MapParser", "PreExtractedMapData" , "tri");
        public string PreviousMapName = "";

        #region Misc Helpers
        public bool RayIntersectsKDTree(KDNode? node, Vector3 origin, Vector3 end)
        {
            if (node == null) return false;
            Vector3 dir = end - origin;

            if (!node.bbox.Intersect(origin, end))
                return false;

            if (node.Triangles.Length > 0)
            {
                foreach (Triangle tri in node.Triangles)
                    if (tri.Intersect(origin, dir))
                        return true;

                return false;
            }

            if (RayIntersectsKDTree(node.Left, origin, end)) 
                return true;
            return RayIntersectsKDTree(node.Right, origin, end);
        }
        private static BoundingBox CalculateBoundingBox(List<Triangle> triangles)
        {
            Triangle first = triangles[0];
            Vector3 min = first.Point1;
            Vector3 max = first.Point1;

            foreach (Triangle tri in triangles)
            {
                if (tri.Point1.X < min.X) min.X = tri.Point1.X; else if (tri.Point1.X > max.X) max.X = tri.Point1.X;
                if (tri.Point1.Y < min.Y) min.Y = tri.Point1.Y; else if (tri.Point1.Y > max.Y) max.Y = tri.Point1.Y;
                if (tri.Point1.Z < min.Z) min.Z = tri.Point1.Z; else if (tri.Point1.Z > max.Z) max.Z = tri.Point1.Z;

                if (tri.Point2.X < min.X) min.X = tri.Point2.X; else if (tri.Point2.X > max.X) max.X = tri.Point2.X;
                if (tri.Point2.Y < min.Y) min.Y = tri.Point2.Y; else if (tri.Point2.Y > max.Y) max.Y = tri.Point2.Y;
                if (tri.Point2.Z < min.Z) min.Z = tri.Point2.Z; else if (tri.Point2.Z > max.Z) max.Z = tri.Point2.Z;

                if (tri.Point3.X < min.X) min.X = tri.Point3.X; else if (tri.Point3.X > max.X) max.X = tri.Point3.X;
                if (tri.Point3.Y < min.Y) min.Y = tri.Point3.Y; else if (tri.Point3.Y > max.Y) max.Y = tri.Point3.Y;
                if (tri.Point3.Z < min.Z) min.Z = tri.Point3.Z; else if (tri.Point3.Z > max.Z) max.Z = tri.Point3.Z;
            }

            return new BoundingBox { Min = min, Max = max };
        }
        KDNode? BuildKDTree(List<Triangle> triangles, int depth = 0)
        {
            if (triangles.Count <= 0) return null;

            KDNode node = new();
            node.bbox = CalculateBoundingBox(triangles);
            node.Axis = depth % 3;

            if (triangles.Count <= 3)
            {
                node.Triangles = [.. triangles];
                return node;
            }

            float[] centers = new float[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                centers[i] = node.Axis switch
                {
                    0 => (t.Point1.X + t.Point2.X + t.Point3.X) / 3f,
                    1 => (t.Point1.Y + t.Point2.Y + t.Point3.Y) / 3f,
                    _ => (t.Point1.Z + t.Point2.Z + t.Point3.Z) / 3f,
                };
            }

            int[] indices = Enumerable.Range(0, triangles.Count).ToArray();
            Array.Sort(indices, (a, b) => centers[a].CompareTo(centers[b]));

            int mid = triangles.Count / 2;

            List<Triangle> leftTriangles = indices.Take(mid).Select(i => triangles[i]).ToList(); // [..mid] is lowk ugly
            List<Triangle> rightTriangles = indices.Skip(mid).Select(i => triangles[i]).ToList(); // same here, ik I can use [mid..] to splice, but it looks wierd

            if (triangles.Count > 1000 && depth < 4)
            {
                KDNode? left = null, right = null;
                Parallel.Invoke(() => left = BuildKDTree(leftTriangles, depth + 1), () => right = BuildKDTree(rightTriangles, depth + 1));
                node.Left = left;
                node.Right = right;
            }
            else
            {
                node.Left = BuildKDTree(leftTriangles, depth + 1);
                node.Right = BuildKDTree(rightTriangles, depth + 1);
            }

            return node;
        }
        #endregion
       
        public List<Triangle> Triangles = new List<Triangle>();

        public bool LoadMap(string mapName)
        {
            string filePath = Path.Combine(cs2BaseFolder, _mapsFolder, mapName + ".vpk");
            string triFilePath = Path.Combine(_pathToTris, mapName + ".tri");
            if (!File.Exists(filePath) || !File.Exists(triFilePath))
            {
                Console.WriteLine("Failed To Find Current Map And Or The Tri File. filePath: " + filePath + " " + "triFilePath: " + triFilePath);
                return false;
            }

            if (!LoadTri(triFilePath))
            {
                Console.WriteLine("Failed to load .tri: " + triFilePath);
                return false;
            }
            PreviousMapName = mapName;
            return true;
        }
        private KDNode? KDTreeRoot;
        public bool LoadTri(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            try
            {
                var sw = Stopwatch.StartNew();

                byte[] buffer = File.ReadAllBytes(filePath);

                int triSize = Marshal.SizeOf<Triangle>();
                int numElements = buffer.Length / triSize;

                Triangles = new List<Triangle>(numElements);

                unsafe
                {
                    fixed (byte* p = buffer)
                    {
                        Triangle* pTri = (Triangle*)p;

                        for (int i = 0; i < numElements; i++)
                            Triangles.Add(pTri[i]);
                    }
                }

                KDTreeRoot = BuildKDTree(Triangles);

                Triangles.Clear();
                Triangles.TrimExcess();

                sw.Stop();
                Console.WriteLine($"Loaded {filePath} in {sw.ElapsedMilliseconds}ms");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("LoadTri exception: " + ex);
                return false;
            }
        }
        public bool IsVisible(Vector3 origin, Vector3 end)
        {
            return !RayIntersectsKDTree(KDTreeRoot, origin, end);
        }
    }
}
