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

using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class StaticIntegration : GH_Component
    {
        private const int MAX_ITERATIONS = 2500;

        public StaticIntegration()
            : base("Vector Field Integration", "VFI", "Simulates a point travelling through a vector field", "SPM", "Integrate")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{8F81C07B-2A9C-47b7-8A75-475C524528A7}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.VectorField_Icon; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Field Points", "P", "List of points in the vector field (parallel to V)", GH_ParamAccess.list);
            pManager.Register_GenericParam("Field Vectors", "VT", "List of vectors in the vector field, or tensors representing a vector field (parallel to P)", GH_ParamAccess.list);
            pManager.Register_PointParam("Travelling Point", "TP", "Point to simulate travelling through the vector field");
            pManager.Register_GenericParam("Dynamics", "D", "List of dynamics to modify the vector field with", GH_ParamAccess.list);
            pManager.Register_GenericParam("Settings", "SS", "Static Settings object to customize integration parameters");
            
            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[3].Optional = true;
            pManager[4].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Travelled Points", "P", "The Travelling point after it has travelled [step] iterations");
            pManager.Register_VectorParam("Travelled Vectors", "V", "The resultant vectors at each step of the integration");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // inputs
            var points = new List<GH_Point>();
            var planes = new List<GH_Plane>();

            var vectors = new List<GH_Vector>();
            var traveller = new GH_Point();
            var settings = new GH_ObjectWrapper();
            var dynamicsWrapped = new List<GH_ObjectWrapper>();
            var dynamics = new List<IDynamic>();

            // outputs
            var linePoints = new List<GH_Point>();
            var lineVecs = new List<GH_Vector>();

            // gather and validate inputs
            if (DA.GetDataList(0, points) && points == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid points list. Operation canceled.");
                return;
            }

            if (DA.GetData(2, ref traveller) && traveller == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid travelling point. Operation canceled.");
                return;
            }

            if (DA.GetDataList(3, dynamicsWrapped) && dynamicsWrapped == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid dynamics list. Operation canceled.");
                return;
            }
            dynamics = (from d in dynamicsWrapped select d.Value as IDynamic).ToList();
            Algos.SortDynamicsByPriority(dynamics);

            // spm parameters component is optional, we use its defaults if it is not available
            StaticSettings spm_settings = new StaticSettings();

            if (DA.GetData(4, ref settings))
            {
                // if getdata succeeded but the settings var is null we had bad input
                if (settings == null)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid settings input. Operation canceled.");
                    return;
                }

                // otherwise cast from gh_objectwrapper and continue
                spm_settings = (StaticSettings)settings.Value;
            }

            var bases = new List<Basis>();
            var basesWrapper = new List<GH_ObjectWrapper>();

            // we need to get the vector field information after settings, for tensor settings
            if (spm_settings.tensor && (spm_settings.tensorDir >= 0 && spm_settings.tensorDir <= 2))
            {
                if (DA.GetDataList(1, basesWrapper) && basesWrapper == null)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid tensor field. Operation canceled.");
                    return;
                }

                for (int i = 0; i < basesWrapper.Count; i++)
                {
                    Basis b = basesWrapper[i].Value as Basis;
                    bases.Add(new Basis(points[i], b.Vectors));
                    bases[i].Axes = spm_settings.tensorAxes;
                }
            }
            else
            {
                if (DA.GetDataList(1, vectors) && vectors == null)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector list. Operation canceled.");
                    return;
                }
                
                if (vectors.Count != points.Count && vectors.Count != 0)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Vector list size mismatch with points list, they must be equal in length (or empty if you wish to integrate using just dynamics). Operation canceled.");
                    return;
                }

                // if vectors list is empty we'll populate it with empty vectors to match each point
                if (vectors.Count == 0)
                    foreach (var i in Enumerable.Range(0, points.Count))
                        vectors.Add(new GH_Vector());
                
                for (int i = 0; i < points.Count; i++)
                    bases.Add(new Basis(points[i], vectors[i]));
            }

            int steps = spm_settings.steps;
            if (steps == 0)
                steps = MAX_ITERATIONS;

            var startBasis = new Basis(traveller);
            var vecLast = new GH_Vector();
            double xy = 0;
            double yz = 0;
            double xz = 0;

            // add start point to output
            linePoints.Add(startBasis.Point);

            Algos.ClearDynamics(dynamics);

            // find each next point based on an averaging formula and iterate
            for (int i = 0; i < steps; i++)
            {
                bool add = false;
                var outBasis = new Basis();

                if (points.Count != 0 &&
                    !Algos.SampleForNextPoint(bases, traveller.Value, startBasis, vecLast, spm_settings, out outBasis))
                    break;

                if (dynamics.Count > 0)
                {
                    traveller = Algos.GetPointModifiedByDynamics(traveller, outBasis, dynamics, spm_settings);
                    outBasis.Point = traveller;
                }
                else
                    traveller = outBasis.Point;

                // this step must be done oustide of the regular halting checks as we must store the axes rotations
                if (spm_settings.windAngle != 0.0d && !vecLast.Value.IsZero && 
                    Algos.IsWoundPast(outBasis.Vector.Value, vecLast.Value, spm_settings.windAngle, ref xy, ref yz, ref xz))
                    break;

                var working = Algos.CheckHaltingConditions(traveller, startBasis, outBasis, vecLast.Value, out add, spm_settings);

                traveller = outBasis.Point;
                vecLast = outBasis.Vector;

                // cache the vector between start and start+1
                if (i == 0 && working)
                    startBasis.Vector.Value = traveller.Value - startBasis.Point.Value;

                if (add)
                {
                    linePoints.Add(traveller);
                    lineVecs.Add(vecLast);
                }

                if (!working)
                    break;
            }

            DA.SetDataList(0, linePoints);
            DA.SetDataList(1, lineVecs);
        }

    }
}

