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
