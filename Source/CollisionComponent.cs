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
