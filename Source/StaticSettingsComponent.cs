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
using Grasshopper.Kernel.Attributes;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class StaticSettings
    {
        public int steps;
        public double tolerance;
        public bool stop;
        public double avgRadius;
        public double snapTol;
        public double snapAngle;
        public double windAngle;
        public bool tensor;
        public int tensorDir;
        public bool lineCont;
        public List<int> tensorAxes;

        public GH_Brep bounds;
        public GH_Surface surface;

        // we set up some defaults here to make it easier for the components
        // they will use these defaults if they are not passed a paramaters object
        public StaticSettings()
        {
            steps = 0;
            tolerance = 10.0d;
            stop = true;
            avgRadius = 0.0d;
            snapTol = 0.0d;
            snapAngle = 0.0d;
            windAngle = 0.0d;
            tensor = false;
            tensorDir = 0;
            lineCont = false;

            tensorAxes = new List<int>(new [] {0, 1});

            surface = new GH_Surface();
            bounds = new GH_Brep();
        }
    }

    public class StaticSettingsComponent : GH_Component
    {
        public StaticSettingsComponent()
            : base("Static Settings", "SPM Static", "Settings to configure a static SPM vector field integration component", "SPM", "Integrate")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{D49F709E-546C-4186-9D2C-1C8D36DEF5BF}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Static_Settings; }
        }
        
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_IntegerParam("Step count", "Steps", "Number of steps to simulate per run of the simulation. 0 to run until integration is complete", 0);
            pManager.Register_DoubleParam("Tolerance", "Tol", "Distance from field where integration will halt. 0 to run until integration is complete", 10.0d);
            pManager.Register_BooleanParam("Stop", "Stop", "True if the integration will stop when the integration line has stopped or has wound past the stop winding angle", true);
            pManager.Register_DoubleParam("Winding Stop Angle", "StopA", "The winding angle is used to stop the integration if it winds around the set angle (radians). Used to stop the integration at singularities or orbits", 0.0d);
            pManager.Register_DoubleParam("Interpolation Radius", "IntrpR", "Radius to use in vector interpolation method. 0 to use closest vector only", 0.0d);
            pManager.Register_BRepParam("Bounding Brep", "Bound", "Bounding area to contain the integration within");
            pManager.Register_DoubleParam("Snapping Tolerance", "SnapT", "Tolerance range to test for snapping to the start point, used to detect and close orbits. 0 to disable check", 0.0d);
            pManager.Register_DoubleParam("Snapping Minimum Angle", "SnapA", "If snapping tolerance is set, this sets the minimum difference in angles before a snap occurs. 0 to disable check. Max is 1.", 0.0d);
            pManager.Register_SurfaceParam("Surface", "Surf", "Surface to constrain the integration to, the intergration will snap to this surface as it continues");
            pManager.Register_BooleanParam("Tensor Field", "Tensor", "Set to true if your input is actually a tensor field (list of planes)", false);
            pManager.Register_IntegerParam("Tensor Direction", "TensDir", "Which direction to integrate across if we're using a tensor field (0 = X, 1 = Y, 2 = Z)", 0.0d);
            pManager.Register_IntegerParam("Tensor Axes", "TensAxs", "Which Axes are used in the line continuity check, if enabled (0 = X, 1 = Y, 2 = Z)", GH_ParamAccess.list);
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
            pManager[9].Optional = true;
            pManager[10].Optional = true;
            pManager[11].Optional = true;
            pManager[12].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Settings Output", "SS", "Settings output to wire to a SPM vector field integration component");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var steps = new GH_Integer();
            var tolerance = new GH_Number();
            var stop = new GH_Boolean();
            var windAngle = new GH_Number();
            var avgRadius = new GH_Number();
            var bounds = new GH_Brep();
            var snapTol = new GH_Number();
            var snapAngle = new GH_Number();
            var surface = new GH_Surface();
            var tensor = new GH_Boolean();
            var tensorDir = new GH_Integer();
            var tensorAxes = new List<GH_Integer>();
            var lineCont = new GH_Boolean();

            if (DA.GetData(0, ref steps) && steps == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid step count. Operation canceled.");
                return;
            }

            if (DA.GetData(1, ref tolerance) && tolerance == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tolerance. Operation canceled.");
                return;
            }

            if (DA.GetData(2, ref stop) && stop == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid stop value. Operation canceled.");
                return;
            }

            if (DA.GetData(3, ref windAngle) && windAngle == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tolerance. Operation canceled.");
                return;
            }

            if (DA.GetData(4, ref avgRadius) && avgRadius == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid interpolation radius value. Operation canceled.");
                return;
            }

            if (DA.GetData(5, ref bounds) && bounds == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid bounding box. Operation canceled.");
                return;
            }

            if (DA.GetData(6, ref snapTol) && snapTol == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid snap tolerance. Operation canceled.");
                return;
            }

            if (DA.GetData(7, ref snapAngle) && snapAngle == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid snapping angle. Operation canceled.");
                return;
            }

            if (DA.GetData(8, ref surface) && surface == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid surface constraint. Operation canceled.");
                return;
            }

            if (DA.GetData(9, ref tensor) && tensor == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tensor input. Operation canceled.");
                return;
            }

            if (DA.GetData(10, ref tensorDir) && tensorDir == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tensor direction. Operation canceled.");
                return;
            }

            if (tensorDir.Value < 0 || tensorDir.Value > 2)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tensor direction. Valid input is 0-2. Operation canceled.");
                return;
            }

            if (DA.GetDataList(11, tensorAxes) && tensorAxes == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tensor direction. Operation canceled.");
                return;
            }

            if (DA.GetData(12, ref lineCont) && lineCont == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid line continuity value. Operation canceled.");
                return;
            }

            StaticSettings settings = new StaticSettings();
            
            settings.steps = steps.Value;
            settings.stop = stop.Value;
            settings.windAngle = windAngle.Value;
            settings.tolerance = tolerance.Value;
            settings.avgRadius = avgRadius.Value;
            settings.bounds = bounds;
            settings.snapTol = snapTol.Value;
            settings.snapAngle = snapAngle.Value;
            settings.surface = surface;
            settings.tensor = tensor.Value;
            settings.tensorDir = tensorDir.Value;
            settings.lineCont = lineCont.Value;
                       
            if (tensorAxes.Count == 0)
            {
                settings.tensorAxes.Add(0);
                settings.tensorAxes.Add(1);
            }
            else
            {
                foreach (var axis in tensorAxes)
                    settings.tensorAxes.Add(axis.Value);
            }

            if (settings.tensor == false)
                settings.tensorDir = -1;

            var output = new GH_ObjectWrapper(settings);

            DA.SetData(0, output);
        }
    }
}
