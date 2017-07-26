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

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class OpenClosedCurveSorter : GH_Component
    {
        public OpenClosedCurveSorter()
            : base("Open/Closed Curve Sorter", "CrvSt", "Sorts a list of points representing curves into open and closed lists", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{FF43611A-404B-4257-96A3-3812F20145A9}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Closed_or_Open; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Curve Points", "P", "List of points representing a curve", GH_ParamAccess.list);
        }
 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Open Curve Set", "OC", "The list of points representing an open curve");
            pManager.Register_PointParam("Closed Curve Set", "CC", "The list of points representing a closed curve");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var points = new List<GH_Point>();

            List<GH_Point> open = null;
            List<GH_Point> closed = null;

            // gather and validate inputs
            if (DA.GetDataList(0, points) && points == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid points list.  Operation canceled.");
                return;
            }

            if (points.Count > 1)
            {
                if (points[0].Value == points[points.Count - 1].Value)
                {
                    closed = points;
                    closed.RemoveAt(points.Count - 1);
                }
                else
                    open = points;
            }

            DA.SetDataList(0, open);
            DA.SetDataList(1, closed);
        }
   }
}
