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
    class VortexDynamic : IDynamic
    {
        public bool PostProcess { get { return false; } }
        public Param Param { get; set; }
        public bool Accelerated { get { return false; } }

        public VortexDynamic()
        {
            Param = new Param();
        }

        public void Process(List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            var planes = Param["Pl"] as List<GH_Plane>;
            var h = (double)Param["h"];
            var k = (double)Param["k"];
            var a = (double)Param["a"];
            var e = (bool)Param["e"];
            var funnel = (bool)Param["F"];
            var reverse = (bool)Param["r"];

            var nv = new GH_Vector();
            double u1, v1, u2, v2;
            u1 = u2 = v1 = v2 = 0.0d;

            for (int i = 0; i < points.Count; i++)
            {
                foreach (var pl in planes)
                {
                    var o = pl.Value.Origin;
                    var currP = points[i].Value;

                    if (surface.IsValid)
                    {
                        if (!e)
                        {
                            surface.Face.ClosestPoint(currP, out u1, out v1);
                            var surfPl = new Plane(currP, surface.Face.NormalAt(u1, v1));

                            Point3d remap;
                            surfPl.RemapToPlaneSpace(pl.Value.Origin, out remap);

                            var dir = surfPl.PointAt(remap.X, remap.Y) - surfPl.Origin;
                            dir.Unitize();

                            surface.Face.ClosestPoint(pl.Value.Origin, out u2, out v2);

                            Point2d uv1 = new Point2d(u1, v1);
                            Point2d uv2 = new Point2d(u2, v2);

                            var dis = uv1.DistanceTo(uv2);
                            dir *= (k / Math.Pow(dis, h));

                            var tan = Vector3d.CrossProduct(dir, surface.Face.NormalAt(u1, v1));
                            tan.Unitize();
                            tan *= a / dis;

                            Vector3d rotation = dir + tan;
                            rotation.Unitize();

                            Basis offCheck = new Basis(new GH_Point(currP + rotation));
                            if (!Algos.CheckIfOffSurface(offCheck, surface))
                                nv.Value += rotation;
                        }
                        else
                        {
                            surface.Face.ClosestPoint(currP, out u1, out v1);
                            surface.Face.ClosestPoint(o, out u2, out v2);

                            var p1 = new Point2d(u1, v1);
                            var p2 = new Point2d(u2, v2);

                            var c = surface.Face.ShortPath(p1, p2, 0.001d);
                            var v = c.TangentAtStart;

                            nv = new GH_Vector(v);
                            nv.Value.Unitize();
                            nv.Value *= (k / Math.Pow(c.GetLength(), 1d + h));

                            var sn = surface.Face.NormalAt(u1, v1);
                            var sncv = Vector3d.CrossProduct(sn, v);
                            sncv.Unitize();
                            sncv *= a / c.GetLength();

                            nv.Value += sncv;
                        }

                        if (reverse)
                        {
                            surface.Face.ClosestPoint(currP, out u1, out v1);
                            var surfN = surface.Face.NormalAt(u1, v1);

                            var v = Vector3d.CrossProduct(surfN, nv.Value);
                            v.Unitize();
                            v *= nv.Value.Length;
                            nv.Value = v;
                        }
                    }
                    else
                    {
                        nv = new GH_Vector(o - currP);
                        nv.Value.Unitize();
                        nv.Value *= (k / Math.Pow(currP.DistanceTo(o), 1d + h));

                        if (funnel)
                        {
                            Point3d outP;
                            pl.Value.RemapToPlaneSpace(currP, out outP);
                            nv.Value *= Math.Sign(outP.Z);
                        }

                        Point3d sign;
                        pl.Value.RemapToPlaneSpace(currP, out sign);

                        if (sign.X != 0 && sign.Y != 0)
                        {
                            Vector3d tan;
                            
                            tan = new Vector3d(-sign.Y, sign.X, 0);

                            var tanAtPl = pl.Value.PointAt(tan.X, tan.Y, 0);
                            var tanAtO = tanAtPl - o;

                            tanAtO *= a / Math.Pow(tan.Length, 2d + h);
                            nv.Value += tanAtO;

                        }                        
                    }

                    vectors[i].Value += nv.Value;
                }
            }
        }
    }
}
