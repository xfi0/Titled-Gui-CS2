using Swed64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titled_Gui;
using Titled_Gui.Data;

namespace Titled_Gui.Data
{
    public class MatrixSetting
    {
        ViewMatrix viewMatrix(IntPtr Matrix)
        {
            var matrix = new ViewMatrix();
            var viewmatrix = GameState.swed.ReadMatrix(Matrix);

            matrix.m11 = viewmatrix[0];
            matrix.m12 = viewmatrix[1];
            matrix.m13 = viewmatrix[2];
            matrix.m14 = viewmatrix[3];
            matrix.m21 = viewmatrix[4];
            matrix.m22 = viewmatrix[5];
            matrix.m23 = viewmatrix[6];
            matrix.m24 = viewmatrix[7];
            matrix.m31 = viewmatrix[8];
            matrix.m32 = viewmatrix[9];
            matrix.m33 = viewmatrix[10];
            matrix.m34 = viewmatrix[11];
            matrix.m41 = viewmatrix[12];
            matrix.m42 = viewmatrix[13];
            matrix.m43 = viewmatrix[14];
            matrix.m44 = viewmatrix[15];
            return matrix;
        }
    }
}
