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
