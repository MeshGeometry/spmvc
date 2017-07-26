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
