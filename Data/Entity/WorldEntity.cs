using System.Numerics;
using Titled_Gui.Data.Game;
using static Titled_Gui.Data.Entity.WorldEntityManager;

namespace Titled_Gui.Data.Entity
{
    public class WorldEntity
    {
        public nint PawnAddress {  get; set; }
        public Vector3 Position { get; set; }
        public Vector2 Position2D { get; set; }
        public IntPtr ItemNode { get; set; }
        public EntityKind Type {get; set; }
        public string RawType {get; set; }
        public string DisplayName { get; set; } = "Unknown";
        public Vector3 VecMin { get; set; }
        public Vector3 VecMax { get; set; }
        public Vector3[] Get3DCorners(WorldEntity? worldEntity)
        {
            if (worldEntity == null || float.IsNaN(worldEntity.VecMin.X) ||float.IsNaN(worldEntity.VecMin.Y) ||float.IsNaN(worldEntity.VecMin.Z))
                return Array.Empty<Vector3>();

            return
            [
                worldEntity.Position + new Vector3(worldEntity.VecMin.X, worldEntity.VecMin.Y, worldEntity.VecMin.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMax.X, worldEntity.VecMin.Y, worldEntity.VecMin.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMin.X, worldEntity.VecMax.Y, worldEntity.VecMin.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMax.X, worldEntity.VecMax.Y, worldEntity.VecMin.Z),

                worldEntity.Position + new Vector3(worldEntity.VecMin.X, worldEntity.VecMin.Y, worldEntity.VecMax.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMax.X, worldEntity.VecMin.Y, worldEntity.VecMax.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMin.X, worldEntity.VecMax.Y, worldEntity.VecMax.Z),
                worldEntity.Position + new Vector3(worldEntity.VecMax.X, worldEntity.VecMax.Y, worldEntity.VecMax.Z)
            ];
        }
        public string GetSchemaName()
        {
            var identity = GameState.swed.ReadPointer(this.PawnAddress + Offsets.m_pEntity);

            return GameState.swed.ReadString(identity + Offsets.m_designerName, 32);
        }

    }
}
