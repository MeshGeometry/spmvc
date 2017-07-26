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
