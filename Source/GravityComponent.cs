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
