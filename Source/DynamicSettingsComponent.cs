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
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class DynamicSettings
    {
        public bool respawn;
        public int newSpawn;
        public int rndSpawn;
        public double avgRadius;
        public double snapTol;
        public double snapAngle;
        public double windAngle;
        public bool lineCont;

        public GH_Brep bounds;
        public GH_Surface surface;

        // we set up some defaults here to make it easier for the components
        // they will use these defaults if they are not passed a paramaters object
        public DynamicSettings()
        {
            respawn = true;
            newSpawn = 0;
            rndSpawn = 0;

            avgRadius = 0.0d;
            snapTol = 0.0d;
            snapAngle = 0.0d;
            windAngle = 0.0d;
            lineCont = false;
            
            surface = new GH_Surface();
            bounds = new GH_Brep();
        }
    }

    public class DynamicSettingsComponent : GH_Component
    {
        public DynamicSettingsComponent()
            : base("Dynamic Settings", "SPM Dynamic", "Settings to configure a dynamic SPM vector field integration component", "SPM", "Integrate")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{B6193B15-4C1D-46F3-B0CA-EE54D59CC032}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Dynamic_Settings; }
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_BooleanParam("Respawn", "Respawn", "True if the particles should be respawned when they reach the end of their lifetimes", true);
            pManager.Register_IntegerParam("New Spawn Step Count", "NewSpK", "Steps between spawning a new set of particles at the start points (0 to disable)", 0);
            pManager.Register_IntegerParam("Randomize New Spawn", "RndSp", "Randomly add or subtract up to this number from the new spawn time to create random distributions of particles (0 to disable)", 0);
            pManager.Register_DoubleParam("Interpolation Radius", "IntrpR", "Radius to use in vector interpolation method. 0 to use closest vector only", 0.0d);
            pManager.Register_BRepParam("Bounding Brep", "Bound", "Bounding area to contain the integration within");
            pManager.Register_DoubleParam("Snapping Tolerance", "SnapT", "Tolerance range to test for snapping to the start point, used to detect and close orbits. 0 to disable check", 0.0d);
            pManager.Register_DoubleParam("Snapping Minimum Angle", "SnapA", "If snapping tolerance is set, this sets the minimum difference in angles before a snap occurs. 0 to disable check. Max is 1.", 0.0d);
            pManager.Register_SurfaceParam("Surface", "Surf", "Surface to constrain the integration to, the intergration will snap to this surface as it continues");
            pManager.Register_BooleanParam("Line Continuity", "LCont", "Force lines to follow a straighter path if one is available", false);
            
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
            pManager[5].Optional = true;
            pManager[6].Optional = true;
            pManager[7].Optional = true;
            pManager[8].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Settings Output", "DS", "Settings output to wire to a SPM vector field integration component");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var respawn = new GH_Boolean();
            var newSpawn = new GH_Integer();
            var rndSpawn = new GH_Integer();

            var avgRadius = new GH_Number();
            var bounds = new GH_Brep();
            var snapTol = new GH_Number();
            var snapAngle = new GH_Number();
            var surface = new GH_Surface();
            var lineCont = new GH_Boolean();

            if (DA.GetData(0, ref respawn) && respawn == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid respawn value. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref newSpawn) && newSpawn == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid new spawn value. Operation canceled.");
                return;
            }

            if (DA.GetData(2, ref rndSpawn) && rndSpawn == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid new spawn randomizing value. Operation canceled.");
                return;
            }

            if (DA.GetData(3, ref avgRadius) && avgRadius == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid interpolation radius value. Operation canceled.");
                return;
            }

            if (DA.GetData(4, ref bounds) && bounds == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid bounding box. Operation canceled.");
                return;
            }

            if (DA.GetData(5, ref snapTol) && snapTol == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid snap tolerance. Operation canceled.");
                return;
            }

            if (DA.GetData(6, ref snapAngle) && snapAngle == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid snapping angle. Operation canceled.");
                return;
            }

            if (DA.GetData(7, ref surface) && surface == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid surface constraint. Operation canceled.");
                return;
            }

            if (DA.GetData(8, ref lineCont) && lineCont == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid line continuity value. Operation canceled.");
                return;
            }

            DynamicSettings settings = new DynamicSettings();

            settings.respawn = respawn.Value;
            settings.newSpawn = newSpawn.Value;
            settings.rndSpawn = rndSpawn.Value;
            settings.avgRadius = avgRadius.Value;
            settings.bounds = bounds;
            settings.snapTol = snapTol.Value;
            settings.snapAngle = snapAngle.Value;
            settings.surface = surface;
            settings.lineCont = lineCont.Value;
           
            var output = new GH_ObjectWrapper(settings);

            DA.SetData(0, output);
        }
    }
}
