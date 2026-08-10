using System.Numerics;
using Vortice.D3DCompiler;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;
using Titled_Gui.Classes.VPK.Types;
using Titled_Gui.Data.Entity.Types;
using Titled_Gui.Classes.Math;

namespace Titled_Gui.Classes.Rendering
{
    public sealed class SkinnedMeshRenderer(ID3D11Device device, ID3D11DeviceContext deviceContext) : IDisposable // w pastes
    {
        private const int _kMaxBones = 128;
        private const int _vertexStride = 56;
        private readonly ID3D11Device _device = device;
        private readonly ID3D11DeviceContext _deviceContext = deviceContext;
        private ID3D11VertexShader? _vertexShader;
        private ID3D11PixelShader? _pixelShader;
        private ID3D11InputLayout? _layout;
        private ID3D11Buffer? _cbConstant;
        private ID3D11Buffer? _cbBones;
        private float[]? _boneScratch;
        private readonly float[] _boneRow = new float[12];
        private object? _lastBones;
        private SkinnedMeshData? _lastBonesMesh;

        public bool CreatePipeline(string vsSource, string psSource)
        {
            if (_device == null)
                return false;

            bool hi = _device.FeatureLevel >= FeatureLevel.Level_11_0;
            string vsT = hi ? "vs_5_0" : "vs_4_0";
            string psT = hi ? "ps_5_0" : "ps_4_0";

            Compiler.Compile(vsSource, "VS", vsT, vsT, out var vsBlob, out var vsErr);
            if (vsBlob == null)
            {
                Console.WriteLine("VertexShader compile failed: " + (vsErr?.AsString() ?? "unknown error"));
                return false;
            }
            _vertexShader = _device.CreateVertexShader(vsBlob, null);

            var layout = new[]
            {
                new InputElementDescription("POSITION", 0, Format.R32G32B32_Float, 0, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("NORMAL", 0, Format.R32G32B32_Float, 12, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("BLENDINDICES", 0, Format.R32G32B32A32_UInt, 24, 0, InputClassification.PerVertexData, 0),
                new InputElementDescription("BLENDWEIGHT", 0, Format.R32G32B32A32_Float, 40, 0, InputClassification.PerVertexData, 0),
            };
            _layout = _device.CreateInputLayout(layout, vsBlob);

            Compiler.Compile(psSource, "PS", psT, psT, out var psBlob, out var psErr);
            if (psBlob == null)
            {
                Console.WriteLine("PixelShader compile failed: " + (psErr?.AsString() ?? "unknown error"));
                return false;
            }

            _pixelShader = _device.CreatePixelShader(psBlob, null);

            _cbConstant = _device.CreateBuffer(144, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);
            _cbBones = _device.CreateBuffer(_kMaxBones * 3 * 16, BindFlags.ConstantBuffer, ResourceUsage.Dynamic, CpuAccessFlags.Write);

            bool success = _vertexShader != null && _pixelShader != null && _layout != null && _cbConstant != null && _cbBones != null;
            return success;
        }

        public void Begin()
        {
            _deviceContext.IASetPrimitiveTopology(PrimitiveTopology.TriangleList);
            _deviceContext.IASetInputLayout(_layout);
            _deviceContext.VSSetShader(_vertexShader);
            _deviceContext.PSSetShader(_pixelShader);
            _deviceContext.VSSetConstantBuffer(0, _cbConstant);
            _deviceContext.VSSetConstantBuffer(1, _cbBones);
            _deviceContext.PSSetConstantBuffer(0, _cbConstant);
        }

        public void UploadConstant(float[] vp, Vector4 color, int style, Vector3 camPos, float glowThickness, float glowIntensity, int occlusionMode, float time)
        {
            if (_deviceContext == null || _cbConstant == null)
                return;

            var mapped = _deviceContext.Map(_cbConstant, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            var span = mapped.AsSpan<float>(36);
            for (int i = 0; i < 16; i++)
                span[i] = vp[i];

            span[16] = color.X;
            span[17] = color.Y;
            span[18] = color.Z;
            span[19] = color.W;
            span[20] = BitConverter.Int32BitsToSingle(style);
            span[21] = camPos.X;
            span[22] = camPos.Y;
            span[23] = camPos.Z;
            span[24] = 0;
            span[25] = 0;
            span[26] = 0;
            span[27] = 0;
            span[28] = glowThickness;
            span[29] = glowIntensity;
            span[30] = BitConverter.Int32BitsToSingle(occlusionMode);
            span[31] = time;
            span[32] = 0f;
            span[33] = 0f;
            span[34] = 0f;
            span[35] = 0f;
            _deviceContext.Unmap(_cbConstant, 0);
        }
        public bool UploadBones(IReadOnlyList<Bone> bones, SkinnedMeshData mesh)
        {
            if (bones == null || bones.Count == 0 || _deviceContext == null || _cbBones == null)
                return false;

            int count = System.Math.Min(bones.Count, _kMaxBones);
            if (_boneScratch == null || _boneScratch.Length < _kMaxBones * 12)
                _boneScratch = new float[_kMaxBones * 12];

            var scratch = _boneScratch;
            var inv = mesh.InvBindPoses;

            if (!(ReferenceEquals(bones, _lastBones) && Equals(mesh, _lastBonesMesh)))
            {
                for (int i = 0; i < count; i++)
                {
                    var b = bones[i];
                    int dst = i * 12;
                    MathUtils.MatFromPosQuat(b.Position, b.Rotation, _boneRow, 0);
                    if (dst + 12 <= inv.Length)
                        MathUtils.MatMulTo(_boneRow, 0, inv, dst, scratch, dst);
                    else
                        Array.Copy(_boneRow, 0, scratch, dst, 12);
                }

                _lastBones = bones;
                _lastBonesMesh = mesh;
            }

            MappedSubresource mapped = _deviceContext.Map(_cbBones, MapMode.WriteDiscard, Vortice.Direct3D11.MapFlags.None);
            Span<float> span = mapped.AsSpan<float>(_kMaxBones * 12);
            scratch.AsSpan(0, count * 12).CopyTo(span);
            _deviceContext.Unmap(_cbBones, 0);

            return true;
        }

        public void DrawMesh(GpuMesh mesh)
        {
            if (_deviceContext == null)
                return;

            _deviceContext.IASetVertexBuffer(0, mesh.VertexBuffer, _vertexStride, 0);
            _deviceContext.IASetIndexBuffer(mesh.IndexBuffer, Format.R32_UInt, 0);
            _deviceContext.DrawIndexed(mesh.IndexCount, 0, 0);
        }

        public void Dispose()
        {
            _vertexShader?.Dispose();
            _pixelShader?.Dispose();
            _layout?.Dispose();
            _cbConstant?.Dispose();
            _cbBones?.Dispose();
            _vertexShader = null;
            _pixelShader = null;
            _layout = null;
            _cbConstant = null;
            _cbBones = null;
        }
    }
}