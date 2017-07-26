//Copyright (c) 2011 Yolles Partnership Inc.

//This software is provided 'as-is', without any express or implied
//warranty. In no event will the authors be held liable for any damages
//arising from the use of this software.

//Permission is granted to anyone to use this software for any purpose,
//including commercial applications, and to alter it and redistribute it
//freely, subject to the following restrictions:

//   1. The origin of this software must not be misrepresented; you must not
//   claim that you wrote the original software. If you use this software
//   in a product, an acknowledgment in the product documentation would be
//   appreciated but is not required.

//   2. Altered source versions must be plainly marked as such, and must not be
//   misrepresented as being the original software.

//   3. This notice may not be removed or altered from any source
//   distribution.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class Basis
    {
        public GH_Point Point { get; set; }
        public GH_Vector[] Vectors { get; set; }
        public GH_Vector Vector { get { return Vectors[0]; } set { Vectors[0] = value; } }
        public List<int> Axes { get; set; }

        public Basis()
        {
            Point = new GH_Point();
            InitVectorField();
            InitAxes();
        }

        public Basis(GH_Point point)
        {
            Point = point;
            InitVectorField();
            InitAxes();
        }

        public Basis(GH_Point point, GH_Vector vector) : this(point)
        {
            Vectors[0] = vector;
        }

        public Basis(GH_Point point, GH_Vector[] vectors) : this(point)
        {
            Vectors = vectors;
        }

        private void InitAxes()
        {
            Axes = new List<int>();
        }

        private void InitVectorField()
        {
            Vectors = new GH_Vector[3];
            for(int i = 0; i < 3; i++)
                Vectors[i] = new GH_Vector();
        }

        public Vector3d GetBestContinuousVector(Vector3d compare)
        {
            double bestA = 0.0d;
            double testA = 0.0d;
            Vector3d bestV = new Vector3d();
            Vector3d testV = new Vector3d();

            if (Axes.Count == 0 || Axes.Contains(0))
                bestV = GetBestContinuousVector(compare, Vectors[0].Value, -Vectors[0].Value, out bestA);

            if (!Vectors[1].Value.IsZero && Axes.Contains(1))
            {
                testV = GetBestContinuousVector(compare, Vectors[1].Value, -Vectors[1].Value, out testA);

                if (testA < bestA)
                    bestV = testV;
            }

            if (!Vectors[2].Value.IsZero && Axes.Contains(2))
            {
                testV = GetBestContinuousVector(compare, Vectors[2].Value, -Vectors[2].Value, out testA);

                if (testA < bestA)
                    bestV = testV;
            }
            
            return bestV;
        }

        private static Vector3d GetBestContinuousVector(Vector3d start, Vector3d a, Vector3d b, out double angle)
        {
            var va1 = Vector3d.VectorAngle(start, a);
            var va2 = Vector3d.VectorAngle(start, b);

            if (va1 < va2)
            {
                angle = va1;
                return a;
            }

            angle = va2;
            return b;
        }
    }
}
