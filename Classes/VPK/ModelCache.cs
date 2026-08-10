using Titled_Gui.Classes.VPK.Types;
using Vortice.Direct3D11;

namespace Titled_Gui.Classes.VPK
{
    public sealed class ModelCache(ID3D11Device device) : IDisposable
    {
        public const int VertexStride = 56;
        private readonly ID3D11Device _device = device;
        private readonly Dictionary<string, GpuMesh> _meshes = [];
        private readonly Dictionary<string, Task<SkinnedMeshData?>> _loading = [];

        public GpuMesh? Get(string modelName)
        {
            if (_meshes.TryGetValue(modelName, out var mesh))
                return mesh;

            if (!_loading.ContainsKey(modelName))
                _loading[modelName] = Task.Run(() => VpkModelLoader.Load(modelName));

            return null;
        }

        public void FlushCompleted()
        {
            if (_loading.Count <= 0)
                return;

            foreach (var keyValue in _loading.ToList())
            {
                if (!keyValue.Value.IsCompleted)
                    continue;

                SkinnedMeshData? data = keyValue.Value.Result;
                if (data != null)
                {
                    GpuMesh? gpuMesh = CreateGpuMesh(data.Value);
                    if (gpuMesh != null)
                        _meshes[keyValue.Key] = (GpuMesh)gpuMesh;
                }

                _loading.Remove(keyValue.Key);
            }
        }

        private GpuMesh? CreateGpuMesh(SkinnedMeshData data)
        {
            if (_device == null)
                return null;

            var verts = new SkinnedVertex[data.Positions.Length];
            for (int i = 0; i < data.Positions.Length; i++)
            {
                verts[i] = new SkinnedVertex
                {
                    Position = data.Positions[i],
                    Normal = data.Normals.Length > i ? data.Normals[i] : System.Numerics.Vector3.UnitY,
                    Joint0 = data.BlendIndices[i * 4 + 0],
                    Joint1 = data.BlendIndices[i * 4 + 1],
                    Joint2 = data.BlendIndices[i * 4 + 2],
                    Joint3 = data.BlendIndices[i * 4 + 3],
                    Weights = new System.Numerics.Vector4(data.BlendWeights[i * 4 + 0], data.BlendWeights[i * 4 + 1], data.BlendWeights[i * 4 + 2], data.BlendWeights[i * 4 + 3]),
                };
            }

            var vertexBuffer = _device.CreateBuffer(verts, BindFlags.VertexBuffer, ResourceUsage.Immutable, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            var indexBuffer = _device.CreateBuffer(data.Indices, BindFlags.IndexBuffer, ResourceUsage.Immutable, CpuAccessFlags.None, ResourceOptionFlags.None, 0);
            var indexCount = data.Indices.Length;

            return new GpuMesh
            {
                Data = data,
                VertexBuffer = vertexBuffer,
                IndexBuffer = indexBuffer,
                IndexCount = indexCount
            };
        }

        public void Dispose()
        {
            foreach (var mesh in _meshes.Values)
            {
                mesh.VertexBuffer?.Dispose();
                mesh.IndexBuffer?.Dispose();
            }

            _meshes.Clear();
            _loading.Clear();
        }
    }
}