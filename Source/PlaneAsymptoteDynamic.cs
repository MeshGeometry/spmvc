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
    class PlaneAsymptoteDynamic : IDynamic
    {
        public bool PostProcess { get { return false; } }
        public Param Param { get; set; }
        public bool Accelerated { get { return false; } }

        public PlaneAsymptoteDynamic()
        {
            Param = new Param();
        }

        public void Process(List<GH_Point> points, List<GH_Vector> vectors, GH_Surface surface)
        {
            var planes = Param["Pl"] as List<GH_Plane>;
            var k1 = (double)Param["k1"];
            var k2 = (double)Param["k2"];
            
            for (int i = 0; i < points.Count; i++)
            {
                foreach (var pl in planes)
                {
                    Point3d remap;
                    pl.Value.RemapToPlaneSpace(points[i].Value, out remap);

                    var denom = Math.Sqrt(Math.Pow(remap.X, 2) + Math.Pow(remap.Y, 2));
                    Point3d direction = new Point3d(remap.X / denom,
                                                    remap.Y / denom,
                                                    (-Math.Sign(remap.Z) * Math.Abs(remap.Z)) / Math.Pow(denom, k2));
                    direction = pl.Value.PointAt(direction.X, direction.Y, direction.Z);
                    
                    var movement = direction - pl.Value.Origin;
                    movement *= k1;
                    vectors[i].Value += movement;
                }
            }
        }
    }
}
