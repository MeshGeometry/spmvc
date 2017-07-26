// MIT License

// Copyright (c) 2017 Mesh Consultants Inc

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
