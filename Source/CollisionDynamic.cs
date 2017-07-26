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
using Rhino.Geometry.Intersect;
using Grasshopper.Kernel.Types;

using System.Windows.Forms;

namespace GrasshopperCs
{
    class CollisionDynamic : IDynamic
    {
        public bool PostProcess { get { return true; } }
        public Param Param { get; set; }
        public bool Accelerated { get { return false; } }

        public CollisionDynamic()
        {
            Param = new Param();
        }

        public void Process(List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            var surfaces = Param["Surfs"] as List<GH_Surface>;            
            var alignList = new List<int>();
                        
            for (int i = 0; i < points.Count; i++)
            {
                var point = points[i].Value;
                var vector = vectors[i].Value;

                foreach (var surf in surfaces)
                {
                    var curve = Curve.CreateControlPointCurve(new Point3d[] { point, point + vector });
                    Curve[] overlaps;
                    Point3d[] intersections;
                    Intersection.CurveBrep(curve, surf.Value, 0.001d, out overlaps, out intersections);
                    
                    if (intersections.Length > 0)
                    {
                        var intersect = intersections[0];
                        
                        // get uv coordiantes of intersection
                        double u, v;
                        surf.Face.ClosestPoint(intersect, out u, out v);                        
                        var surfNormal = surf.Face.NormalAt(u, v);
                        
                        // unitize manually, apply dot product
                        surfNormal *= 1 / surfNormal.Length;
                        surfNormal *= Algos.DotProduct(vector, surfNormal);

                        // black magic
                        vector -= 2 * surfNormal;

                        vectors[i].Value = vector;

                        alignList.Add(i);
                    }
                }
            }

            if (alignList.Count > 0)
                Param["alignAccVectors"] = alignList;
            else
                Param.Remove("alignAccVectors");
        }
    }
}
