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
    public class DynamicIntegration : GH_Component
    {
        static List<GH_Point> moving = null;
        static Basis[] startBasis = null;
        static GH_Vector[] lastVecs = null;

        static double[] xy = null;
        static double[] yz = null;
        static double[] xz = null;

        public DynamicIntegration()
            : base("Dynamic Vector Field Integration Simulation", "VFIS", "Dynamically simulates a point travelling through a vector field", "SPM", "Sim")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{BB98017A-3520-4a12-8FD2-D1B2080DBAC9}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.VectorFieldSim_Icon; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Field Points", "P", "List of points in the vector field (parallel to V)", GH_ParamAccess.list);
            pManager.Register_VectorParam("Field Vectors", "V", "List of vectors in the vector field (parallel to P)", GH_ParamAccess.list);
            pManager.Register_PointParam("Start Points", "SP", "Points to simulate travelling through the vector field", GH_ParamAccess.list);
            pManager.Register_GenericParam("Settings", "S", "Settings object to customize integration parameters");
            pManager[3].Optional = true;
            pManager.Register_BooleanParam("Reset simulation", "R", "Reset the simulation to the starting point", false);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Travelled Points", "P", "The Travelling points after they have travelled [step] iterations");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // inputs
            var points = new List<GH_Point>();
            var vectors = new List<GH_Vector>();
            var startPoints = new List<GH_Point>();
            var reset = new GH_Boolean();

            var settings = new GH_ObjectWrapper();

            if (DA.GetDataList(0, points) && points == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid points list. Operation canceled.");
                return;
            }

            if (DA.GetDataList(1, vectors) && vectors == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector list. Operation canceled.");
                return;
            }

            if (vectors.Count != points.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Vector list size mismatch with points list, they must be equal in length. Operation canceled.");
                return;
            }

            if (DA.GetDataList(2, startPoints) && startPoints == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid travelling points. Operation canceled.");
                return;
            }

            // spm parameters component is optional, we use its defaults if it is not available
            SPM_Parameters spm_settings = new SPM_Parameters();

            if (DA.GetData(3, ref settings))
            {
                // if getdata succeeded but the settings var is null we had bad input
                if (settings == null)
                {
                    this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid settings input. Operation canceled.");
                    return;
                }

                // otherwise cast from gh_objectwrapper and continue
                spm_settings = (SPM_Parameters)settings.Value;
            }

            if (DA.GetData(4, ref reset) && reset == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid reset input. Operation canceled.");
                return;
            }

            if (startBasis == null || reset.Value)
            {
                var count = startPoints.Count;

                startBasis = new Basis[count];
                lastVecs = new GH_Vector[count];

                xy = new double[count];
                yz = new double[count];
                xz = new double[count];
                
                for (int i = 0; i < count; i++)
                {
                    startBasis[i] = new Basis(startPoints[i]);
                    lastVecs[i] = new GH_Vector();
                    xy[i] = 0;
                    yz[i] = 0;
                    xz[i] = 0;
                }
            }

            if (moving == null || reset.Value)
            {
                moving = startPoints;
                return;
            }

            int steps = spm_settings.steps;
            if (steps == 0)
                steps = 1;

            var bases = new List<Basis>();
            for (int i = 0; i < points.Count; i++)
                bases.Add(new Basis(points[i], vectors[i]));

            // find each next point based on an averaging formula and iterate
            
            for (int i = 0; i < startPoints.Count; i++)
            {
                for (int j = 0; j < steps; j++)
                {
                    bool add = false;

                    var outBasis = new Basis();

                    bool working = Algos.SampleForNextPoint(bases, moving[i].Value, startBasis[i], lastVecs[i], spm_settings, out outBasis, out add);

                    if (spm_settings.stop && spm_settings.windAngle != 0.0d)
                    {
                        if (!lastVecs[i].Value.IsZero)
                        {
                            double cxy = xy[i];
                            double cyz = yz[i];
                            double cxz = xz[i];

                            if (Algos.IsWoundPast(outBasis.Vector.Value, lastVecs[i].Value, spm_settings.windAngle, ref cxy, ref cyz, ref cxz))
                                break;

                            xy[i] = cxy;
                            yz[i] = cyz;
                            xz[i] = cxz;
                        }
                    }
                    
                    lastVecs[i] = outBasis.Vector;

                    if (working && startBasis[i].Vector.Value.IsZero)
                        startBasis[i].Vector.Value = moving[i].Value - startBasis[i].Point.Value;

                    if (add)
                        moving[i] = outBasis.Point;

                    if (!working)
                        moving[i] = startPoints[i];
                }
            }

            DA.SetDataList(0, moving);
        }

    }
}