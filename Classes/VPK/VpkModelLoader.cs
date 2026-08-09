using SteamDatabase.ValvePak;
using System.Buffers.Binary;
using System.Numerics;
using Titled_Gui.Classes.Math;
using Titled_Gui.Classes.VPK.Types;
using ValveResourceFormat;
using ValveResourceFormat.Blocks;
using ValveResourceFormat.ResourceTypes;
using ValveResourceFormat.ResourceTypes.ModelAnimation;
using Bone = ValveResourceFormat.ResourceTypes.ModelAnimation.Bone;

namespace Titled_Gui.Classes.VPK
{
    public static class VpkModelLoader
    {
        public static string NormalizeVmdl(string path) => Normalize(path, ".vmdl", ".vmdl_c");
        public static string NormalizeVmesh(string path) => Normalize(path, ".vmesh", ".vmesh_c");

        private static string Normalize(string path, string src, string cmp)
        {
            path = path.Replace('\\', '/').ToLowerInvariant();
            if (path.EndsWith(cmp, StringComparison.Ordinal))
                return path;

            if (path.EndsWith(src, StringComparison.Ordinal))
                return path + "_c";

            return path + cmp;
        }

        public static SkinnedMeshData? Load(string vmdlPath)
        {
            if (!CS2Utils.Initialize() || CS2Utils.Package == null || string.IsNullOrWhiteSpace(vmdlPath))
                return null;

            Package package = CS2Utils.Package;
            byte[]? modelBytes = ReadEntry(package, NormalizeVmdl(vmdlPath));
            if (modelBytes == null)
                return null;

            Resource resource = new();
            resource.Read(new MemoryStream(modelBytes), false, false);
            Model? model = (Model?)resource.DataBlock;
            if (model == null)
                return null;

            var meshes = model.GetReferenceMeshNamesAndLoD().ToList();
            if (meshes.Count > 0)
            {
                string? meshName = null;
                int meshIndex = -1;
                foreach (var (index, name, lodMask) in meshes)
                {
                    if ((lodMask & 1L) == 0)
                        continue;

                    if (meshName == null || lodMask < 1)
                    {
                        meshName = name;
                        meshIndex = index;
                        if (lodMask == 1)
                            break;
                    }
                }

                if (meshName == null || meshIndex < 0)
                    return null;

                byte[]? meshBytes = ReadEntry(package, NormalizeVmesh(meshName));
                if (meshBytes == null || meshBytes.Length <= 0)
                    return null;

                Resource resource1 = new();
                resource1.Read(new MemoryStream(meshBytes), false, false);
                Mesh? mesh = (Mesh?)resource1.DataBlock;
                int[]? remap1 = model.GetRemapTable(meshIndex);
                if (mesh == null || remap1 == null)
                    return null;

                var vbib = mesh.VBIB;
                if (vbib == null)
                    return null;

                return BuildMesh(vbib, remap1, model);
            }

            var embedded = model.GetEmbeddedMeshesAndLoD().ToList();
            var target = embedded.Where(e => e.Mesh.VBIB != null)
                .OrderByDescending(e => e.Name.Contains("thirdperson", StringComparison.OrdinalIgnoreCase))
                .ThenByDescending(e => e.Name.Contains("body", StringComparison.OrdinalIgnoreCase))
                .Select(e => (e.Mesh, e.MeshIndex))
                .FirstOrDefault();

            int[]? remap = model.GetRemapTable(target.MeshIndex);
            if (target.Mesh == null || target.Mesh.VBIB == null || remap == null)
                return null;

            return BuildMesh(target.Mesh.VBIB, remap, model);
        }

        private static byte[]? ReadEntry(Package pkg, string path)
        {
            PackageEntry? entry = pkg.FindEntry(path);
            if (entry == null)
                return null;

            pkg.ReadEntry(entry, out var data);
            return data;
        }

        private static SkinnedMeshData? BuildMesh(VBIB vbib, int[]? remap, Model model)
        {
            Bone[] bones = model.Skeleton?.Bones ?? [];
            int boneCount = bones.Length;
            int[]? boneParents = [.. bones.Select(b => b.Parent?.Index ?? -1)];
            string[]? boneNames = [.. bones.Select(b => b.Name)];
            if (boneNames == null || boneParents == null || boneCount <= 0 || boneParents.Length <= 0 || boneNames.Length <= 0)
                return null;

            List<Vector3> positions = [];
            List<uint> blendIndices = [];
            List<float> blendWeights = [];
            List<uint> indices = [];

            int partCount = System.Math.Min(vbib.VertexBuffers.Count, vbib.IndexBuffers.Count);

            for (int part = 0; part < partCount; part++)
            {
                VBIB.OnDiskBufferData vb = vbib.VertexBuffers[part];
                VBIB.OnDiskBufferData ib = vbib.IndexBuffers[part];

                VBIB.RenderInputLayoutField? positionField = FindField(vb, "POSITION", 0);
                if (positionField == null)
                    continue;

                VBIB.RenderInputLayoutField? blendIndicesField = FindField(vb, "BLENDINDICES", 0);
                VBIB.RenderInputLayoutField? blendWeightField = FindField(vb, "BLENDWEIGHT", 0) ?? FindField(vb, "BLENDWEIGHT", 1) ?? FindField(vb, "BLENDWEIGHT", 2) ?? FindField(vb, "BLENDWEIGHT", 3);

                Vector3[]? position = VBIB.GetVector3AttributeArray(vb, positionField.Value);
                ushort[]? bi = blendIndicesField == null ? [] : VBIB.GetBlendIndicesArray(vb, blendIndicesField.Value, remap);
                float[]? bw = blendWeightField == null ? [] : DecodeBlendWeights(vb, blendWeightField.Value);

                uint offset = (uint)positions.Count;

                for (int v = 0; v < position.Length; v++)
                {
                    positions.Add(position[v]);

                    for (int k = 0; k < 4; k++)
                    {
                        int boneIdx = bi.Length > 0 ? bi[v * 4 + k] : 0;
                        if (boneCount > 0)
                            boneIdx = System.Math.Clamp(boneIdx, 0, boneCount - 1);

                        boneIdx = CollapseToStable(boneIdx, boneNames, boneParents, boneCount);
                        blendIndices.Add((uint)boneIdx);

                        float weight = bw.Length > 0 ? bw[v * 4 + k] : (k == 0 ? 1f : 0f);
                        blendWeights.Add(weight);
                    }
                }

                int elementSize = (int)ib.ElementSizeInBytes;
                for (int i = 0; i + elementSize <= ib.Data.Length; i += elementSize)
                {
                    uint idx = elementSize == 2 ? BinaryPrimitives.ReadUInt16LittleEndian(ib.Data.AsSpan(i)) : BinaryPrimitives.ReadUInt32LittleEndian(ib.Data.AsSpan(i));
                    indices.Add(idx + offset);
                }
            }

            if (positions.Count <= 0 || indices.Count <= 0)
                return null;

            return new SkinnedMeshData
            {
                Positions = [.. positions],
                Normals = ComputeNormals(positions, indices),
                BlendIndices = [.. blendIndices],
                BlendWeights = [.. blendWeights],
                Indices = [.. indices],
                BoneCount = boneCount,
                InvBindPoses = ComputeInvBindPoses(model, boneCount),
            };
        }

        private static Vector3[] ComputeNormals(List<Vector3> positions, List<uint> indices)
        {
            var normals = new Vector3[positions.Count];
            for (int i = 0; i + 2 < indices.Count; i += 3)
            {
                var a = positions[(int)indices[i]];
                var b = positions[(int)indices[i + 1]];
                var c = positions[(int)indices[i + 2]];
                var n = Vector3.Cross(b - a, c - a);
                normals[(int)indices[i]] += n;
                normals[(int)indices[i + 1]] += n;
                normals[(int)indices[i + 2]] += n;
            }

            for (int i = 0; i < normals.Length; i++)
            {
                if (normals[i].LengthSquared() > 1e-12f)
                    normals[i] = Vector3.Normalize(normals[i]);
                else
                    normals[i] = Vector3.UnitY;
            }

            return normals;
        }

        private static VBIB.RenderInputLayoutField? FindField(VBIB.OnDiskBufferData vb, string semantic, int index)
        {
            foreach (var f in vb.InputLayoutFields)
            {
                if (string.Equals(f.SemanticName, semantic, StringComparison.OrdinalIgnoreCase) && f.SemanticIndex == index)
                    return f;
            }

            return null;
        }

        private static float[] DecodeBlendWeights(VBIB.OnDiskBufferData vb, VBIB.RenderInputLayoutField field)
        {
            int vertexCount = (int)vb.ElementCount;
            int stride = (int)vb.ElementSizeInBytes;
            int offset = (int)field.Offset;
            var data = vb.Data;
            var result = new float[vertexCount * 4];

            for (int v = 0; v < vertexCount; v++)
            {
                int b = v * stride + offset;
                if (field.Format == DXGI_FORMAT.R32G32B32A32_FLOAT)
                {
                    for (int k = 0; k < 4; k++)
                        result[v * 4 + k] = BitConverter.ToSingle(data, b + k * 4);
                }
                else if (field.Format == DXGI_FORMAT.R8G8B8A8_UNORM)
                {
                    for (int k = 0; k < 4; k++)
                        result[v * 4 + k] = data[b + k] / 255f;
                }
                else
                {
                    result[v * 4] = 1f;
                }
            }

            return result;
        }

        private static bool IsUnstable(string name)
        {
            string lower = name.ToLowerInvariant();
            return lower.Contains("jiggle") || lower.Contains("iktarget") || lower.Contains("weaponhier") || lower.Contains("attach");
        }

        private static int CollapseToStable(int bone, string[] names, int[] parents, int boneCount)
        {
            int depth = 0;
            while (bone >= 0 && bone < boneCount && IsUnstable(names[bone]) && depth < 10)
            {
                int parent = parents[bone];
                if (parent < 0 || parent == bone) break;
                bone = parent;
                depth++;
            }
            return bone;
        }

        private static float[] ComputeInvBindPoses(Model model, int boneCount)
        {
            float[] inv = new float[boneCount * 12];
            if (boneCount == 0 || model.Skeleton == null)
                return inv;

            Bone[] bones = model.Skeleton.Bones;
            float[][] world = new float[boneCount][];

            for (int i = 0; i < boneCount; i++)
            {
                Bone? b = bones[i];
                float[]? local = MathUtils.MatFromPosQuat(b.Position, b.Angle);
                if (local == null)
                    return inv;

                var parent = b.Parent?.Index ?? -1;
                world[i] = parent >= 0 && parent < i && world[parent] != null ? MathUtils.MatMul(world[parent], local) : local;

                Array.Copy(MathUtils.MatInvertRigid(world[i]), 0, inv, i * 12, 12);
            }

            return inv;
        }
    }
}
