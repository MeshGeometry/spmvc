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
    public class CollisionComponent : GH_Component
    {
        public CollisionComponent()
            : base("Collision", "Col", "A post-processing dynamic which prevents collisions with a list of surfaces during an integration", "SPM", "Dynamics")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{25785133-D9A7-4043-863F-87391CBFCE7A}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Collision; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_SurfaceParam("Surfaces", "S", "List of surfaces to prevent collisions with", GH_ParamAccess.list);
            pManager.Register_DoubleParam("Restitution", "R", "Restitution of the surfaces (Bounce factor) to apply when colliding with a surface", 1.0d);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Collision Parameters", "D", "Parameters used to modify the vector field of the Vector Field Creater that this must be wired into");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var surfaces = new List<GH_Surface>();
            var scaleF = new GH_Number();

            if (DA.GetDataList(0, surfaces) && surfaces == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid surface list. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref scaleF) && scaleF == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid scale factor. Operation canceled.");
                return;
            }

            var dynamic = new CollisionDynamic();
            dynamic.Param["Surfs"] = surfaces;
            dynamic.Param["R"] = scaleF;

            DA.SetData(0, new GH_ObjectWrapper(dynamic));
        }
    }
}
