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

