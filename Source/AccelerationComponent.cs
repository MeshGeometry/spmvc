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
    public class AccelerationComponent : GH_Component
    {
        public AccelerationComponent()
            : base("Acceleration", "Acc", "Provides acceleration to a series of dynamics, which will be applied throughout the intergration", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{00A2BB2E-F780-4B2B-ABF2-5D860461B704}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Acceleration; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_GenericParam("Dynamics", "D", "List of dynames to accelerate", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Drag", "Dg", "Drag is used to modify the acceleration speed over time", 1.0d);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Acceleration Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var dynamics = new List<GH_ObjectWrapper>();
            var drag = new GH_Number();

            if (DA.GetDataList(0, dynamics) && dynamics == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid dynamics list. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref drag) && drag== null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid drag value. Operation canceled.");
                return;
            }

            if (drag.Value < 0 || drag.Value > 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Drag must be between 0 and 1. Operation canceled.");
                return;
            }

            var dynamic = new AccelerationDynamic();
            dynamic.Param["D"] = dynamics;
            dynamic.Param["Dg"] = drag;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}

