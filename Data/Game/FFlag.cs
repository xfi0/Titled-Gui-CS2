using System;
using System.Collections.Generic;
using System.Text;

namespace Titled_Gui.Data.Game
{
    internal class FFlag
    {
        public enum FFlagStates
        {
            None = 0,
            Standing = 65665,
            Crouching = 65667,
            idk = 65664,
            OnGround = 1 << 0,
        }
    }
}
