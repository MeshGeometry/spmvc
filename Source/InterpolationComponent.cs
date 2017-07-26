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
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace GrasshopperCs
{
    public class InterpolationComponent : GH_Component
    {
        public InterpolationComponent()
            : base("Point/Vector Interpolation", "Pt Interp", "Interpolate an N-dimensional point based on surrounding M-dimensional vectors", "SPM", "Utilities")
        {
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{DDFB6A33-82D4-40AB-B9BB-83111A8AF030}"); }
        }

        protected override Bitmap Internal_Icon_24x24
        {
            get { return Properties.Resources.Interpolation3d; }
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Sample Points", "SP", "Tree of lists representing N-dimensional points. These points must be parallel to the list of incoming Sample Vectors.", GH_ParamAccess.tree);
            pManager.Register_VectorParam("Sample Vectors", "SV", "Tree of lists representing M-dimensional vectors. These vectors must be parallel to the list of incoming Sample Points.", GH_ParamAccess.tree);
            pManager.Register_PointParam("Test Points To Interpolate", "TP", "Tree of lists representing N-dimensional points, these points will be the reference points for each interpolation", GH_ParamAccess.tree);
            pManager.Register_DoubleParam("Radius", "R", "Radius from each test point to use in the interpolation", 0.0d);
            pManager.Register_DoubleParam("Interpolation Strength", "p", "1 for linear, n for higher strengths", 2.0d);
        }
 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_VectorParam("Vectors", "V", "Tree of lists representing N-dimensional vectors");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            var samplePoints = new List<Vector>();
            var sampleVectors = new List<Vector>();
            var testPoints = new List<Vector>();
            var radius = new GH_Number();
            var p = new GH_Number();

            var pdt = new GH_Structure<GH_Point>();
            var vdt = new GH_Structure<GH_Vector>();
            var tpdt = new GH_Structure<GH_Point>();

            // gather and validate inputs
            if (DA.GetDataTree(0, out pdt) && pdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid sample point tree. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetDataTree(1, out vdt) && vdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid vector tree. Operation canceled.");
                return;
            }

            if (pdt.Branches.Count != vdt.Branches.Count)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Point and Vector trees must be parallel (same count). Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetDataTree(2, out tpdt) && tpdt == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid test point tree. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetData(3, ref radius) && radius == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid radius value. Operation canceled.");
                return;
            }

            // gather and validate inputs
            if (DA.GetData(4, ref p) && p == null)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Invalid strength value. Operation canceled.");
                return;
            }

            // collect input Vectors
            samplePoints = GetVectorsFromTree(pdt);
            sampleVectors = GetVectorsFromTree(vdt);
            testPoints = GetVectorsFromTree(tpdt);

            // if r is 0 calculate a sane default
            double r = radius.Value;
            if (r == 0.0d)
                r = GetDefaultRadius(samplePoints);

            // get interpolated vectors
            var interpolated = new List<Vector>();
            foreach (var tp in testPoints)
            {
                var withinRadius = GetAllSampleIndicesWithinRadius(tp, samplePoints, r).ToList();
                interpolated.Add(GetInterpolatedVector(tp, samplePoints, sampleVectors, withinRadius, r, p.Value));
            }

            // set interpolated vectors as output
            var outputTree = new GH_Structure<GH_Point>();

            for (int i = 0; i < interpolated.Count; i++)
            {
                var v = interpolated[i];
                var points = new List<GH_Point>();
                
                for (int j = 0; j < v.Rank / 3; j++)
                    points.Add(new GH_Point(new Rhino.Geometry.Point3d(v[j * 3], v[j * 3 + 1], v[j * 3 + 2])));

                outputTree.AppendRange(points, new GH_Path(i));
            }
            
            DA.SetDataTree(0, outputTree);
        }

        private double GetDefaultRadius(List<Vector> samplePoints)
        {
            var output = 0.0d;
            var avgPt = new Vector(samplePoints[0].Rank);

            // calculate average point
            foreach (var p in samplePoints)
                avgPt += p;
            avgPt /= samplePoints.Count;

            // calculate max distance from average point to sample points
            foreach (var p in samplePoints)
            {
                var d = Vector.DistanceBetween(avgPt, p);
                if (d > output)
                    output = d;
            }

            return output;
        }

        private Vector GetInterpolatedVector(Vector tp, List<Vector> samplePoints, List<Vector> sampleVectors, List<int> sampleIndices, double radius, double p)
        {
            var output = new Vector(sampleVectors[0].Rank);

            double sumWeight = 0.0d;
            var weightedVecs = new Vector[sampleIndices.Count];

            for (int i = 0; i < sampleIndices.Count; i++)
            {
                int idx = sampleIndices[i];
                double d = Vector.DistanceBetween(tp, samplePoints[idx]);
                
                if (d == 0)
                    return sampleVectors[idx];

                var w = Math.Pow((radius - d) / (radius * d), p);

                weightedVecs[i] = sampleVectors[idx] * w;
                sumWeight += w;
            }

            foreach (var v in weightedVecs)
                output += v / sumWeight;

            return output;
        }

        private IEnumerable<int> GetAllSampleIndicesWithinRadius(Vector tp, List<Vector> samplePoints, double radius)
        {
            for (int i = 0; i < samplePoints.Count; i++)
                if (Vector.DistanceBetween(tp, samplePoints[i]) < radius)
                    yield return i;
        }

        private List<Vector> GetVectorsFromTree<T>(GH_Structure<T> tree) where T : IGH_Goo
        {
            var output = new List<Vector>();

            for (int i = 0; i < tree.Branches.Count; i++)
            {
                var branchItems = tree[i].Count * 3;
                output.Add(new Vector(branchItems));

                for (int j = 0; j < branchItems; j++)
                {
                    if (tree[i][j / 3] is GH_Point)
                    {
                        GH_Point p = tree[i][j / 3] as GH_Point;
                        output[i][j] = p.Value[j % 3];
                    }
                    else if (tree[i][j / 3] is GH_Vector)
                    {
                        GH_Vector p = tree[i][j / 3] as GH_Vector;
                        output[i][j] = p.Value[j % 3];
                    }

                    output[i].SetMagnitude();
                }
            }

            return output;
        }
    }
}
