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
    public class VortexComponent : GH_Component
    {
        public VortexComponent()
            : base("Vortex Dynamic", "Vortex", "Creates vortices inside a vector field at specified points (at the origins of the planar inputs)", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{98DE1D47-12C8-454E-93E7-79636B8BFC90}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Vortex; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PlaneParam("Plane", "Pl", "Plane for the vortex to operate on. The origin of the plane is the singularity point of the vortex", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Exponent", "h", "Distance from field where integration will halt. 0 to run until integration is complete", 1.0d);
            pManager.Register_DoubleParam("Factor", "k", "Scalar multiple of the gravity force (positive for attraction, negative for repulsion)", 1.0d);
            pManager.Register_DoubleParam("Rotational Factor", "a", "Scalar multiple of the effect the spiral causes on the vector field", 1.0d);
            pManager.Register_BooleanParam("Exact", "e", "Use exact geodesic distances to calculate spiral path when on a surface, otherwise use a fast approximation", false);
            pManager.Register_BooleanParam("Funnel", "F", "True if you want a funnel instead of a vortex. A funnel will attract on one side and shoot out the other", false);
            pManager.Register_BooleanParam("Reverse", "R", "True if you want to reverse the spiral when constrained to a surface", false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Vortex Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var pl = new List<GH_Plane>();
            var h = new GH_Number();
            var k = new GH_Number();
            var a = new GH_Number();
            var e = new GH_Boolean();
            var f = new GH_Boolean();
            var r = new GH_Boolean();

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

            if (DA.GetData(3, ref a) && a == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid a value. Operation canceled.");
                return;
            }

            if (DA.GetData(4, ref e) && e == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid e value. Operation canceled.");
                return;
            }

            if (DA.GetData(5, ref f) && f == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid funnel value. Operation canceled.");
                return;
            }

            if (DA.GetData(6, ref r) && r == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid reverse value. Operation canceled.");
                return;
            }

            var dynamic = new VortexDynamic();
            dynamic.Param["Pl"] = pl;
            dynamic.Param["h"] = h.Value;
            dynamic.Param["k"] = k.Value;
            dynamic.Param["a"] = a.Value;
            dynamic.Param["e"] = e.Value;
            dynamic.Param["F"] = f.Value;
            dynamic.Param["r"] = r.Value;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
