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
        public string DisplayName { get; set; } = "Unknown";
        public string GetSchemaName()
        {
            var identity = GameState.swed.ReadPointer(this.PawnAddress + Offsets.m_pEntity);

            return GameState.swed.ReadString(identity + Offsets.m_designerName, 32);
        }

    }
}
