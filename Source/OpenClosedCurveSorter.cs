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
