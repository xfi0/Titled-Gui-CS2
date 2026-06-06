using System.Diagnostics;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using Titled_Gui.Classes.Math;
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
            if (node == null)
                return false;

            Vector3 dir = end - origin;
            Vector3 invDir = new(
                dir.X == 0f ? float.MaxValue : 1f / dir.X,
                dir.Y == 0f ? float.MaxValue : 1f / dir.Y,
                dir.Z == 0f ? float.MaxValue : 1f / dir.Z
            );

            return RayIntersectsKDTreeInternal(node, origin, end, invDir);
        }

        private bool RayIntersectsKDTreeInternal(KDNode? node, Vector3 origin, Vector3 end, Vector3 invDir)
        {
            if (node == null) 
                return false;

            if (!node.bbox.Intersect(origin, end, invDir))
                return false;

            if (node.Triangles.Length > 0)
            {
                Vector3 dir = end - origin;
                foreach (Triangle tri in node.Triangles)
                    if (tri.Intersect(origin, dir))
                        return true;

                return false;
            }

            if (RayIntersectsKDTreeInternal(node.Left, origin, end, invDir))
                return true;

            return RayIntersectsKDTreeInternal(node.Right, origin, end, invDir);
        }
        private BoundingBox CalculateBoundingBox(List<Triangle> triangles)
        {
            Triangle first = triangles[0];
            Vector3 min = first.Point1;
            Vector3 max = first.Point1;

            foreach (Triangle tri in triangles)
            {
                if (tri.Point1.X < min.X) 
                    min.X = tri.Point1.X; 

                else if (tri.Point1.X > max.X) 
                    max.X = tri.Point1.X;

                if (tri.Point1.Y < min.Y) 
                    min.Y = tri.Point1.Y; 

                else if (tri.Point1.Y > max.Y) 
                    max.Y = tri.Point1.Y;

                if (tri.Point1.Z < min.Z) 
                    min.Z = tri.Point1.Z; 

                else if (tri.Point1.Z > max.Z) 
                    max.Z = tri.Point1.Z;

                if (tri.Point2.X < min.X)
                    min.X = tri.Point2.X; 

                else if (tri.Point2.X > max.X)
                    max.X = tri.Point2.X;

                if (tri.Point2.Y < min.Y) 
                    min.Y = tri.Point2.Y; 

                else if (tri.Point2.Y > max.Y) 
                    max.Y = tri.Point2.Y;

                if (tri.Point2.Z < min.Z) 
                    min.Z = tri.Point2.Z; 

                else if (tri.Point2.Z > max.Z) 
                    max.Z = tri.Point2.Z;

                if (tri.Point3.X < min.X) 
                    min.X = tri.Point3.X; 

                else if (tri.Point3.X > max.X)
                    max.X = tri.Point3.X;

                if (tri.Point3.Y < min.Y) min.Y = tri.Point3.Y; 
                else if (tri.Point3.Y > max.Y) 
                    max.Y = tri.Point3.Y;

                if (tri.Point3.Z < min.Z)
                    min.Z = tri.Point3.Z; 

                else if (tri.Point3.Z > max.Z) 
                    max.Z = tri.Point3.Z;
            }

            return new BoundingBox { Min = min, Max = max };
        }
        private KDNode? BuildKDTree(List<Triangle> triangles, int depth = 0)
        {
            if (triangles.Count <= 0) 
                return null;

            KDNode node = new();
            node.Axis = depth % 3;
            node.bbox = CalculateBoundingBox(triangles);

            if (triangles.Count <= 3)
            {
                node.Triangles = [.. triangles];
                return node;
            }

            int axis = node.Axis;

            float[] centers = new float[triangles.Count];
            for (int i = 0; i < triangles.Count; i++)
            {
                Triangle t = triangles[i];
                centers[i] = axis switch
                {
                    0 => (t.Point1.X + t.Point2.X + t.Point3.X) / 3f,
                    1 => (t.Point1.Y + t.Point2.Y + t.Point3.Y) / 3f,
                    _ => (t.Point1.Z + t.Point2.Z + t.Point3.Z) / 3f,
                };
            }

            int[] indices = Enumerable.Range(0, triangles.Count).ToArray();
            Array.Sort(indices, (a, b) => centers[a].CompareTo(centers[b]));

            int mid = triangles.Count / 2;

            var leftTriangles = new List<Triangle>(mid);
            var rightTriangles = new List<Triangle>(triangles.Count - mid);

            for (int i = 0; i < mid; i++) leftTriangles.Add(triangles[indices[i]]);
            for (int i = mid; i < triangles.Count; i++) rightTriangles.Add(triangles[indices[i]]);

            if (depth == 0 && triangles.Count > 1000)
            {
                KDNode? left = null, right = null;
                Parallel.Invoke(
                    () => left = BuildKDTree(leftTriangles, depth + 1),
                    () => right = BuildKDTree(rightTriangles, depth + 1)
                );
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
            var asm = Assembly.GetExecutingAssembly();
            var names = asm.GetManifestResourceNames();
          
            var stream = asm.GetManifestResourceStream("Titled_Gui.Data.Game.MapParser.PreExtractedMapData.tri." + mapName + ".tri");
            if (stream == null)
            {
                Console.WriteLine("Failed To Find Current Map And Or The Tri File. filePath: " + "Titled_Gui.Game.MapParser.PreExtractedMapData.tri." + mapName + ".tri");
                return false;
            }

            var triData = new byte[stream.Length];
            stream.ReadExactly(triData);

            if (!LoadTri(triData, mapName))
            {
                Console.WriteLine("Failed to load .tri: " + "Titled_Gui.Game.MapParser.PreExtractedMapData.tri." + mapName + ".tri");
                return false;
            }

            PreviousMapName = mapName;
            return true;
        }
        private KDNode? KDTreeRoot;
        public bool LoadTri(byte[] triData, string mapName)
        {
            try
            {
                var sw = Stopwatch.StartNew();

                int triSize = Marshal.SizeOf<Triangle>();
                int numElements = triData.Length / triSize;

                Triangles = new List<Triangle>(numElements);

                unsafe
                {
                    fixed (byte* p = triData)
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
                Console.WriteLine($"Loaded {mapName} in {sw.ElapsedMilliseconds}ms");

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
