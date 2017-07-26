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
using System.Drawing;
using System.Windows.Forms;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

#if false

namespace GrasshopperCs
{
    public class DiscontinuityCheckComponent : GH_Component
    {
        public DiscontinuityCheckComponent()
            : base("Discontinuity Point Check", "DPC", "Output a 3d point for each discontinuity on a given surface", "SPM", "Util")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{BB6A0811-2C91-49E1-A486-861BADB2AEA1}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Interpolation; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_SurfaceParam("Surface", "S", "Surface to check for discontinuities.");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Points", "P", "List of 3d points that are on discontinuities of the surface");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var surface = new GH_Surface();

            if (DA.GetData(0, ref surface) && surface == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid surface. Operation canceled.");
                return;
            }

            var output = new List<GH_Point>();

            Continuity c = Continuity.C0_continuous | Continuity.C1_continuous | Continuity.C2_continuous |
                            Continuity.C0_locus_continuous | Continuity.C1_locus_continuous | Continuity.C2_locus_continuous |
                            Continuity.Cinfinity_continuous;

            // u direction
            var u = 0.0d;
            var v = 0.0d;

            surface.Face.GetNextDiscontinuity(1, c, 0, 1, out u);
            output.Add(new GH_Point(surface.Face.PointAt(u, 0)));
            output.Add(new GH_Point(surface.Face.PointAt(u, 1)));

            DA.SetDataList(0, output);
        }
    }
}

#endif