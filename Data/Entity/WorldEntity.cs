using System.Numerics;
using Titled_Gui.Data.Game;
using ValveResourceFormat.ResourceTypes.ModelAnimation.SegmentDecoders;
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
        /// <summary>
        /// 4x4 Matrix
        /// </summary>
        public float[] Matrix { get; set; }
        public float[] Rotation { get; set; }
        public Vector3[] Get3DCorners(WorldEntity? worldEntity)
        {
            if (worldEntity == null || float.IsNaN(worldEntity.VecMin.X) ||float.IsNaN(worldEntity.VecMin.Y) ||float.IsNaN(worldEntity.VecMin.Z))
                return Array.Empty<Vector3>();

            Vector3[] localCorners =
            [
                worldEntity.VecMin,
                new(worldEntity.VecMin.X, worldEntity.VecMax.Y, worldEntity.VecMin.Z),
                new(worldEntity.VecMax.X, worldEntity.VecMax.Y, worldEntity.VecMin.Z),
                new(worldEntity.VecMax.X, worldEntity.VecMin.Y, worldEntity.VecMin.Z),
                new(worldEntity.VecMin.X, worldEntity.VecMin.Y, worldEntity.VecMax.Z),
                new(worldEntity.VecMin.X, worldEntity.VecMax.Y, worldEntity.VecMax.Z),
                worldEntity.VecMax,
                new(worldEntity.VecMax.X, worldEntity.VecMin.Y, worldEntity.VecMax.Z),
            ];
            float pitch = worldEntity.Rotation[0] * (MathF.PI / 180f);
            float yaw = worldEntity.Rotation[1] * (MathF.PI / 180f);
            float roll = worldEntity.Rotation[2] * (MathF.PI / 180f);

            float cy = MathF.Cos(yaw * 0.5f), sy = MathF.Sin(yaw * 0.5f);
            float cp = MathF.Cos(pitch * 0.5f), sp = MathF.Sin(pitch * 0.5f);
            float cr = MathF.Cos(roll * 0.5f), sr = MathF.Sin(roll * 0.5f);

            float qW = cr * cp * cy + sr * sp * sy;
            float qX = sr * cp * cy - cr * sp * sy;
            float qY = cr * sp * cy + sr * cp * sy;
            float qZ = cr * cp * sy - sr * sp * cy;


            Vector3[] worldCorners = localCorners
                .Select(c => RotateByQuaternion(qX, qY, qZ, qW, c) + worldEntity.Position)
                .ToArray();

            return worldCorners;
        }
        public static Vector3 RotateByQuaternion(float qX, float qY, float qZ, float qW, Vector3 v)
        {
            Vector3 u = new(qX, qY, qZ);
            float s = qW;

            return 2f * Vector3.Dot(u, v) * u
                   + (s * s - Vector3.Dot(u, u)) * v
                   + 2f * s * Vector3.Cross(u, v);
        }
        public static Vector3 RotateCorner(Vector3 origin, float x, float y, float z,
            Vector3 right, Vector3 forward, Vector3 up)
        {
            return origin + right * x + forward * +up * z;
        }

        public string GetSchemaName()
        {
            var identity = GameState.swed.ReadPointer(this.PawnAddress + Offsets.m_pEntity);

            return GameState.swed.ReadString(identity + Offsets.m_designerName, 32);
        }

    }
}
