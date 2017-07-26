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

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class GravityComponent : GH_Component
    {
        public GravityComponent()
            : base("Gravity Dynamic", "Grav", "Creates gravity sink/sources at specified points", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{8A84F13F-F882-4E40-B9CB-64D0AFA2F688}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Gravity; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Singularity Points", "SP", "List of points, where each point relates to a vector (parallel to V)", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Exponent", "h", "h controls the fall-off of the gravity effect, higher is faster fall-off", 1.0d);
            pManager.Register_DoubleParam("Factor", "k", "Scalar multiple of the gravity force (positive for attraction, negative for repulsion)", 1.0d);
            pManager.Register_BooleanParam("Exact", "e", "Use exact geodesic distances to calculate path when on a surface, otherwise use a fast approximation", false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Gravity Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<GH_Point>();
            var h = new GH_Number();
            var k = new GH_Number();
            var e = new GH_Boolean();

            if (DA.GetDataList(0, points) && points == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid points list. Operation canceled.");
                return;
            }

            if (points.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Singularity point list must not be empty. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref h) && h == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid h value. Operation canceled.");
                return;
            }

            if (DA.GetData(2, ref k) && k == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid k value. Operation canceled.");
                return;
            }

            if (DA.GetData(3, ref e) && e == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid e value. Operation canceled.");
                return;
            }

            var dynamic = new GravityDynamic();
            dynamic.Param["SP"] = points;
            dynamic.Param["h"] = h.Value;
            dynamic.Param["k"] = k.Value;
            dynamic.Param["e"] = e.Value;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
