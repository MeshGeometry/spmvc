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
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class TensorComponent : GH_Component
    {
        public TensorComponent()
            : base("Tensor", "Tensor", "A Tensor component represents a 3d frame or basis, such as a plane", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{76BD305C-A2D5-4C4B-AF0C-108DF64FCDF1}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Tensor_Creator; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_VectorParam("X Axis", "X", "List of vectors representing the X axes of the tensors", GH_ParamAccess.list);
            pManager.Register_VectorParam("Y Axis", "Y", "List of vectors representing the Y axes of the tensors", GH_ParamAccess.list);
            pManager.Register_VectorParam("Z Axis", "Z", "List of vectors representing the Z axes of the tensors", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Tensors", "T", "The list of resulting tensors");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var tensors = new List<Basis>();

            var xaxis = new List<GH_Vector>();
            var yaxis = new List<GH_Vector>();
            var zaxis = new List<GH_Vector>();

            if (DA.GetDataList(0, xaxis) && xaxis == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid X axis vector list. Operation canceled.");
                return;
            }

            if (DA.GetDataList(1, yaxis) && yaxis == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Y axis vector list. Operation canceled.");
                return;
            }

            if (DA.GetDataList(2, zaxis) && zaxis == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid Z axis vector list. Operation canceled.");
                return;
            }

            var xc = xaxis.Count;
            var yc = yaxis.Count;
            var zc = zaxis.Count;

            // the lists can either be length 0, or the same length as every other vector list
            var xym = Math.Max(xc, yc);
            var max = Math.Max(xym, zc);

            if (xc != 0 && xc != max)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "X list is not parallel to Y and Z. Operation canceled.");
                return;
            }

            if (yc != 0 && yc != max)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Y list is not parallel to X and Z. Operation canceled.");
                return;
            }

            if (zc != 0 && zc != max)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Z list is not parallel to X and Y. Operation canceled.");
                return;
            }

            for (int i = 0; i < max; i++)
            {
                Basis tensor = new Basis();

                if (xc != 0)
                    tensor.Vectors[0].Value = xaxis[i].Value;
                else
                    tensor.Vectors[0].Value = new Rhino.Geometry.Vector3d();

                if (yc != 0)
                    tensor.Vectors[1].Value = yaxis[i].Value;
                else
                    tensor.Vectors[1].Value = new Rhino.Geometry.Vector3d();

                if (zc != 0)
                    tensor.Vectors[2].Value = zaxis[i].Value;
                else
                    tensor.Vectors[2].Value = new Rhino.Geometry.Vector3d();

                tensors.Add(tensor);
            }

            var output = new List<GH_ObjectWrapper>();
            foreach (var t in tensors)
                output.Add(new GH_ObjectWrapper(t));

            DA.SetDataList(0, output);
        }
    }
}
