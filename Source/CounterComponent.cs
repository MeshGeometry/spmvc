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
    public class CounterComponent : GH_Component
    {
        static int counter;   

        public CounterComponent()
            : base("Counter", "Counter", "A persistent counter that increases its count each iteration", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{F4B7C93A-6C2B-44A5-9A97-0007B517ECEB}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Timer; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_BooleanParam("Reset", "R", "Reset the counter to 0", false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_IntegerParam("Counter Value", "i", "Current value of the ticker");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var reset = new GH_Boolean();

            if (DA.GetData(0, ref reset) && reset == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid reset input. Operation canceled.");
                return;
            }

            if (reset.Value)
                counter = 0;
            else
                counter++;

            DA.SetData(0, counter);
        }
    }
}