using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Titled_Gui.Data.Game;

namespace Titled_Gui.Data
{
    public class MatrixSetting
    {
        ViewMatrix ViewMatrix(IntPtr Matrix)
        {
            var matrix = new ViewMatrix();
            var Viewmatrix = GameState.swed.ReadMatrix(Matrix);

            matrix.m11 = Viewmatrix[0];
            matrix.m12 = Viewmatrix[1];
            matrix.m13 = Viewmatrix[2];
            matrix.m14 = Viewmatrix[3];
            matrix.m21 = Viewmatrix[4];
            matrix.m22 = Viewmatrix[5];
            matrix.m23 = Viewmatrix[6];
            matrix.m24 = Viewmatrix[7];
            matrix.m31 = Viewmatrix[8];
            matrix.m32 = Viewmatrix[9];
            matrix.m33 = Viewmatrix[10];
            matrix.m34 = Viewmatrix[11];
            matrix.m41 = Viewmatrix[12];
            matrix.m42 = Viewmatrix[13];
            matrix.m43 = Viewmatrix[14];
            matrix.m44 = Viewmatrix[15];
            return matrix;
        }
    }
}
