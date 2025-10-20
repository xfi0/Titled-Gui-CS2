using System.Numerics;

namespace Titled_Gui.Data.Entity
{
    public class Entity
    {
        public Vector3 Position { get; set; } //entity Position in 3D space
        public Vector3 View { get; set; } // View offset of the entity
        public Vector3 Origin { get; set; } // Origin of the entity
        public Vector2 Position2D { get; set; } // entity Position in 2D space (screen space)
        public Vector2 ViewPosition2D { get; set; } // View offset Position in 2D space (screen space)
        public Vector2 Head2D { get; set; } // Head Position in 2D space (screen space)
        public Vector3 Head { get; set; } // Head Position of the entity in 3D space
        public int LifeState { get; set; } // life state of the entity
        public int Team { get; set; } //Team of the entity
        public int Health { get; set; } // Health of the entity
        public bool Visible { get; set; } // visibility of the entity
        public float Distance { get; set; } // Distance to the entity,from the local player
        public IntPtr PawnAddress { get; set; } // pointer to the entity's pawn address
        public List<Vector3>? Bones { get; set; } // list of Bones for the entity
        public List<Vector2>? Bones2D { get; set; } // list of Bones in 2D space (screen space)
        public IntPtr dwSensitivity { get; set; } // sensitivity for the local player
        public float Sensitivity { get; set; } // sensitivity for the local player
        public string? Name { get; set; }
        public int Ammo { get; set; }
        public int Account, CashSpent, CashSpentTotal; 
        public bool IsScoped { get; set; }
        public IntPtr? CurrentWeapon { get; set; }
        public short? WeaponIndex { get; set; }
        public string? CurrentWeaponName { get; set; }
        public int PlayerIndex { get; set; }
        public int Armor {  get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 ViewAngles { get; set; }
        public Vector3 EyeDirection { get; set; }
        public Vector3 ViewDirection { get; set; }
        public Vector3 AimPunchAngle { get; set; }
        public Vector3 AimPunchAngleVel { get; set; }
        public Vector3 AimPunchCache { get; set; }
        public int AimPunchTickBase { get; set; }
        public int AimPunchTickFraction { get; set; }
        public bool IsAttacking { get; set; }
        public bool IsShooting { get; set; }
        public bool IsBuyMenuOpen { get; set; }
        public bool IsEnemy { get; set; }
        public bool IsFlashed { get; set; }
        public int ShotsFired { get; set; }
        public int Ping { get; set; }
        public bool IsWalking { get; set; }
        public bool HasBomb { get; set; }
        public bool IsDefusing { get; set; }
        public bool InBombZone { get; set; }
        public IntPtr SpottedByState { get; set; }
        public Vector3 AngEyeAngles { get; set; }
        public Vector3 GunGameImmunityColor { get; set; }
    }
}
