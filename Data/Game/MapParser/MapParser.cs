using SteamDatabase.ValvePak;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using Titled_Gui.Classes.VPK;
using ValveResourceFormat;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.ResourceTypes.RubikonPhysics;
using ValveResourceFormat.ResourceTypes.RubikonPhysics.Shapes;
using ValveResourceFormat.Serialization.KeyValues;
using static Titled_Gui.Data.Game.VRF.Types;

namespace Titled_Gui.Data.Game.MapParser
{
    internal class MapParser
    {
        public static string ctPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\pak01_dir.vpk:characters/models/ctm_sas/ctm_sas.vmdl_c"; // : is inside the vp
        public static string tPath = "C:\\Program Files (x86)\\Steam\\steamapps\\common\\Counter-Strike Global Offensive\\game\\csgo\\pak01_dir.vpk:characters/models/ctm_sas/ctm_sas.vmdl_c"; // : is inside the vp
        private static bool _debugging = false;
        private static void Log(string log)
        {
            if (_debugging)
                Console.WriteLine(log);
        }

        public static void Main()
        {
            try
            {
                string? steamPath = CS2Utils.FindSteamPath();
                if (string.IsNullOrEmpty(steamPath))
                {
                    Log("ERROR: Steam not found!");
                    return;
                }

                string? cs2Path = FindCS2Installation(steamPath);
                if (string.IsNullOrEmpty(cs2Path))
                {
                    Log("ERROR: CS2 not found!");
                    return;
                }

                string triOutputDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Titled", "CS2", "External", "Map Data", "tri");
                Directory.CreateDirectory(triOutputDir);

                var officialVpks = GetOfficialVpks(cs2Path);
                var workshopVpks = GetWorkshopVpks(cs2Path);
                var allVpks = officialVpks.Concat(workshopVpks).ToList();

                Log($"Processing {allVpks.Count} maps in parallel...");

                Parallel.ForEach(allVpks, new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount }, vpkFile =>
                {
                    try
                    {
                        Log($"processing {Path.GetFileName(vpkFile)}");
                        ProcessVpkFile(vpkFile, triOutputDir);
                    }
                    catch (Exception ex)
                    {
                        Log($"Error processing {Path.GetFileName(vpkFile)}: {ex.Message}");
                    }
                });

                Log("\nExtraction completed!");
            }
            catch (Exception ex) { Log($"ERROR: {ex.Message}"); }
        }

        static List<string> GetOfficialVpks(string cs2Path)
        {
            string officialMapsPath = Path.Combine(cs2Path, @"game\csgo\maps");
            if (!Directory.Exists(officialMapsPath)) return [];

            return Directory.GetFiles(officialMapsPath, "*.vpk")
                .Where(f =>
                {
                    string name = Path.GetFileName(f).ToLower();
                    return !name.Contains("vanity") && !name.Contains("workshop") &&
                           !name.Contains("graphics_settings") && !name.Contains("lobby_mapveto") &&
                           (name.StartsWith("de_") || name.StartsWith("cs_") || name.StartsWith("ar_"));
                }).ToList();
        }

        static List<string> GetWorkshopVpks(string cs2Path)
        {
            string workshopPath = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(cs2Path)) ?? "", @"workshop\content\730");
            if (!Directory.Exists(workshopPath)) return [];

            return Directory.GetDirectories(workshopPath)
                .SelectMany(dir => Directory.GetFiles(dir, "*.vpk")
                    .Where(f => !Path.GetFileName(f).ToLower().Contains("vanity") &&
                                !Path.GetFileName(f).ToLower().Contains("workshop")))
                .GroupBy(Path.GetDirectoryName)
                .Select(g =>
                {
                    var files = g.ToList();
                    return FindBaseVpkFile(files) ?? files.OrderByDescending(f => new FileInfo(f).Length).First();
                })
                .ToList();
        }

        static string? FindCS2Installation(string steamPath)
        {
            try
            {
                string libraryFoldersPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");

                if (File.Exists(libraryFoldersPath))
                {
                    string content = File.ReadAllText(libraryFoldersPath);
                    var libraryPaths = new List<string>();

                    string[] lines = content.Split('\n');
                    foreach (string line in lines)
                    {
                        if (line.Contains("\"path\""))
                        {
                            int startIndex = line.IndexOf("\"", line.IndexOf("\"path\"") + 6) + 1;
                            int endIndex = line.LastIndexOf("\"");
                            if (startIndex > 0 && endIndex > startIndex)
                            {
                                string path = line.Substring(startIndex, endIndex - startIndex);
                                path = path.Replace("\\\\", "\\");
                                libraryPaths.Add(path);
                            }
                        }
                    }

                    foreach (string libraryPath in libraryPaths)
                    {
                        string cs2Path = Path.Combine(libraryPath, "steamapps", "common", "Counter-Strike Global Offensive");
                        if (Directory.Exists(cs2Path))
                        {
                            return cs2Path;
                        }
                    }
                }

                string defaultCS2Path = Path.Combine(steamPath, "steamapps", "common", "Counter-Strike Global Offensive");
                if (Directory.Exists(defaultCS2Path))
                {
                    return defaultCS2Path;
                }
            }
            catch (Exception ex)
            {
                Log($"Warning: Error finding CS2 installation: {ex.Message}");
            }

            return null;
        }

        static void ProcessOfficialMaps(string steamPath, string? triOutputDir)
        {
            Log("\nProcessing official maps...");

            string? cs2InstallPath = FindCS2Installation(steamPath);
            if (cs2InstallPath == null)
            {
                Log("CS2 installation not found in any Steam library!");
                return;
            }

            string officialMapsPath = Path.Combine(cs2InstallPath, @"game\csgo\maps");

            if (!Directory.Exists(officialMapsPath))
            {
                Log("Official maps directory not found, skipping...");
                return;
            }

            var vpkFiles = Directory.GetFiles(officialMapsPath, "*.vpk")
                .Where(f =>
                {
                    string fileName = Path.GetFileName(f).ToLower();
                    return !fileName.Contains("vanity") && !fileName.Contains("workshop") &&
                           !fileName.Contains("graphics_settings") && !fileName.Contains("lobby_mapveto") &&
                           (fileName.StartsWith("de_") || fileName.StartsWith("cs_") || fileName.StartsWith("ar_"));
                }).ToList();

            Log($"Found {vpkFiles.Count} official map VPK files");
            foreach (string vpkFile in vpkFiles)
            {
                try
                {
                    ProcessVpkFile(vpkFile, triOutputDir);
                }
                catch (Exception ex)
                {
                    Log($"Error processing {Path.GetFileName(vpkFile)}: {ex.Message}");
                }
            }
        }

        static void ProcessWorkshopMaps(string steamPath, string? triOutputDir)
        {
            Log("\nProcessing workshop maps...");
            string? cs2InstallPath = FindCS2Installation(steamPath);
            if (cs2InstallPath == null)
            {
                Log("CS2 installation not found for workshop maps!");
                return;
            }

            // Workshop maps are typically in the Steam library where CS2 is installed
            string steamLibraryPath = Path.GetDirectoryName(Path.GetDirectoryName(cs2InstallPath)) ?? ""; // Go up two levels from CS2 install
            string workshopPath = Path.Combine(steamLibraryPath, @"workshop\content\730") ?? "";

            if (!Directory.Exists(workshopPath))
            {
                Log("Workshop directory not found, skipping...");
                return;
            }

            var workshopDirs = Directory.GetDirectories(workshopPath);
            Log($"Found {workshopDirs.Length} workshop directories");

            foreach (string workshopDir in workshopDirs)
            {
                try
                {
                    var vpkFiles = Directory.GetFiles(workshopDir, "*.vpk")
                        .Where(f => !Path.GetFileName(f).ToLower().Contains("vanity") &&
                                   !Path.GetFileName(f).ToLower().Contains("workshop"))
                        .ToList();

                    if (vpkFiles.Count > 0)
                    {
                        string baseVpk = FindBaseVpkFile(vpkFiles) ?? vpkFiles.OrderByDescending(f => new FileInfo(f).Length).First();
                        ProcessVpkFile(baseVpk, triOutputDir);
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error processing workshop directory {Path.GetFileName(workshopDir)}: {ex.Message}");
                }
            }
        }

        static string? FindBaseVpkFile(List<string> vpkFiles)
        {
            return vpkFiles.FirstOrDefault(vpkFile =>
            {
                string fileName = Path.GetFileNameWithoutExtension(vpkFile);
                return !fileName.EndsWith("_000") && !fileName.EndsWith("_001") &&
                       !fileName.EndsWith("_002") && !fileName.EndsWith("_003") &&
                       !fileName.EndsWith("_004") && !fileName.EndsWith("_005") &&
                       !fileName.EndsWith("_006") && !fileName.EndsWith("_007") &&
                       !fileName.EndsWith("_008") && !fileName.EndsWith("_009");
            });
        }

        static void ProcessVpkFile(string vpkPath, string? triOutputDir)
        {
            Log($"Processing: {Path.GetFileName(vpkPath)}");

            try
            {
                using var package = new Package();
                package.Read(vpkPath);
                if (package.Entries == null)
                    return;

                // Look for nested VPK files (workshop maps)
                if (package.Entries.TryGetValue("vpk", out List<PackageEntry>? value))
                {
                    var mapVpkFiles = value.Where(f => f.DirectoryName?.Equals("maps", StringComparison.OrdinalIgnoreCase) == true).ToList();

                    if (mapVpkFiles.Count != 0)
                    {
                        var mainMapVpk = mapVpkFiles
                            .Where(f => !f.FileName.Contains("_3dsky", StringComparison.OrdinalIgnoreCase) &&
                                       !f.FileName.Contains("_skybox", StringComparison.OrdinalIgnoreCase) &&
                                       !f.FileName.Contains("_sky", StringComparison.OrdinalIgnoreCase))
                            .OrderBy(f => f.FileName.Length)
                            .FirstOrDefault() ?? mapVpkFiles.First();

                        ProcessNestedVpkFile(package, mainMapVpk, triOutputDir);
                        return;
                    }
                }

                ProcessOfficialMapVpk(package, triOutputDir);
            }
            catch (Exception ex)
            {
                Log($"ERROR: Failed to open VPK {Path.GetFileName(vpkPath)}: {ex.Message}");
            }
        }

        static void ProcessNestedVpkFile(Package parentPackage, PackageEntry nestedVpkFile, string? triOutputDir)
        {
            try
            {
                parentPackage.ReadEntry(nestedVpkFile, out byte[] vpkData);
                using (var memoryStream = new MemoryStream(vpkData))
                using (var nestedPackage = new Package())
                {
                    nestedPackage.SetFileName($"{nestedVpkFile.DirectoryName}/{nestedVpkFile.FileName}.vpk");
                    nestedPackage.Read(memoryStream);
                    ProcessOfficialMapVpk(nestedPackage, triOutputDir);
                }
            }
            catch (Exception ex)
            {
                Log($"  Error processing nested VPK: {ex.Message}");
            }
        }

        static void ProcessOfficialMapVpk(Package package, string? triOutputDir)
        {
            var vmdlFiles = package.Entries
                .Where(kvp => kvp.Key.Equals("vmdl_c", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Log($"  Found {vmdlFiles.Count} vmdl_c entries");

            foreach (var entry in vmdlFiles)
            {
                foreach (var file in entry.Value)
                {
                    if (file.FileName.Contains("world_physics", StringComparison.OrdinalIgnoreCase))
                    {
                        string? mapName = ExtractMapNameFromFile(file);
                        if (string.IsNullOrEmpty(mapName)) continue;

                        Log($"  Processing collision data for: {mapName}");
                        ProcessWorldPhysicsFile(package, file, mapName, triOutputDir);
                    }
                }
            }
        }

        static string? ExtractMapNameFromFile(PackageEntry file)
        {
            if (!string.IsNullOrEmpty(file.DirectoryName))
            {
                var parts = file.DirectoryName.Split('/', '\\');
                for (int i = 0; i < parts.Length; i++)
                {
                    if (parts[i].Equals("maps", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length)
                        return parts[i + 1];
                }
                return parts[parts.Length - 1];
            }
            return file.FileName.Replace("world_physics", "").Replace(".vmdl_c", "").Trim('_', '.');
        }

        static void ProcessWorldPhysicsFile(Package package, PackageEntry file, string mapName, string? triOutputDir)
        {
            try
            {
                package.ReadEntry(file, out byte[] fileData);
                using var resource = new Resource();
                resource.Read(new MemoryStream(fileData));

                if (resource.Blocks.FirstOrDefault(b => b.Type == BlockType.PHYS) is not PhysAggregateData phys)
                {
                    Log($"    WARNING: No PHYS block in {mapName}");
                    return;
                }
                var defaultIndices = phys.CollisionAttributes
                                .Select((attr, i) => (attr, i))
                                .Where(x => x.attr.GetStringProperty("m_CollisionGroupString")
                                    .Equals("default", StringComparison.OrdinalIgnoreCase))
                                .Select(x => x.i)
                                .ToHashSet();

                if (defaultIndices.Count == 0) defaultIndices.Add(0);

                var triangles = new List<Triangle>();

                foreach (var part in phys.Parts)
                {
                    var shape = part.Shape;

                    foreach (var hull in shape.Hulls)
                    {
                        if (!defaultIndices.Contains(hull.CollisionAttributeIndex)) continue;
                        ConvertHullToTriangles(hull.Shape, triangles);
                    }

                    foreach (var mesh in shape.Meshes)
                    {
                        if (!defaultIndices.Contains(mesh.CollisionAttributeIndex)) continue;
                        ConvertMeshToTriangles(mesh.Shape, triangles);
                    }
                }

                if (triangles.Count > 0 && triOutputDir != null)
                {
                    WriteTriangleFile(triangles, Path.Combine(triOutputDir, $"{mapName}.tri"));
                    Log($"    Written: {mapName}.tri ({triangles.Count} triangles)");
                }
            }
            catch (Exception ex)
            {
                Log($"    ERROR: {mapName}: {ex.Message}");
            }
        }
        static void ConvertHullToTriangles(Hull hull, List<Triangle> triangles)
        {
            var vertices = hull.GetVertexPositions();
            var edges = hull.GetEdges();
            var faces = hull.GetFaces();

            foreach (var face in faces)
            {
                int startEdge = face.Edge;
                if (startEdge >= edges.Length) continue;

                int edge = edges[startEdge].Next;
                int iterations = 0;

                while (edge != startEdge && iterations++ < 1000)
                {
                    if (edge >= edges.Length) break;
                    int nextEdge = edges[edge].Next;
                    if (nextEdge >= edges.Length) break;

                    int i0 = edges[startEdge].Origin;
                    int i1 = edges[edge].Origin;
                    int i2 = edges[nextEdge].Origin;

                    if (i0 < vertices.Length && i1 < vertices.Length && i2 < vertices.Length)
                        triangles.Add(new Triangle { Point1 = vertices[i0], Point2 = vertices[i1], Point3 = vertices[i2] });

                    edge = nextEdge;
                }
            }
        }

        static void ConvertMeshToTriangles(ValveResourceFormat.ResourceTypes.RubikonPhysics.Shapes.Mesh mesh, List<Triangle> triangles)
        {
            var vertices = mesh.GetVertices();
            var triangleIndices = mesh.GetTriangles();

            for (int i = 0; i < triangleIndices.Length; i++)
            {
                var tri = triangleIndices[i];
                if (tri.X < vertices.Length && tri.Y < vertices.Length && tri.Z < vertices.Length)
                    triangles.Add(new Triangle { Point1 = vertices[tri.X], Point2 = vertices[tri.Y], Point3 = vertices[tri.Z] });
            }
        }

        static List<int> GetCollisionAttributeIndices(KV3Parser parser)
        {
            var indices = new List<int>();
            int index = 0;

            while (true)
            {
                string path = $"m_collisionAttributes[{index}].m_CollisionGroupString";
                string collisionGroup = parser.GetValue(path);

                if (string.IsNullOrEmpty(collisionGroup))
                    break;

                // Remove quotes and check for default group (more flexible matching)
                string cleanGroup = collisionGroup.Trim('"').Trim();
                if (cleanGroup.Equals("default", StringComparison.OrdinalIgnoreCase) ||
                    cleanGroup.Equals("Default", StringComparison.Ordinal) ||
                    cleanGroup == "0") // Sometimes default is represented as "0"
                {
                    indices.Add(index);
                }

                index++;
            }

            // If no default collision groups found, add index 0 as fallback
            if (indices.Count == 0)
            {
                indices.Add(0);
            }

            return indices;
        }

        static void ProcessHulls(KV3Parser parser, List<int> collisionIndices, List<VRF.Types.Triangle> triangles)
        {
            int index = 0;
            int processedHulls = 0;

            while (true)
            {
                string path = $"m_parts[0].m_rnShape.m_hulls[{index}].m_nCollisionAttributeIndex";
                string collisionIndexStr = parser.GetValue(path);

                if (string.IsNullOrEmpty(collisionIndexStr))
                    break;

                if (int.TryParse(collisionIndexStr, out int collisionIndex) && collisionIndices.Contains(collisionIndex))
                {
                    // Try multiple vertex data paths (like the C++ parser)
                    string vertexData = null;

                    // First try m_VertexPositions
                    string vertexPath = $"m_parts[0].m_rnShape.m_hulls[{index}].m_Hull.m_VertexPositions";
                    vertexData = parser.GetValue(vertexPath);

                    // If empty, try m_Vertices
                    if (string.IsNullOrEmpty(vertexData))
                    {
                        vertexPath = $"m_parts[0].m_rnShape.m_hulls[{index}].m_Hull.m_Vertices";
                        vertexData = parser.GetValue(vertexPath);
                    }

                    if (!string.IsNullOrEmpty(vertexData))
                    {
                        try
                        {
                            var vertices = ParseFloatArray(vertexData);
                            var faces = ParseByteArray(parser.GetValue($"m_parts[0].m_rnShape.m_hulls[{index}].m_Hull.m_Faces"));
                            var edges = ParseEdgeArray(parser.GetValue($"m_parts[0].m_rnShape.m_hulls[{index}].m_Hull.m_Edges"));

                            if (vertices.Count > 0 && faces.Count > 0 && edges.Count > 0)
                            {
                                int trianglesBefore = triangles.Count;
                                ConvertHullToTriangles(vertices, faces, edges, triangles);
                                int trianglesAdded = triangles.Count - trianglesBefore;
                                processedHulls++;
                            }
                        }
                        catch (Exception ex)
                        {
                            Log($"    ERROR processing hull {index}: {ex.Message}");
                        }
                    }
                }

                index++;
            }

            Log($"    Total hulls processed: {processedHulls}");
        }

        static void ProcessMeshes(KV3Parser parser, List<int> collisionIndices, List<VRF.Types.Triangle> triangles)
        {
            int index = 0;
            int processedMeshes = 0;

            while (true)
            {
                string path = $"m_parts[0].m_rnShape.m_meshes[{index}].m_nCollisionAttributeIndex";
                string collisionIndexStr = parser.GetValue(path);

                if (string.IsNullOrEmpty(collisionIndexStr))
                    break;

                if (int.TryParse(collisionIndexStr, out int collisionIndex) && collisionIndices.Contains(collisionIndex))
                {
                    try
                    {
                        var triangleIndices = ParseIntArray(parser.GetValue($"m_parts[0].m_rnShape.m_meshes[{index}].m_Mesh.m_Triangles"));
                        var vertices = ParseFloatArray(parser.GetValue($"m_parts[0].m_rnShape.m_meshes[{index}].m_Mesh.m_Vertices"));

                        if (vertices.Count > 0 && triangleIndices.Count > 0)
                        {
                            int trianglesBefore = triangles.Count;
                            ConvertMeshToTriangles(vertices, triangleIndices, triangles);
                            int trianglesAdded = triangles.Count - trianglesBefore;
                            processedMeshes++;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"    ERROR processing mesh {index}: {ex.Message}");
                    }
                }
                index++;
            }

            Log($"    Total meshes processed: {processedMeshes}");
        }

        static void ConvertHullToTriangles(List<Vector3> vertices, List<byte> faces, List<Edge> edges, List<VRF.Types.Triangle> triangles)
        {
            foreach (byte startEdge in faces)
            {
                if (startEdge >= edges.Count) continue;

                int edge = edges[startEdge].Next;
                int iterations = 0;
                const int maxIterations = 1000; // Increased safety limit for complex hulls

                while (edge != startEdge && iterations < maxIterations)
                {
                    if (edge >= edges.Count) break;

                    int nextEdge = edges[edge].Next;
                    if (nextEdge >= edges.Count) break;

                    // Ensure valid vertex indices
                    if (edges[startEdge].Origin < vertices.Count &&
                        edges[edge].Origin < vertices.Count &&
                        edges[nextEdge].Origin < vertices.Count)
                    {
                        triangles.Add(new VRF.Types.Triangle
                        {
                            Point1 = vertices[edges[startEdge].Origin],
                            Point2 = vertices[edges[edge].Origin],
                            Point3 = vertices[edges[nextEdge].Origin]
                        });
                    }

                    edge = nextEdge;
                    iterations++;
                }
            }
        }

        static void ConvertMeshToTriangles(List<Vector3> vertices, List<int> triangleIndices, List<VRF.Types.Triangle> triangles)
        {
            for (int i = 0; i < triangleIndices.Count; i += 3)
            {
                triangles.Add(new VRF.Types.Triangle
                {
                    Point1 = vertices[triangleIndices[i]],
                    Point2 = vertices[triangleIndices[i + 1]],
                    Point3 = vertices[triangleIndices[i + 2]]
                });
            }
        }

        static List<Vector3> ParseFloatArray(string data)
        {
            var floats = ParseFloatBytes(data);
            var vertices = new List<Vector3>();

            for (int i = 0; i < floats.Count; i += 3)
            {
                vertices.Add(new Vector3 { X = floats[i], Y = floats[i + 1], Z = floats[i + 2] });
            }

            return vertices;
        }

        static List<byte> ParseByteArray(string data)
        {
            return ParseBytes(data);
        }

        static List<Edge> ParseEdgeArray(string data)
        {
            var bytes = ParseBytes(data);
            var edges = new List<Edge>();

            for (int i = 0; i < bytes.Count; i += 4)
            {
                edges.Add(new Edge
                {
                    Next = bytes[i],
                    Twin = bytes[i + 1],
                    Origin = bytes[i + 2],
                    Face = bytes[i + 3]
                });
            }

            return edges;
        }

        static List<byte> ParseBytes(string data)
        {
            data = data.Trim();
            if (data.StartsWith("#[") && data.EndsWith("]"))
                data = data.Substring(2, data.Length - 3);

            var bytes = new List<byte>();
            var span = data.AsSpan();
            int i = 0;
            while (i < span.Length)
            {
                while (i < span.Length && (span[i] == ' ' || span[i] == '\t' || span[i] == '\r' || span[i] == '\n')) i++;
                if (i + 2 <= span.Length && IsHex(span[i]) && IsHex(span[i + 1]))
                {
                    bytes.Add((byte)(HexVal(span[i]) << 4 | HexVal(span[i + 1])));
                    i += 2;
                }
                else i++;
            }
            return bytes;
        }

        static bool IsHex(char c) => (c >= '0' && c <= '9') || (c >= 'A' && c <= 'F') || (c >= 'a' && c <= 'f');
        static int HexVal(char c) => c >= 'a' ? c - 'a' + 10 : c >= 'A' ? c - 'A' + 10 : c - '0';

        static List<float> ParseFloatBytes(string data)
        {
            var bytes = ParseBytes(data);
            var floats = new float[bytes.Count / 4];
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(
                System.Runtime.InteropServices.CollectionsMarshal.AsSpan(bytes).Slice(0, floats.Length * 4)
            ).CopyTo(floats);
            return [.. floats];
        }

        static List<int> ParseIntArray(string data)
        {
            var bytes = ParseBytes(data);
            var ints = new int[bytes.Count / 4];
            System.Runtime.InteropServices.MemoryMarshal.Cast<byte, int>(
                System.Runtime.InteropServices.CollectionsMarshal.AsSpan(bytes).Slice(0, ints.Length * 4)
            ).CopyTo(ints);
            return [.. ints];
        }

        static void WriteTriangleFile(List<VRF.Types.Triangle> triangles, string outputPath)
        {
            using (var fs = new FileStream(outputPath, FileMode.Create))
            using (var writer = new BinaryWriter(fs))
            {
                foreach (var triangle in triangles)
                {
                    writer.Write(triangle.Point1.X);
                    writer.Write(triangle.Point1.Y);
                    writer.Write(triangle.Point1.Z);
                    writer.Write(triangle.Point2.X);
                    writer.Write(triangle.Point2.Y);
                    writer.Write(triangle.Point2.Z);
                    writer.Write(triangle.Point3.X);
                    writer.Write(triangle.Point3.Y);
                    writer.Write(triangle.Point3.Z);
                }
            }
        }
    }
}
