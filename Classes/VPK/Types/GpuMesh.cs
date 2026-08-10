using Vortice.Direct3D11;

namespace Titled_Gui.Classes.VPK.Types
{
    public struct GpuMesh
    {
        public SkinnedMeshData Data;
        public ID3D11Buffer VertexBuffer;
        public ID3D11Buffer IndexBuffer;
        public int IndexCount;
    }
}
