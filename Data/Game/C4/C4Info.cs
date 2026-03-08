using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Titled_Gui.Classes;
using Titled_Gui.Data.Entity;
using static Titled_Gui.Data.Entity.Types;

namespace Titled_Gui.Data.Game.C4
{
    internal class C4Info : ThreadService
    {
        public static Types.C4? C4 = null;
        /// <summary>
        /// pointer to a C_PlantedC4, if available.
        /// </summary>
        /// <returns>A pointer to a C_PlantedC4. otherwise, <see cref="IntPtr.Zero"/></returns>
        public IntPtr GetC4()
        {
            IntPtr plantedPointer = GameState.swed.ReadPointer(GameState.client + Offsets.dwPlantedC4);
            if (plantedPointer == IntPtr.Zero)
                return plantedPointer == IntPtr.Zero ? IntPtr.Zero : plantedPointer;

            IntPtr plantedPointerDeref = GameState.swed.ReadPointer(plantedPointer);

            return plantedPointerDeref;
        }
        /// <summary>
        /// pointer to the c4's game scene node, if available.
        /// </summary>
        /// <returns>A pointer to the game scene node if the C4 is planted. otherwise, <see cref="IntPtr.Zero"/>.</returns>
        private IntPtr GetNode()
        {
            IntPtr planted = GetC4();
            if (planted == IntPtr.Zero)
                return IntPtr.Zero;

            return GameState.swed.ReadPointer(planted + Offsets.m_pGameSceneNode);
        }

        private Vector3 GetPos()
        {
            IntPtr node = GetNode();

            if (node == IntPtr.Zero)
                return new Vector3(0, 0, 0);

            return GameState.swed.ReadVec(node + Offsets.m_vecOrigin);
        }

        protected override void FrameAction()
        {
            IntPtr c4 = GetC4();
            IntPtr node = GetNode();
            Vector3 position = GetPos();
            float[] viewMatrix = GameState.swed.ReadMatrix(GameState.client + Offsets.dwViewMatrix);

            if (c4 == IntPtr.Zero || node == IntPtr.Zero || position == new Vector3(0, 0, 0)) 
                return;
            
            C4 = new Types.C4()
            {
                Address = c4,
                ExplosionTime = GameState.swed.ReadFloat(c4 + Offsets.m_flC4Blow) - GlobalVar.GetCurrentTime(),
                Position = position,
                Position2D = Calculate.WorldToScreen(viewMatrix, position),
                PlantedSite = (BombSite)GameState.swed.ReadInt(c4, Offsets.m_nBombSite),
                BeingDefused = GameState.swed.ReadBool(c4 + Offsets.m_bBeingDefused),
                Planted = GameState.swed.ReadBool(c4 + Offsets.m_bC4Activated),
                Matrix = GameState.swed.ReadMatrix(node + Offsets.m_nodeToWorld)
            };

            Thread.SpinWait(20);
        }
    }
}
