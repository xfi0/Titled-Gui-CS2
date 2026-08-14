using System;
using System.Collections.Generic;
using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Titled_Gui.Data.Menu.Types
{
    public class BulletTracer(Vector3 startPoint, Vector3 intersectPoint, float TimeLeft)
    {
        public Vector3 StartPoint = startPoint; // former eye pos
        public Vector3 IntersectPoint = intersectPoint; // where it hits, eye -> end is tracer
        public float TimeLeft = TimeLeft;
        public float TotalTime = TimeLeft;
    }
}
