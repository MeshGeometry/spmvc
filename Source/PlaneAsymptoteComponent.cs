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
    public class PlaneAsymptoteComponent : GH_Component
    {
        public PlaneAsymptoteComponent()
            : base("Plane Asymptote Dynamic", "PlAsym", "Creates a plane saddle point at specified points (at the origins of the planar inputs)", "SPM", "Dynamics")
        {
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{B3B79141-C9CE-4378-B99C-901D19050BD1}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Plane_Asymptote; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PlaneParam("Plane", "Pl", "Plane for the vortex to operate on. The origin of the plane is the singularity point of the vortex", GH_ParamAccess.list);
            pManager.Register_DoubleParam("k1", "k1", "Scalar multiple of the effect of the plane", 1.0d);
            pManager.Register_DoubleParam("k2", "k2", "Exponent controlling the curves steepness", 0.5d);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Plane Asymptote Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pl = new List<GH_Plane>();
            var k1 = new GH_Number();
            var k2 = new GH_Number();

            if (DA.GetDataList(0, pl) && pl == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid pl value. Operation canceled.");
                return;
            }

            if (pl.Count == 0)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Singularity point list must not be empty. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref k1) && k1 == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid k value. Operation canceled.");
                return;
            }

            if (DA.GetData(2, ref k2) && k2 == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid k value. Operation canceled.");
                return;
            }

            var dynamic = new PlaneAsymptoteDynamic();
            dynamic.Param["Pl"] = pl;
            dynamic.Param["k1"] = k1.Value;
            dynamic.Param["k2"] = k2.Value;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
