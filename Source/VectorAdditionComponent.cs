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
    public class VectorAdditionComponent : GH_Component
    {
        public VectorAdditionComponent()
            : base("Vector Addition", "VAdd", "Adds a vector to each vector in a vector field", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{C9835FBE-9FB6-451D-8E09-2A9C308EE968}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.VectorAdd; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_VectorParam("Vector", "V", "Vector to add to each vector in the field");
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Vector Addition Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var v = new GH_Vector();

            if (DA.GetData(0, ref v) && v == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector. Operation canceled.");
                return;
            }

            var dynamic = new VectorAdditionDynamic();
            dynamic.Param["V"] = v;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
