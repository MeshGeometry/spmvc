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