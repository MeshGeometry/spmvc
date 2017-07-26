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
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class VectorFieldCreator : GH_Component
    {
        public VectorFieldCreator()
            : base("Vector Field Creator", "VFC", "Modifies or creates a new vector field", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{1A74A063-225E-4FF0-9C3D-3A6A879FE7CE}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Vector_Field_Creator; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Point Field", "P", "List of points, where each point relates to a vector (parallel to V)", GH_ParamAccess.list);
            pManager.Register_VectorParam("Vector Field", "V", "List of vectors, where each vector relates to a point (parallel to P)", GH_ParamAccess.list);
            pManager.Register_GenericParam("Dynamics", "D", "List of dynamics to modify the vector field with", GH_ParamAccess.list);
            pManager.Register_SurfaceParam("Surface", "S", "Optional surface to create geodesic curves between points in space instead of direct lines");

            // vector field is optional, will create a field of 0 vectors if not present
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Point Field", "P", "Output points (parallel to V)");
            pManager.Register_VectorParam("Vector Field", "V", "Resultant field of vectors (parallel to P)");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<GH_Point>();
            var vectors = new List<GH_Vector>();
            var surface = new GH_Surface();
            var dynamicsWrapped = new List<GH_ObjectWrapper>();
            
            if (DA.GetDataList(0, points) && points == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid point list. Operation canceled.");
                return;
            }

            if (DA.GetDataList(1, vectors) && vectors == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector list. Operation canceled.");
                return;
            }

            // if vec field is empty, create parallel list of 0 vectors
            if (vectors.Count == 0)
                for (int i = 0; i < points.Count; i++)
                    vectors.Add(new GH_Vector());

            if (DA.GetDataList(2, dynamicsWrapped) && dynamicsWrapped == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid dynamics list. Operation canceled.");
                return;
            }

            if (DA.GetData(3, ref surface) && surface == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid surface. Operation canceled.");
                return;
            }

            var dynamics = new List<IDynamic>();
            foreach (var d in dynamicsWrapped)
                dynamics.Add(d.Value as IDynamic);
            Algos.SortDynamicsByPriority(dynamics);

            Algos.ClearDynamics(dynamics);

            foreach (var d in dynamics)
                Algos.ProcessDynamics(d, points, vectors, surface);

            Algos.RealignAccelerationVectors(dynamics, vectors);

            DA.SetDataList(0, points);
            DA.SetDataList(1, vectors);
        }
    }
}
