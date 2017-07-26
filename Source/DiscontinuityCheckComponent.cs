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