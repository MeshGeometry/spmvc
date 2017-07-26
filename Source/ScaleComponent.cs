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
    public class ScaleComponent : GH_Component
    {
        public ScaleComponent()
            : base("Scale", "Scale", "Scales the vectors logarithmically as a post-process step", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{03616063-FE9F-4E62-824C-6A74B7D7D956}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Scale; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_DoubleParam("Scale Factor", "S", "Amount to scale by", 1.0d);
            pManager.Register_DoubleParam("Fall Off Range", "F", "Distance to begin logarithmic scaling", 1000.0d);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Vector Addition Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var s = new GH_Number();
            var f = new GH_Number();

            if (DA.GetData(0, ref s) && s == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid scale factor. Operation canceled.");
                return;
            }

            if (s.Value < 0d || s.Value > 1.0d)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Scale Factor must be between 0 and 1. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref f) && f == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid fall off value. Operation canceled.");
                return;
            }

            var dynamic = new ScaleDynamic();
            dynamic.Param["S"] = s;
            dynamic.Param["F"] = f;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
