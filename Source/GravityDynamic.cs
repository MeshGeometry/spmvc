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
using System.Windows.Forms;

using Rhino.Geometry;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    class GravityDynamic : IDynamic
    {
        public bool PostProcess { get { return false; } }
        public Param Param { get; set; }
        public bool Accelerated { get { return false; } }

        public GravityDynamic()
        {
            Param = new Param();
        }

        public void Process(List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            var sp = Param["SP"] as List<GH_Point>;
            var h = (double)Param["h"];
            var k = (double)Param["k"];
            var e = (bool)Param["e"];

            GH_Vector nv = new GH_Vector();
            double u1, v1, u2, v2;
            u1 = u2 = v1 = v2 = 0.0d;

            for (int i = 0; i < points.Count; i++)
            {
                foreach (var p in sp)
                {
                    if (surface.IsValid)
                    {
                        if (!e)
                        {
                            surface.Face.ClosestPoint(points[i].Value, out u1, out v1);
                            var surfPl = new Plane(points[i].Value, surface.Face.NormalAt(u1, v1));

                            Point3d remap;
                            surfPl.RemapToPlaneSpace(p.Value, out remap);

                            var dir = surfPl.PointAt(remap.X, remap.Y) - surfPl.Origin;
                            dir.Unitize();

                            surface.Face.ClosestPoint(p.Value, out u2, out v2);

                            Point2d uv1 = new Point2d(u1, v1);
                            Point2d uv2 = new Point2d(u2, v2);

                            var dis = uv1.DistanceTo(uv2);
                            dir *= (k / Math.Pow(dis, h));
                            
                            nv.Value += dir;
                        }
                        else
                        {
                            surface.Face.ClosestPoint(points[i].Value, out u1, out v1);
                            surface.Face.ClosestPoint(p.Value, out u2, out v2);

                            var p1 = new Point2d(u1, v1);
                            var p2 = new Point2d(u2, v2);

                            var c = surface.Face.ShortPath(p1, p2, 0.001d);
                            var v = c.TangentAtStart;

                            nv = new GH_Vector(v);
                            nv.Value.Unitize();
                            nv.Value *= (k / Math.Pow(c.GetLength(), 1d + h));
                        }
                    }
                    else
                    {
                        nv = new GH_Vector(p.Value - points[i].Value);
                        nv.Value.Unitize();
                        nv.Value *= (k / Math.Pow(points[i].Value.DistanceTo(p.Value), 1d + h));                       
                    }

                    vectors[i].Value += nv.Value;
                }
            }
        }
    }
}
