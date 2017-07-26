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
