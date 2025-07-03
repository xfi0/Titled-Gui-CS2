using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Titled_Gui.Data
{
    public class Entity
    {
        public Vector3 position { get; set; } //entity position in 3D space
        public Vector3 view { get; set; } // view offset of the entity
        public Vector3 origin { get; set; } // origin of the entity
        public Vector2 position2D { get; set; } // entity position in 2D space (screen space)
        public Vector2 viewPosition2D { get; set; } // view offset position in 2D space (screen space)
        public Vector2 head2D { get; set; } // head position in 2D space (screen space)
        public Vector3 head { get; set; } // head position of the entity in 3D space
        public int LifeState { get; set; } // life state of the entity
        public int team { get; set; } //team of the entity
        public int health { get; set; } // health of the entity
        public bool Visible { get; set; } // visibility of the entity
        public float distance { get; set; } // distance to the entity,from the local player
        public IntPtr PawnAddress { get; set; } // pointer to the entity's pawn address
        public List<Vector3> bones { get; set; } // list of bones for the entity
        public List<Vector2> bones2D { get; set; } // list of bones in 2D space (screen space)
        public IntPtr dwSensitivity { get; set; } // sensitivity for the local player
        public float Sensitivity { get; set; } // sensitivity for the local player
        public string Name { get; set; }
        public IntPtr HeldWeapon { get; set; }
        public string HeldWeaponName { get; set; }
        public IntPtr WeaponIndex { get; set; }

    }
}
